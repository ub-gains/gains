using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace UntoldByte.GAINS.Editor
{
    internal static class CameraUtilities
    {
        private static Camera camera;

        internal static Camera GetSceneViewCamera()
        {
            if (camera == null)
            {
                camera = Object.Instantiate(SceneView.lastActiveSceneView.camera);
                camera.CopyFrom(SceneView.lastActiveSceneView.camera);
                camera.nearClipPlane = 0.01f;
                camera.aspect = 1;
            }
            return camera;
        }

        internal static void ReleaseSceneViewCamera()
        {
            if (camera != null)
                if (camera.gameObject != null)
                    Object.DestroyImmediate(camera.gameObject);
                else
                    Object.DestroyImmediate(camera);
        }

        internal static void DrawSquareCameraPolyLine(Camera camera)
        {
            Vector3[] worldPolyLine = GetCameraWorldFramePoints(camera);
            Handles.DrawAAPolyLine(worldPolyLine);
        }

        private static Vector3[] GetCameraWorldFramePoints(Camera camera)
        {
            Vector3[] screenPoints = GetCameraScreenFramePoints(camera);
            Vector3[] worldPoints = screenPoints.Select(sp => camera.ScreenToWorldPoint(sp)).ToArray();
            return worldPoints;
        }

        private static Vector3[] GetCameraScreenFramePoints(Camera camera)
        {
            float distance = camera.nearClipPlane + 2;
            int halfMinDimension = Mathf.Min(camera.pixelWidth, camera.pixelHeight) / 2;
            Vector3 center = new Vector3(camera.pixelWidth / 2, camera.pixelHeight / 2, distance);
            Vector3 bottomLeft = center + new Vector3(-halfMinDimension, -halfMinDimension + 1);
            Vector3 topLeft = center + new Vector3(-halfMinDimension, halfMinDimension - 1);
            Vector3 topRight = center + new Vector3(halfMinDimension, halfMinDimension - 1);
            Vector3 bottomRight = center + new Vector3(halfMinDimension, -halfMinDimension + 1);
            return new Vector3[] { bottomLeft + new Vector3(0, 1), topLeft, topRight, bottomRight, bottomLeft + new Vector3(-1, 0) };
        }

        #region Camera capture

        internal static Texture2D GetCameraRenderTexture(Camera inputCamera, int width, int height, bool getDepth = false)
        {
            Rect rect = new Rect(0, 0, width, height);
#pragma warning disable IDE0017 // Simplify object initialization
            RenderTexture renderTexture = new RenderTexture(width * 2, height * 2, 24, RenderTextureFormat.ARGBFloat);
#pragma warning restore IDE0017 // Simplify object initialization
            renderTexture.antiAliasing = 8;
            renderTexture.anisoLevel = 8;
            Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGBAFloat, false);

            Camera camera = Camera.Instantiate(inputCamera);
            camera.CopyFrom(inputCamera);

            RenderTexture tmpRenderTexture = camera.targetTexture;
            RenderTexture activeRenderTexture = RenderTexture.active;
            DepthTextureMode tmpDepthTextureMode = camera.depthTextureMode;
            camera.depthTextureMode = DepthTextureMode.Depth;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.allowMSAA = true;
            camera.targetTexture = renderTexture;
            camera.farClipPlane = 10;
            camera.nearClipPlane = 0.05f;
            camera.pixelRect = new Rect(0, 0, width * 2, height * 2);
            camera.backgroundColor = Color.white;

            if (getDepth)
            {
                CommandBuffer commandBuffer = new CommandBuffer();
                Material material = new Material(Shader.Find("Hidden/UntoldByte/GAINS/DepthShader"));
                if (CommonUtilities.IsOpenGLLike())
                    material.EnableKeyword("GAINS_GL");
                else
                    material.DisableKeyword("GAINS_GL");
                commandBuffer.Blit(renderTexture.colorBuffer, renderTexture.colorBuffer, material);
                camera.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer);

                camera.Render();
            }

            if (!getDepth)
                camera.Render();

            RenderTexture finalRenderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGBFloat);
            Graphics.Blit(renderTexture, finalRenderTexture);

            RenderTexture.active = finalRenderTexture;
            screenShot.ReadPixels(rect, 0, 0);
            screenShot.Apply();

            camera.targetTexture = tmpRenderTexture;
            RenderTexture.active = activeRenderTexture;
            camera.depthTextureMode = tmpDepthTextureMode;

            Object.DestroyImmediate(renderTexture);
            Object.DestroyImmediate(finalRenderTexture);
            Camera.DestroyImmediate(camera.gameObject);
            return screenShot;
        }

        internal static Texture2D GetCameraDepthTexture2D(GameObject gameObject, Camera inputCamera, int width, int height, float nearClipPlane = 0.01f, float farClipPlane = 10)
        {
            Mesh mesh = MeshUtilities.GetSharedMesh(gameObject);
            if (mesh == null) return null;

#pragma warning disable IDE0017 // Simplify object initialization
            RenderTexture renderTexture = new RenderTexture(width * 2, height * 2, 24, RenderTextureFormat.ARGBFloat);
#pragma warning restore IDE0017 // Simplify object initialization
            renderTexture.antiAliasing = 8;
            renderTexture.anisoLevel = 8;

            Camera camera = Camera.Instantiate(inputCamera);
            camera.CopyFrom(inputCamera);

            RenderTexture activeRenderTexture = RenderTexture.active;
            camera.depthTextureMode = DepthTextureMode.Depth;
            camera.clearFlags = CameraClearFlags.Depth;
            camera.allowMSAA = true;
            camera.targetTexture = renderTexture;
            camera.farClipPlane = farClipPlane;
            camera.nearClipPlane = nearClipPlane;
            camera.pixelRect = new Rect(0, 0, width * 2, height * 2);

            Transform transform = gameObject.transform;
            Matrix4x4 matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);

#pragma warning disable IDE0017 // Simplify object initialization
            CommandBuffer commandBuffer = new CommandBuffer();
#pragma warning restore IDE0017 // Simplify object initialization
            commandBuffer.name = "Depth Renderer";
            Material material = new Material(Shader.Find("Hidden/UntoldByte/GAINS/DepthFastShader"));

            commandBuffer.SetRenderTarget(renderTexture);
            commandBuffer.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
            commandBuffer.ClearRenderTarget(true, true, Color.black);
            commandBuffer.DrawMesh(mesh, matrix, material);

            Graphics.ExecuteCommandBuffer(commandBuffer);

            Rect rect = new Rect(0, 0, width, height);
            Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGBAFloat, false);
            RenderTexture finalRenderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
            Graphics.Blit(renderTexture, finalRenderTexture);

            RenderTexture.active = finalRenderTexture;
            screenShot.ReadPixels(rect, 0, 0);
            screenShot.Apply();

            var minMax = GetMinMaxValues(screenShot);
            var pixels = AdjustPixels(screenShot, minMax);
#pragma warning disable UNT0017 // SetPixels invocation is slow - for higher precision (smoother depth texture)
            screenShot.SetPixels(0, 0, width, height, pixels);
#pragma warning restore UNT0017 // SetPixels invocation is slow
            screenShot.Apply();

            RenderTexture.active = activeRenderTexture;
            renderTexture.Release();
            finalRenderTexture.Release();
            Object.DestroyImmediate(renderTexture);
            Object.DestroyImmediate(finalRenderTexture);
            Object.DestroyImmediate(camera.gameObject);

            return screenShot;
        }

        internal static Texture2D GetCameraDepthNormalTexture2D(GameObject gameObject, Camera inputCamera, int width, int height,  float nearClipPlane = 0.01f, float farClipPlane = 10)
        {
            Mesh mesh = MeshUtilities.GetSharedMesh(gameObject);
            if (mesh == null) return null;

#pragma warning disable IDE0017 // Simplify object initialization
            RenderTexture renderTexture = new RenderTexture(width * 2, height * 2, 24, RenderTextureFormat.ARGBFloat);
#pragma warning restore IDE0017 // Simplify object initialization
            renderTexture.antiAliasing = 8;
            renderTexture.anisoLevel = 8;

            Camera camera = Camera.Instantiate(inputCamera);
            camera.CopyFrom(inputCamera);

            RenderTexture activeRenderTexture = RenderTexture.active;
            camera.depthTextureMode = DepthTextureMode.Depth;
            camera.clearFlags = CameraClearFlags.Depth;
            camera.allowMSAA = true;
            camera.targetTexture = renderTexture;
            camera.farClipPlane = farClipPlane;
            camera.nearClipPlane = nearClipPlane;
            camera.pixelRect = new Rect(0, 0, width * 2, height * 2);

            Transform transform = gameObject.transform;
            Matrix4x4 matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);

#pragma warning disable IDE0017 // Simplify object initialization
            CommandBuffer commandBuffer = new CommandBuffer();
#pragma warning restore IDE0017 // Simplify object initialization
            commandBuffer.name = "Depth Normal Renderer";
            Material material = new Material(Shader.Find("Hidden/UntoldByte/GAINS/DepthNormalShader"));
            material.SetVector("_MyProjectionParams", GetProjectionParameters(nearClipPlane, farClipPlane));
            material.SetVector("_MyZBufferParams", GetZBufferParams(nearClipPlane, farClipPlane));

            commandBuffer.SetRenderTarget(renderTexture);
            commandBuffer.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
            commandBuffer.DrawMesh(mesh, matrix, material);
            
            Graphics.ExecuteCommandBuffer(commandBuffer);

            Rect rect = new Rect(0, 0, width, height);
            Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGBAFloat, false);
            RenderTexture finalRenderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
            Graphics.Blit(renderTexture, finalRenderTexture);

            RenderTexture.active = finalRenderTexture;
            screenShot.ReadPixels(rect, 0, 0);
            screenShot.Apply();

            RenderTexture.active = activeRenderTexture;
            renderTexture.Release();
            finalRenderTexture.Release();
            Object.DestroyImmediate(renderTexture);
            Object.DestroyImmediate(finalRenderTexture);
            Object.DestroyImmediate(camera.gameObject);

            return screenShot;
        }

        internal static Vector4 GetProjectionParameters(float nearClip, float farClip)
        {
            float x = SystemInfo.usesReversedZBuffer ? -1 : 1;
            float y = nearClip;
            float z = farClip;
            float w = 1 / farClip;
            return new Vector4(x, y, z, w);
        }

        internal static Vector4 GetZBufferParams(float nearClip, float farClip)
        {
            //if (SystemInfo.usesReversedZBuffer)
            //{
                float x = -1 + farClip / nearClip;
                float y = 1;
                float z = x / farClip;
                float w = 1 / farClip;
                return new Vector4(x, y, z, w);
            //}
            //else
            //{
            //    float x = 1 - farClip / nearClip;
            //    float y = farClip / nearClip;
            //    float z = x / farClip;
            //    float w = y / farClip;
            //    return new Vector4(x, y, z, w);
            //}
        }

        [ExecuteInEditMode]
        internal static Texture2D GetCameraDepthTexture2D(Camera inputCamera, int width, int height, bool linearEyeDepth = false, float nearClipPlane = 0.01f, float farClipPlane = 10)
        {
#pragma warning disable IDE0017 // Simplify object initialization
            RenderTexture renderTexture = new RenderTexture(width * 2, height * 2, 0, RenderTextureFormat.ARGBFloat);
#pragma warning restore IDE0017 // Simplify object initialization
            renderTexture.antiAliasing = 8;
            renderTexture.anisoLevel = 8;

            Camera camera = Camera.Instantiate(inputCamera);
            camera.CopyFrom(inputCamera);

            RenderTexture activeRenderTexture = RenderTexture.active;
            camera.depthTextureMode = DepthTextureMode.Depth;
            camera.clearFlags = CameraClearFlags.Depth;
            camera.allowMSAA = true;
            camera.targetTexture = renderTexture;
            camera.farClipPlane = farClipPlane;
            camera.nearClipPlane = nearClipPlane;
            camera.pixelRect = new Rect(0, 0, width * 2, height * 2);

#pragma warning disable IDE0017 // Simplify object initialization
            CommandBuffer commandBuffer = new CommandBuffer();
#pragma warning restore IDE0017 // Simplify object initialization
            commandBuffer.name = "Copy Depth from _CameraDepthTexure CB";
            Material material = new Material(Shader.Find("Hidden/UntoldByte/GAINS/DepthShader"));
            material.SetInt("_LinearEyeDepth", linearEyeDepth ? 1 : 0);
            material.DisableKeyword(linearEyeDepth ? "_LinearEyeDepth__0" : "_LinearEyeDepth__1");
            material.EnableKeyword(linearEyeDepth ? "_LinearEyeDepth__1" : "_LinearEyeDepth__0");
            if (CommonUtilities.IsOpenGLLike())
                material.EnableKeyword("GAINS_GL");
            else
                material.DisableKeyword("GAINS_GL");
            commandBuffer.Blit(null, renderTexture.colorBuffer, material);
            camera.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer);

            camera.Render();

            Rect rect = new Rect(0, 0, width, height);
            Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGBAFloat, false);
            RenderTexture finalRenderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
            Graphics.Blit(renderTexture, finalRenderTexture);

            RenderTexture.active = finalRenderTexture;
            screenShot.ReadPixels(rect, 0, 0);
            screenShot.Apply();

            if (!linearEyeDepth)
            {
                var minMax = GetMinMaxValues(screenShot);
                var pixels = AdjustPixels(screenShot, minMax);
#pragma warning disable UNT0017 // SetPixels invocation is slow - for higher precision (smoother depth texture)
                screenShot.SetPixels(0, 0, width, height, pixels);
#pragma warning restore UNT0017 // SetPixels invocation is slow
                screenShot.Apply();
            }

            RenderTexture.active = activeRenderTexture;

            renderTexture.Release();
            finalRenderTexture.Release();
            Object.DestroyImmediate(renderTexture);
            Object.DestroyImmediate(finalRenderTexture);
            Object.DestroyImmediate(camera.gameObject);

            if (!linearEyeDepth)
            {
                byte[] screenShotByteArray = screenShot.EncodeToPNG();
                screenShot.LoadImage(screenShotByteArray);
            }

            return screenShot;
        }

        private static Vector2 GetMinMaxValues(Texture2D texture2D)
        {
            if (texture2D == null) return new Vector2();
            var pixels = texture2D.GetPixels();
            var nonZero = pixels.AsParallel().Where(c => c.r != 0);
            var min = nonZero.Any() ? nonZero.Min(c => c.r) : 0f;
            var max = pixels.AsParallel().Max(c => c.r);
            min = Mathf.Clamp01(min);
            return new Vector2(min, max);
        }

        private static Color[] AdjustPixels(Texture2D texture2D, Vector2 minMax)
        {
            var pixels = texture2D.GetPixels();
            pixels = pixels.AsParallel().Select(c =>
            {
                c.r = c.g = c.b = (c.r - minMax.x) * (1 / (minMax.y - minMax.x));
                return new Color(c.r, c.r, c.r, 1);
            }).ToArray();
            return pixels;
        }

        #endregion Camera capture
    }
}
