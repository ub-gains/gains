using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace UntoldByte.GAINS.Editor
{
    [AddComponentMenu("Scripts/UntoldByte/Entity Material Manager")]
    [System.Serializable]
    public class EntityMaterialManager : MonoBehaviour
    {
        [SerializeReference]
#pragma warning disable CA2235 // Mark all non-serializable fields
        private Material projectionMaterial;
#pragma warning restore CA2235 // Mark all non-serializable fields

        [SerializeField]
        private byte[] bakedTextureData;
        [System.NonSerialized]
        private Texture2D bakedTexture;
        internal Texture2D BakedTexture
        {
            get
            {
                if (bakedTexture != null) return bakedTexture;
                if (bakedTextureData == null || bakedTextureData.Length == 0) return null;
                bakedTexture = TextureUtilities.ByteArrayToTexture2D(bakedTextureData);
                return bakedTexture;
            }
            set
            {
                if (value == null)
                {
                    bakedTextureData = null;
                    Object.DestroyImmediate(bakedTexture);
                }
                else
                {
                    bakedTexture = value;
                    bakedTextureData = value.EncodeToPNG();
                }
            }
        }

        internal bool upscaling;
        internal UpscaleResolution upscaleResolution;

        [SerializeField]
        internal int numberOfBakeLayers;
        [SerializeField]
        internal int numberOfPixelsPerLayer;

        [SerializeField]
        internal bool selected;

        private void OnValidate()
        {
            SetInitialValues();
        }

        internal void SetInitialValues()
        {
            if (numberOfBakeLayers == 0)
                numberOfBakeLayers = 40;
            if (numberOfPixelsPerLayer == 0)
                numberOfPixelsPerLayer = 2;
        }

        internal void SetProjectionMaterial(Material material)
        {
            if (projectionMaterial != null)
                RemoveProjectionMaterial();

            projectionMaterial = material;

            List<Material> sharedMaterials = MeshUtilities.GetSharedGameObjectMaterials(gameObject).ToList();
            sharedMaterials.Add(projectionMaterial);
            MeshUtilities.SetSharedGameObjectMaterials(gameObject, sharedMaterials.ToArray());
        }

        internal async Task UpscaleMaterialImagesAsync()
        {
            upscaling = true;

            int size = (int)upscaleResolution;
            var textures = TextureUtilities.GetMaterialMainTexture(projectionMaterial);
            var upscaledTextures = await CommonUtilities.UpscaleImages(textures, CommonUtilities.Upscaler, size, size);

            Texture2DArray upscaledTexture2DArray = TextureUtilities.PackTextureArray(upscaledTextures.ToArray());
            DestroyImmediate(projectionMaterial.mainTexture);
            projectionMaterial.mainTexture = upscaledTexture2DArray;

            foreach (Texture2D texture2D in textures)
                DestroyImmediate(texture2D);
            foreach (Texture2D texture2D in upscaledTextures)
                DestroyImmediate(texture2D);

            upscaling = false;
        }

        internal void RemoveProjectionMaterial()
        {
            Material currentProjectionMaterial = projectionMaterial;
            if (currentProjectionMaterial == null) return;

            Material[] sharedMaterials = MeshUtilities.GetSharedGameObjectMaterials(gameObject);
            List<Material> sharedMaterialsList = sharedMaterials.ToList();

            sharedMaterialsList.Remove(currentProjectionMaterial);
            DestroyImmediate(currentProjectionMaterial);

            MeshUtilities.SetSharedGameObjectMaterials(gameObject, sharedMaterialsList.ToArray());
        }

        internal void BakeTexture()
        {
            Mesh sharedMesh = MeshUtilities.GetSharedMesh(gameObject);
            Vector2Int maxTextureSize = Vector2Int.Max(new Vector2Int(projectionMaterial.mainTexture.width, projectionMaterial.mainTexture.height), new Vector2Int((int)upscaleResolution, (int)upscaleResolution));
            BakedTexture = TextureUtilities.BakeTexture(sharedMesh, projectionMaterial, maxTextureSize.x, maxTextureSize.y, numberOfBakeLayers, numberOfPixelsPerLayer);
        }

        internal void ExportBaked()
        {
            if (BakedTexture == null)
                BakeTexture();

            string path = "Assets/UntoldByte/GAINSExports/";
            string materialsPath = path + "Materials/";
            string texturesPath = path + "Textures/";

            if (!Directory.Exists(materialsPath))
                Directory.CreateDirectory(materialsPath);

            if (!Directory.Exists(texturesPath))
                Directory.CreateDirectory(texturesPath);

            string timeString = System.DateTime.Now.ToFileTime().ToString();
            string textureFileName = "Texture " + timeString;

            TextureUtilities.ExportTexture(BakedTexture, textureFileName);
            Texture2D loadedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturesPath + textureFileName + ".png");

            Material material = new Material(Shader.Find("Unlit/Texture"));
            material.SetTexture("_MainTex", loadedTexture);
            string materialFileName = "Material " + timeString + ".asset";
            AssetDatabase.CreateAsset(material, materialsPath + materialFileName);
            AssetDatabase.SaveAssets();

            AssetDatabase.Refresh();
        }
    }

    public enum UpscaleResolution
    {
        _768 = 768,
        _1024 = 1024,
        _1536 = 1536,
        _2048 = 2048
    }

}