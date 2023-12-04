using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace UntoldByte.GAINS.Editor
{
    public static class TextureUtilities
    {
        internal static Texture2D[] ToTexture2D(RenderTexture[] renderTextures)
        {
            int length = renderTextures.Length;
            Texture2D[] texture2Ds = new Texture2D[length];
            for (int i = 0; i < length; i++)
            {
                texture2Ds[i] = ToTexture2D(renderTextures[i]);
            }
            return texture2Ds;
        }

        internal static Texture2D ToTexture2D(RenderTexture renderTexture)
        {
            TextureFormat textureFormat = GetEquivalentTexture2DFormat(renderTexture.format);
            Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, textureFormat, false);
            var previousActiveRenderTexture = RenderTexture.active;
            RenderTexture.active = renderTexture;
            tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            tex.Apply();
            RenderTexture.active = previousActiveRenderTexture;
            return tex;
        }

        internal static RenderTexture ToRenderTexture(Texture2D texture2D)
        {
            RenderTextureFormat renderTextureFormat = GetEquivalentRenderTextureFormat(texture2D.format);
            RenderTexture renderTexture = new RenderTexture(texture2D.width, texture2D.height, 24, renderTextureFormat);
            var previousActiveRenderTexture = RenderTexture.active;
            RenderTexture.active = renderTexture;
            Graphics.Blit(texture2D, renderTexture);
            RenderTexture.active = previousActiveRenderTexture;
            return renderTexture;
        }

        internal static TextureFormat GetEquivalentTexture2DFormat(RenderTextureFormat renderTextureFormat)
        {
            if (renderTextureFormat == RenderTextureFormat.ARGB32)
                return TextureFormat.RGBA32;
            if (renderTextureFormat == RenderTextureFormat.ARGBHalf)
                return TextureFormat.RGBAHalf;
            if (renderTextureFormat == RenderTextureFormat.RFloat)
                return TextureFormat.RFloat;
            if (renderTextureFormat == RenderTextureFormat.RGFloat)
                return TextureFormat.RGFloat;
            if (renderTextureFormat == RenderTextureFormat.ARGBFloat)
                return TextureFormat.RGBAFloat;
            return TextureFormat.ARGB32;
        }

        internal static RenderTextureFormat GetEquivalentRenderTextureFormat(TextureFormat textureFormat)
        {
            if (textureFormat == TextureFormat.RGBA32)
                return RenderTextureFormat.ARGB32;
            if (textureFormat == TextureFormat.RGBAHalf)
                return RenderTextureFormat.ARGBHalf;
            if (textureFormat == TextureFormat.RFloat)
                return RenderTextureFormat.RFloat;
            if (textureFormat == TextureFormat.RGFloat)
                return RenderTextureFormat.RGFloat;
            if (textureFormat == TextureFormat.RGBAFloat)
                return RenderTextureFormat.ARGBFloat;
            return RenderTextureFormat.Default;
        }

        internal static Texture2D PackPrototypes(IEnumerable<IPrototype> prototypes, Color color = default)
        {
            return PackPrototypesBase(prototypes, p => p.Texture, color);
        }

        internal static Texture2D PackPrototypesSecondImage(IEnumerable<IPrototype> prototypes, Color color = default)
        {
            return PackPrototypesBase(prototypes, p => p.SecondTexture, color);
        }

        private static Texture2D PackPrototypesBase(IEnumerable<IPrototype> prototypes, System.Func<IPrototype, Texture2D> textureFunc, Color color = default)
        {
            Texture2D[] textures = prototypes.Select(textureFunc).ToArray();
            Color backgroundColor = prototypes != null && prototypes.Count() > 0 &&
                prototypes.All(p => p.Type == PrototypeType.DepthSnap || p.Type == PrototypeType.ColorDepthSnap) ? Color.black : default;
            backgroundColor = color != default ? color : backgroundColor;
            return PackTextures(textures, backgroundColor);
        }

        internal static Texture2D PackTextures(Texture2D[] textures, Color color = default)
        {
            int scale = CalculateScaleForPackedTextures(textures.Length);
            RenderTextureFormat renderTextureFormat = GetEquivalentRenderTextureFormat(textures[0].format);
            int textureDimension = Mathf.CeilToInt(Mathf.Sqrt(textures.Length)) * textures[0].width;
            RenderTexture packedRenderTexture = new RenderTexture(textureDimension, textureDimension, 0, renderTextureFormat);
            SetFilteringForTexture(packedRenderTexture);

            ClearTexture(packedRenderTexture, color);

            Material packerMaterial = new Material(Shader.Find("Hidden/UntoldByte/GAINS/PackerShader"));
            packerMaterial.SetInt("scale", scale);

            Vector4 texturePosition;
            int count = 0;
            var previousActiveRenderTexture = RenderTexture.active;
            foreach (Texture2D texture in textures)
            {
                texturePosition = new Vector4(count / scale, count % scale);
                packerMaterial.SetVector("texturePosition", texturePosition);

                Graphics.Blit(texture, packedRenderTexture, packerMaterial);
                count++;
            }
            RenderTexture.active = previousActiveRenderTexture;

            Object.DestroyImmediate(packerMaterial);
            Texture2D packedTexture2D = ToTexture2D(packedRenderTexture);
            packedRenderTexture.Release();
            Object.DestroyImmediate(packedRenderTexture);

            return packedTexture2D;
        }

        internal static Texture2D[] UnpackTexture(Texture2D texture, int count)
        {
            int scale = CalculateScaleForPackedTextures(count);
            Texture2D[] textures = new Texture2D[count];

            Material unpackerMaterial = new Material(Shader.Find("Hidden/UntoldByte/GAINS/UnpackerShader"));
            unpackerMaterial.SetInt("scale", scale);

            for (int i = 0; i < count; i++)
            {
                RenderTexture tmpRenderTexture = new RenderTexture(texture.width, texture.height, 0, RenderTextureFormat.Default);
                SetFilteringForTexture(tmpRenderTexture);
                unpackerMaterial.SetVector("texturePosition", new Vector4(i / scale, i % scale));

                switch (SystemInfo.graphicsDeviceType)
                {
                    case GraphicsDeviceType.Direct3D11:
                    case GraphicsDeviceType.Direct3D12:
                    case GraphicsDeviceType.Metal:
                    case GraphicsDeviceType.Switch:
                    case GraphicsDeviceType.XboxOne:
                    case GraphicsDeviceType.XboxOneD3D12:
                    case GraphicsDeviceType.GameCoreXboxOne:
                    case GraphicsDeviceType.GameCoreXboxSeries:
                    case GraphicsDeviceType.PlayStation4:
                    case GraphicsDeviceType.PlayStation5:
                        unpackerMaterial.DisableKeyword("GAINS_VK");
                        unpackerMaterial.DisableKeyword("GAINS_GL");
                        unpackerMaterial.EnableKeyword("GAINS_DX");
                        break;
                    case GraphicsDeviceType.OpenGLES3:

#if UNITY_2023_1_OR_NEWER
#else
                    case GraphicsDeviceType.OpenGLES2:
#endif
                    case GraphicsDeviceType.OpenGLCore:
                        unpackerMaterial.DisableKeyword("GAINS_VK");
                        unpackerMaterial.DisableKeyword("GAINS_DX");
                        unpackerMaterial.EnableKeyword("GAINS_GL");
                        break;
                    case GraphicsDeviceType.Vulkan:
                        unpackerMaterial.DisableKeyword("GAINS_DX");
                        unpackerMaterial.DisableKeyword("GAINS_GL");
                        unpackerMaterial.EnableKeyword("GAINS_VK");
                        break;

                    default:
                        break;
                }

                var previousActiveRenderTexture = RenderTexture.active;
                Graphics.Blit(texture, tmpRenderTexture, unpackerMaterial);
                RenderTexture.active = previousActiveRenderTexture;

                textures[i] = ToTexture2D(tmpRenderTexture);
                tmpRenderTexture.Release();
            }

            Object.DestroyImmediate(unpackerMaterial);

            return textures;
        }

        internal static Texture2D InvertInputColors(Texture2D texture)
        {
            Material colorInverterMaterial = new Material(Shader.Find("Hidden/UntoldByte/GAINS/InvertInputColorShader"));
            RenderTextureFormat renderTextureFormat = GetEquivalentRenderTextureFormat(texture.format);
            RenderTexture renderTexture = new RenderTexture(texture.width, texture.height, 0, renderTextureFormat);
            SetFilteringForTexture(renderTexture);

            var previousActiveRenderTexture = RenderTexture.active;
            Graphics.Blit(texture, renderTexture, colorInverterMaterial);
            RenderTexture.active = previousActiveRenderTexture;

            Object.DestroyImmediate(colorInverterMaterial);
            Texture2D invertedTexture = ToTexture2D(renderTexture);
            renderTexture.Release();

            return invertedTexture;
        }

        private static int CalculateScaleForPackedTextures(int numberOfTextures)
        {
            int scale = 0;
            for (int i = 1; i <= 6; i++)
            {
                if (numberOfTextures <= i * i)
                {
                    scale = i;
                    break;
                }
            }
            return scale;
        }

        internal static void SetFilteringForTexture(Texture2D texture)
        {
            texture.anisoLevel = 16;
            texture.filterMode = FilterMode.Bilinear;
        }

        internal static void SetFilteringForTexture(RenderTexture renderTexture)
        {
            renderTexture.anisoLevel = 16;
            renderTexture.antiAliasing = 8;
        }

        internal static void ClearTexture(RenderTexture renderTexture, Color color = default)
        {
            if (color == default)
                color = Color.white;

            var previousActiveRenderTexture = RenderTexture.active;
            RenderTexture.active = renderTexture;
            GL.Clear(true, true, color);
            RenderTexture.active = previousActiveRenderTexture;
        }

        internal static List<Texture2DArray> ToTexture2DArrayList(List<byte[][]> textureArrayByteList, TextureFormat textureFormat, int width, int height)
        {
            List<Texture2DArray> texture2DArrayList = new List<Texture2DArray>();
            foreach (byte[][] textureArrayBytes in textureArrayByteList)
            {
                texture2DArrayList.Add(ToTexture2DArray(textureArrayBytes, textureFormat, width, height));
            }
            return texture2DArrayList;
        }

        internal static List<byte[][]> ToByteArrayList(List<Texture2DArray> texture2DArrayList)
        {
            List<byte[][]> byteArrayList = new List<byte[][]>();
            foreach (Texture2DArray texture2DArray in texture2DArrayList)
            {
                byteArrayList.Add(ToByteArrays(texture2DArray));
            }
            return byteArrayList;
        }

        internal static Texture2DArray ToTexture2DArray(byte[][] textureArrayBytes, TextureFormat textureFormat, int width, int height)
        {
            List<Texture2D> texture2Ds = new List<Texture2D>();
            foreach (byte[] textureBytes in textureArrayBytes)
            {
                Texture2D texture2D = ToTexture2D(textureBytes, textureFormat, width, height);
                texture2Ds.Add(texture2D);
            }
            Texture2DArray texture2DArray = PackTextureArray(texture2Ds.ToArray());
            foreach (Texture2D texture2D in texture2Ds)
            {
                Object.DestroyImmediate(texture2D);
            }
            return texture2DArray;
        }

        internal static byte[][] ToByteArrays(Texture2DArray texture2DArray)
        {
            Texture2D[] texture2Ds = UnpackTextureArray(texture2DArray);
            byte[][] byteArrays = new byte[texture2Ds.Length][];
            for (int i = 0; i < texture2Ds.Length; i++)
            {
                byte[] byteArray = ToByteArray(texture2Ds[i]);
                byteArrays[i] = byteArray;
                Object.DestroyImmediate(texture2Ds[i]);
            }
            return byteArrays;
        }

        internal static Texture2D ToTexture2D(byte[] textureBytes, TextureFormat textureFormat, int width = 512, int height = 512)
        {
            if (IsFloatFormat(textureFormat))
            {
                return ByteArrayToTexture2DEXR(textureBytes, textureFormat, width, height);
            }
            else
            {
                return ByteArrayToTexture2D(textureBytes, textureFormat);
            }
        }

        internal static Texture2D ByteArrayToTexture2D(byte[] imageBytes)
        {
            Texture2D texture2D = new Texture2D(1, 1);
            texture2D.LoadImage(imageBytes);
            return texture2D;
        }

        internal static Texture2D ByteArrayToTexture2D(byte[] imageBytes, TextureFormat textureFormat)
        {
            Texture2D texture2D = new Texture2D(1, 1, textureFormat, false);
            texture2D.LoadImage(imageBytes);
            return texture2D;
        }

        internal static Texture2D ByteArrayToTexture2DEXR(byte[] imageBytes, TextureFormat textureFormat, int width, int height)
        {
            Texture2D texture2D = new Texture2D(width, height, textureFormat, false);
            texture2D.LoadRawTextureData(imageBytes);
            texture2D.Apply();
            return texture2D;
        }

        internal static IEnumerable<Prototype> ReadPrototypes(string prototypesDirectory, PrototypeType prototypeType)
        {
            if (!Directory.Exists(prototypesDirectory))
                Directory.CreateDirectory(prototypesDirectory);

            string[] filePaths = Directory.GetFiles(prototypesDirectory, "*.png");
            int count = 1;

            for (int i = 0; i < filePaths.Length; i++)
            {
                Prototype prototype = new Prototype
                {
                    Id = count++,
                    Type = prototypeType,
                };

                prototype.Texture = LoadPNGTexture(filePaths[i]);
                if (prototypeType == PrototypeType.ColorDepthSnap && filePaths.Length > ++i)
                {
                    prototype.SecondTexture = LoadPNGTexture(filePaths[i]);
                }

                yield return prototype;
            }
        }

        internal static IEnumerable<EntityPrototype> ReadPrototypes(string prototypesDirectory)
        {
            if (!Directory.Exists(prototypesDirectory))
                Directory.CreateDirectory(prototypesDirectory);

            string[] filePaths = Directory.GetFiles(prototypesDirectory, "*").Where(path => !path.EndsWith(".meta")).ToArray();
            int count = 1;

            for (int i = 0; i < filePaths.Length;)
            {
                EntityPrototype prototype = new EntityPrototype
                {
                    Id = count++,
                    Type = PrototypeType.DepthSnap
                };

                int prototypeParts = 0;

                if (filePaths.Length > i && GetFileName(filePaths[i]).EndsWith("d.png"))
                {
                    prototype.Texture = LoadPNGTexture(filePaths[i]);
                    prototypeParts++;
                    i++;
                }

                if (filePaths.Length > i && GetFileName(filePaths[i]).EndsWith("led.dat"))
                {
                    prototype.SecondTexture = LoadFloatTexture(filePaths[i]);
                    prototypeParts++;
                    i++;
                }

                if (filePaths.Length > i && GetFileName(filePaths[i]).EndsWith("uvs.dat"))
                {
                    byte[] fileBytes = File.ReadAllBytes(filePaths[i]);
                    byte[] decompressedBytes = CommonUtilities.Decompress(fileBytes);
                    prototype.UVs = ToVector4Array(decompressedBytes);
                    prototypeParts++;
                    i++;
                }

                yield return prototype;
            }
        }

        private static string GetFileName(string filePath)
        {
            return filePath.Split('/').Last();
        }

        internal static void SavePrototypes(IEnumerable<IPrototype> prototypes, string prototypesDirectory)
        {
            if (Directory.Exists(prototypesDirectory))
                Directory.Delete(prototypesDirectory, true);

            Directory.CreateDirectory(prototypesDirectory);
            int numberOfPrototypes = prototypes.Count();

            int count = 0;
            foreach (Prototype prototype in prototypes)
            {
                count++;
                int spacing = numberOfPrototypes.ToString().Length - count.ToString().Length;
                string spacingString = new string('#', spacing);

                File.WriteAllBytes($@"{prototypesDirectory}/{spacingString}{count}c.png", prototype.Texture.EncodeToPNG());

                if (prototype.SecondTexture == null) continue;

                File.WriteAllBytes($@"{prototypesDirectory}/{spacingString}{count}d.png", prototype.SecondTexture.EncodeToPNG());
            }

            AssetDatabase.Refresh();
        }

        internal static void SavePrototypes(IEnumerable<EntityPrototype> prototypes, string prototypesDirectory)
        {
            if (Directory.Exists(prototypesDirectory))
                Directory.Delete(prototypesDirectory, true);

            Directory.CreateDirectory(prototypesDirectory);
            int numberOfPrototypes = prototypes.Count();

            int count = 0;
            foreach (EntityPrototype prototype in prototypes)
            {
                count++;
                int spacing = numberOfPrototypes.ToString().Length - count.ToString().Length;
                string spacingString = new string('#', spacing);

                if (prototype.Texture != null)
                {
                    SavePNGTexture($@"{prototypesDirectory}/{spacingString}{count}d.png", prototype.Texture);
                }

                if (prototype.SecondTexture != null)
                {
                    SaveFloatTexture($@"{prototypesDirectory}/{spacingString}{count}led.dat", prototype.SecondTexture);
                }

                if (prototype.UVs != null && prototype.UVs.Length > 0)
                {
                    byte[] uvsBytes = ToByteArray(prototype.UVs);
                    byte[] compressedBytes = CommonUtilities.Compress(uvsBytes);
                    File.WriteAllBytes($@"{prototypesDirectory}/{spacingString}{count}uvs.dat", compressedBytes);
                }
            }

            AssetDatabase.Refresh();
        }

        internal static byte[] ToByteArray(Texture2D texture)
        {
            if (IsFloatFormat(texture.format))
            {
                return texture.GetRawTextureData();
            }
            else
            {
                return texture.EncodeToPNG();
            }
        }

        private static byte[] ToByteArray(Vector4[] vectorArray)
        {
            byte[] convertedBytes = new byte[sizeof(float) * 4 * vectorArray.Length];

            for (int i = 0; i < vectorArray.Length; i++)
            {
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(vectorArray[i].x), 0, convertedBytes, sizeof(float) * (i * 4), sizeof(float));
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(vectorArray[i].y), 0, convertedBytes, sizeof(float) * (i * 4 + 1), sizeof(float));
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(vectorArray[i].z), 0, convertedBytes, sizeof(float) * (i * 4 + 2), sizeof(float));
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(vectorArray[i].w), 0, convertedBytes, sizeof(float) * (i * 4 + 3), sizeof(float));
            }

            return convertedBytes;
        }

#pragma warning disable IDE0051 // Remove unused private members
        private static byte[] ToByteArray(Vector3[] vectorArray)
#pragma warning restore IDE0051 // Remove unused private members
        {
            byte[] convertedBytes = new byte[sizeof(float) * 3 * vectorArray.Length];

            for (int i = 0; i < vectorArray.Length; i++)
            {
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(vectorArray[i].x), 0, convertedBytes, sizeof(float) * (i * 3), sizeof(float));
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(vectorArray[i].y), 0, convertedBytes, sizeof(float) * (i * 3 + 1), sizeof(float));
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(vectorArray[i].z), 0, convertedBytes, sizeof(float) * (i * 3 + 2), sizeof(float));
            }

            return convertedBytes;
        }

        private static Vector4[] ToVector4Array(byte[] byteArray)
        {
            int vectorCount = byteArray.Length / (4 * sizeof(float));
            Vector4[] vectorArray = new Vector4[vectorCount];

            for (int i = 0; i < vectorCount; i++)
            {
                float x = System.BitConverter.ToSingle(byteArray, sizeof(float) * i * 4);
                float y = System.BitConverter.ToSingle(byteArray, sizeof(float) * (i * 4 + 1));
                float z = System.BitConverter.ToSingle(byteArray, sizeof(float) * (i * 4 + 2));
                float w = System.BitConverter.ToSingle(byteArray, sizeof(float) * (i * 4 + 3));
                vectorArray[i] = new Vector4(x, y, z, w);
            }

            return vectorArray;
        }

#pragma warning disable IDE0051 // Remove unused private members
        private static Vector3[] ToVector3Array(byte[] byteArray)
#pragma warning restore IDE0051 // Remove unused private members
        {
            int vectorCount = byteArray.Length / (3 * sizeof(float));
            Vector3[] vectorArray = new Vector3[vectorCount];

            for (int i = 0; i < vectorCount; i++)
            {
                float x = System.BitConverter.ToSingle(byteArray, sizeof(float) * i * 3);
                float y = System.BitConverter.ToSingle(byteArray, sizeof(float) * (i * 3 + 1));
                float z = System.BitConverter.ToSingle(byteArray, sizeof(float) * (i * 3 + 2));
                vectorArray[i] = new Vector3(x, y, z);
            }

            return vectorArray;
        }

        private static void SaveBase(Texture2D texture, string pathToSaveTo, string fileName = null)
        {
            if (!Directory.Exists(pathToSaveTo))
                Directory.CreateDirectory(pathToSaveTo);

            bool isFloatFormat = IsFloatFormat(texture);
            byte[] generatedPngBytes = isFloatFormat ? texture.EncodeToEXR(Texture2D.EXRFlags.OutputAsFloat | Texture2D.EXRFlags.CompressZIP) : texture.EncodeToPNG();
            string generatedHash = fileName ?? System.DateTime.Now.ToFileTime().ToString();
            string fileExtension = isFloatFormat ? "exr" : "png";
            File.WriteAllBytes($@"{pathToSaveTo}/{generatedHash}.{fileExtension}", generatedPngBytes);

            AssetDatabase.Refresh();
        }

        internal static void ExportGenerated(Texture2D generatedTexture)
        {
            string pathToSaveTo = "Assets/UntoldByte/GAINSExports/Generated";
            SaveBase(generatedTexture, pathToSaveTo);
        }

        internal static void ExportTexture(Texture2D texture, string fileName)
        {
            string pathToSaveTo = "Assets/UntoldByte/GAINSExports/Textures";
            SaveBase(texture, pathToSaveTo, fileName);
        }

        private static bool IsFloatFormat(Texture2D texture)
        {
            return IsFloatFormat(texture.format);
        }

        private static bool IsFloatFormat(TextureFormat textureFormat)
        {
            switch (textureFormat)
            {
                case TextureFormat.RHalf:
                case TextureFormat.RGHalf:
                case TextureFormat.RGBAHalf:
                case TextureFormat.RFloat:
                case TextureFormat.RGFloat:
                case TextureFormat.RGBAFloat:
                    return true;
                default:
                    return false;
            }
        }

        private static void SavePNGTexture(string filePath, Texture2D texture)
        {
            byte[] dataArray = texture.EncodeToPNG();
            File.WriteAllBytes(filePath, dataArray);

            AssetDatabase.Refresh();
        }

        private static void SaveFloatTexture(string filePath, Texture2D texture)
        {
            byte[] dataArray = texture.GetRawTextureData();
            byte[] compressedDataArray = CommonUtilities.Compress(dataArray);
            File.WriteAllBytes(filePath, compressedDataArray);

            AssetDatabase.Refresh();
        }

        private static Texture2D LoadPNGTexture(string filePath)
        {
            Texture2D texture2D = new Texture2D(1, 1);
            byte[] dataArray = File.ReadAllBytes(filePath);
            texture2D.LoadImage(dataArray);

            return texture2D;
        }

        private static Texture2D LoadFloatTexture(string filePath)
        {
            Texture2D texture2D = new Texture2D(512, 512, TextureFormat.RGBAFloat, 1, true);
            byte[] dataArray = File.ReadAllBytes(filePath);
            byte[] decompressedDataArray = CommonUtilities.Decompress(dataArray);
            texture2D.LoadRawTextureData(decompressedDataArray);

            return texture2D;
        }

        internal static Texture2D BakeTexture(Mesh mesh, Material material, int width, int height, int numberOfBakeLayers, int numberOfPixelsPerLayer, TextureFormat textureFormat = TextureFormat.RGBA32)
        {
            width = width != 0 ? width : 512;
            height = height != 0 ? height : 512;

            Camera camera = CameraUtilities.GetSceneViewCamera();
            camera.orthographic = true;
            camera.transform.position = new Vector3(0.5f, 0.5f, 0f);
            camera.transform.forward = Vector3.forward;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 1;
            camera.aspect = 1;
            camera.rect = new Rect(0, 0, 1, 1);
            camera.orthographicSize = 0.5f;

            Rect rect = new Rect(0, 0, width, height);
            RenderTexture renderTexture = new RenderTexture(width, height, 24, GetEquivalentRenderTextureFormat(textureFormat));
            Texture2D screenShot = new Texture2D(width, height, textureFormat, false);

            RenderTexture tmpRenderTexture = camera.targetTexture;
            RenderTexture activeRenderTexture = RenderTexture.active;
            DepthTextureMode tmpDepthTextureMode = camera.depthTextureMode;
            camera.depthTextureMode = DepthTextureMode.Depth;
            camera.targetTexture = renderTexture;

            CommandBuffer commandBuffer = new CommandBuffer();
            Matrix4x4[] matrices = CreateBakeMatrices(width, height, numberOfBakeLayers, numberOfPixelsPerLayer);
            commandBuffer.ClearRenderTarget(true, true, Color.clear);

            material.SetFloat("_UVMesh", 1);
            foreach (Matrix4x4 matrix in matrices)
            {
                commandBuffer.DrawMesh(mesh, matrix, material);
            }

            camera.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
            camera.Render();

            material.SetFloat("_UVMesh", 0);

            RenderTexture.active = renderTexture;
            screenShot.ReadPixels(rect, 0, 0);
            screenShot.Apply();

            camera.targetTexture = tmpRenderTexture;
            RenderTexture.active = activeRenderTexture;
            camera.depthTextureMode = tmpDepthTextureMode;

            renderTexture.Release();
            Object.DestroyImmediate(renderTexture);

            CameraUtilities.ReleaseSceneViewCamera();

            return screenShot;
        }

        private static Matrix4x4[] CreateBakeMatrices(int width, int height, int numberOfLevels, int pixelsPerLevel)
        {
            List<Matrix4x4> matrices = new List<Matrix4x4>();

            for (int i = numberOfLevels; i > 0; i--)
            {
                int numberOnLevel = i * 8;
                float angle = (2 * Mathf.PI) / numberOnLevel;
                for (int j = 0; j < numberOnLevel; j++)
                {
                    float x = pixelsPerLevel * i * Mathf.Cos(j * angle) / width;
                    float y = pixelsPerLevel * i * Mathf.Sin(j * angle) / height;
                    float z = 0.1f + 0.01f * i;
                    matrices.Add(Matrix4x4.TRS(new Vector3(x, y, z), Quaternion.identity, Vector3.one));
                }
            }

            matrices.Add(Matrix4x4.TRS(new Vector3(0, 0, 0.1f), Quaternion.identity, Vector3.one));

            return matrices.ToArray();
        }

        internal static Texture2D CreateReadableTexture2D(Texture2D texture)
        {
            Texture2D textureCopy = new Texture2D(texture.width, texture.height, texture.format, texture.mipmapCount > 1);
            Graphics.CopyTexture(texture, textureCopy);
            return textureCopy;
        }

        internal static Material[] GetGameObjectMaterials(GameObject gameObject)
        {
            if (gameObject == null) return null;

            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer == null) return null;

            Material[] sharedMaterials = meshRenderer.sharedMaterials;

            return sharedMaterials;
        }

        internal static Material GetGameObjectMaterial(GameObject gameObject, int index)
        {
            Material[] sharedMaterials = GetGameObjectMaterials(gameObject);
            if (sharedMaterials == null || sharedMaterials.Length == 0) return null;

            if (index < 0 || sharedMaterials.Length <= index) return null;

            Material material = sharedMaterials[index];
            if (material == null) return null;

            return material;
        }

        internal static Texture2D GetGameObjectTexture(GameObject gameObject, int index)
        {
            Material material = GetGameObjectMaterial(gameObject, index);

            if (material == null) return null;

            Texture2D baseTexture = GetMaterialMainTexture(material).FirstOrDefault();

            return baseTexture;
        }

        internal static IEnumerable<Texture2D> GetMaterialMainTexture(Material material)
        {
            if (material.mainTexture is Texture2DArray texture2DArray)
            {
                Texture2D[] texture2Ds = UnpackTextureArray(texture2DArray);
                foreach (Texture2D texture in texture2Ds)
                {
                    yield return texture;
                }
            }
            else
            {
                Texture2D baseTexture = material.mainTexture as Texture2D;
                if (baseTexture == null && material.HasProperty("_BaseMap"))
                    baseTexture = material.GetTexture("_BaseMap") as Texture2D;
                if (baseTexture == null && material.HasProperty("_MainTex"))
                    baseTexture = material.GetTexture("_MainTex") as Texture2D;
                yield return baseTexture;
            }
        }

        internal static Material CreateColorProjectionMaterial(Texture2D colorProjection)
        {
            Material material = new Material(Shader.Find("Hidden/UntoldByte/GAINS/IntermediateColorProjectionShader"));
            material.SetTexture("_ProjectionColor", colorProjection);
            material.name = $"Intermediate Color Projection Material";
            material.renderQueue += 1;
            return material;
        }

        internal static Material CreateDepthProjectionMaterial(Texture2D depthProjection)
        {
            Material material = new Material(Shader.Find("Hidden/UntoldByte/GAINS/IntermediateDepthProjectionShader"));
            material.SetTexture("_ProjectionLEDepth", depthProjection);
            material.name = $"Intermediate Depth Projection Material";
            material.renderQueue += 1;
            return material;
        }

        internal static Texture2D[] UnpackTextureArray(Texture2DArray texture2DArray)
        {
            List<Texture2D> texture2Ds = new List<Texture2D>();
            for (int layer = 0; layer < texture2DArray.depth; layer++)
            {
#pragma warning disable IDE0017 // Simplify object initialization
                Texture2D texture2D = new Texture2D(texture2DArray.width, texture2DArray.height, texture2DArray.format, texture2DArray.mipmapCount, false);
#pragma warning restore IDE0017 // Simplify object initialization
                texture2D.anisoLevel = texture2DArray.anisoLevel;
                texture2D.filterMode = texture2DArray.filterMode;
                texture2D.wrapMode = texture2DArray.wrapMode;
                for (int mipMap = 0; mipMap < texture2DArray.mipmapCount; mipMap++)
                    Graphics.CopyTexture(texture2DArray, layer, mipMap, texture2D, 0, mipMap);
                texture2Ds.Add(texture2D);
            }
            return texture2Ds.ToArray();
        }

        internal static Texture2DArray PackTextureArray(Texture2D[] texture2Ds)
        {
            Texture2D t = texture2Ds[0];
#pragma warning disable IDE0017 // Simplify object initialization
            Texture2DArray textureArray = new Texture2DArray(
                t.width, t.height, texture2Ds.Length, t.format, t.mipmapCount > 1
            );
#pragma warning restore IDE0017 // Simplify object initialization
            textureArray.anisoLevel = t.anisoLevel;
            textureArray.filterMode = t.filterMode;
            textureArray.wrapMode = t.wrapMode;
            for (int i = 0; i < texture2Ds.Length; i++)
            {
                t = texture2Ds[i];
                for (int m = 0; m < t.mipmapCount; m++)
                {
                    Graphics.CopyTexture(texture2Ds[i], 0, m, textureArray, i, m);
                }
            }
            return textureArray;
        }

        internal static void Flush()
        {
            if (CommonUtilities.IsOpenGLLike())
                GL.Flush();
        }
    }
}
