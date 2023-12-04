using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static UntoldByte.GAINS.Editor.CommonUtilities;

namespace UntoldByte.GAINS.Editor
{
    [System.Serializable]
    public class EntityMaterialManagerModule : IEntityMaterialManagerModule
    {
        private int upscalerIndex = 0;
        private string[] upscalers = System.Array.Empty<string>();
        private bool gettingUpscalers = false;

        #region Icons backing fields and properties

        private byte[] closeIconData;
        private byte[] upscaleIconData;
        private byte[] bakeIconData;
        private byte[] exportBakedIconData;
        private byte[] refreshIconData;

        [System.NonSerialized] private Texture2D close_icon;
        [System.NonSerialized] private Texture2D upscale_icon;
        [System.NonSerialized] private Texture2D bake_icon;
        [System.NonSerialized] private Texture2D export_baked_icon;
        [System.NonSerialized] private Texture2D refresh_icon; 

        private Texture2D CloseIcon
        {
            get
            {
                if (close_icon != null) return close_icon;
                if (closeIconData == null) return null;
                close_icon = TextureUtilities.ByteArrayToTexture2D(closeIconData);
                return close_icon;
            }
        }

        private Texture2D UpscaleIcon
        {
            get
            {
                if (upscale_icon != null) return upscale_icon;
                if (upscaleIconData == null) return null;
                upscale_icon = TextureUtilities.ByteArrayToTexture2D(upscaleIconData);
                return upscale_icon;
            }
        }

        private Texture2D BakeIcon
        {
            get
            {
                if (bake_icon != null) return bake_icon;
                if (bakeIconData == null) return null;
                bake_icon = TextureUtilities.ByteArrayToTexture2D(bakeIconData);
                return bake_icon;
            }
        }

        private Texture2D ExportBakedIcon
        {
            get
            {
                if (export_baked_icon != null) return export_baked_icon;
                if (exportBakedIconData == null) return null;
                export_baked_icon = TextureUtilities.ByteArrayToTexture2D(exportBakedIconData);
                return export_baked_icon;
            }
        }

        private Texture2D RefreshIcon
        {
            get
            {
                if (refresh_icon != null) return refresh_icon;
                if (refreshIconData == null) return null;
                refresh_icon = TextureUtilities.ByteArrayToTexture2D(refreshIconData);
                return refresh_icon;
            }
        }
        #endregion Icons backing fields and properties

        IEntityMaterialManagerHost entityMaterialManagerHost;

        bool initializedCheckStableDiffusionWebUILoaded;
        bool stableDiffusionWebUILoaded;
        EntityMaterialManager entityMaterialManager;

        private GUIStyle popupStyle;
        private GUIStyle PopupStyle
        {
            get
            {
                if (popupStyle != null) return popupStyle;

#pragma warning disable IDE0017 // Simplify object initialization
                popupStyle = new GUIStyle(EditorStyles.popup);
#pragma warning restore IDE0017 // Simplify object initialization
                popupStyle.fixedHeight = 28;

                return popupStyle;
            }
        }

        private bool assetsInitialized;

        internal void SetHost(IEntityMaterialManagerHost entityMaterialManagerHost)
        {
            this.entityMaterialManagerHost = entityMaterialManagerHost;
        }

        internal void SetTarget(EntityMaterialManager entityMaterialManager)
        {
            this.entityMaterialManager = entityMaterialManager;
        }

        private void CheckStableDiffusionUILoaded()
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            CheckStableDiffusionUILoadedAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private async Task CheckStableDiffusionUILoadedAsync()
        {
            if (initializedCheckStableDiffusionWebUILoaded) return;

            initializedCheckStableDiffusionWebUILoaded = true;
            stableDiffusionWebUILoaded = await CommonUtilities.StableDiffusionWebUILoaded();

            entityMaterialManagerHost.Repaint();
        }

        public void OnGUI()
        {
            if (entityMaterialManager == null) return;

            InitializeAssets();

            CheckStableDiffusionUILoaded();

            if (GUILayout.Button(new GUIContent("Remove", CloseIcon), GUILayout.ExpandHeight(true), GUILayout.Height(28)))
            {
                if (EditorUtility.DisplayDialog("Remove projections", "This operation is not reversible. " +
                    "Are you sure that you want to remove projection materials and Entity Material Manager component", "Yes", "No"))
                {
                    entityMaterialManager.RemoveProjectionMaterial();
                    Object.DestroyImmediate(entityMaterialManager);
                }
            }

            EditorGUI.BeginDisabledGroup(!stableDiffusionWebUILoaded);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Upscaler", EditorStyles.label, GUILayout.ExpandHeight(true), GUILayout.Height(28)); //EditorStyles.boldLabel, GUILayout.ExpandHeight(true), 
            int tmpUpscalerIndex = EditorGUILayout.Popup(upscalerIndex, upscalers, PopupStyle, GUILayout.ExpandHeight(true), GUILayout.Height(28));
            if (tmpUpscalerIndex != upscalerIndex)
            {
                upscalerIndex = tmpUpscalerIndex;
                Upscaler = upscalers[upscalerIndex];
                SaveSettings();
            }
            if (gettingUpscalers)
            {
                GUILayout.Label(new GUIContent("Loading..."), EditorStyles.label, GUILayout.ExpandHeight(true), GUILayout.Height(28));
            }
            else if (GUILayout.Button(new GUIContent("Refresh", RefreshIcon), GUILayout.ExpandHeight(true), GUILayout.Height(28)))
            {
                RefreshUpscalers();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Upscale resolution:", EditorStyles.label, GUILayout.ExpandHeight(true), GUILayout.Height(28));
            entityMaterialManager.upscaleResolution = (UpscaleResolution)EditorGUILayout.EnumPopup(entityMaterialManager.upscaleResolution, PopupStyle);
            GUILayout.EndHorizontal();
            EditorGUI.BeginDisabledGroup(entityMaterialManager.upscaling);
            if (GUILayout.Button(new GUIContent("Upscale", UpscaleIcon), GUILayout.ExpandHeight(true), GUILayout.Height(28)))
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                entityMaterialManager.UpscaleMaterialImagesAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.EndDisabledGroup();

            entityMaterialManager.numberOfBakeLayers = EditorGUILayout.IntSlider(new GUIContent("Padding (dilation) layers"), entityMaterialManager.numberOfBakeLayers, 20, 80);
            entityMaterialManager.numberOfPixelsPerLayer = EditorGUILayout.IntSlider(new GUIContent("Pixels per layer"), entityMaterialManager.numberOfPixelsPerLayer, 1, 5);

            if (GUILayout.Button(new GUIContent("Bake", BakeIcon), GUILayout.ExpandHeight(true), GUILayout.Height(28)))
            {
                entityMaterialManager.BakeTexture();
            }

            if (entityMaterialManager.BakedTexture != null)
            {
                if (GUILayout.Button(new GUIContent("Export baked", ExportBakedIcon), GUILayout.ExpandHeight(true), GUILayout.Height(28)))
                {
                    entityMaterialManager.ExportBaked();
                }

                GUILayout.Space(1);
                Rect bakedTextureRect = GUILayoutUtility.GetLastRect();
                float largerDimension = EditorGUIUtility.currentViewWidth - 20 - 17;
                bakedTextureRect.width = bakedTextureRect.height = largerDimension;

                GUILayout.Label(new GUIContent(entityMaterialManager.BakedTexture),
                    new GUIStyle()
                    {
                        fixedHeight = largerDimension,
                        fixedWidth = largerDimension,
                    });

                Rect lastRect = GUILayoutUtility.GetLastRect();
                if (lastRect.Contains(Event.current.mousePosition))
                {
                    Rect firstButtonRect = new Rect(lastRect.x + lastRect.width - 28, lastRect.y + 4, 24, 24);
                    if (GUI.Button(firstButtonRect, new GUIContent("", "Clear baked")))
                    {
                        entityMaterialManager.BakedTexture = null;
                    }
                    GUI.DrawTexture(firstButtonRect, CloseIcon);
                }
            }
        }

        private void InitializeAssets()
        {
            if (assetsInitialized) return;

            assetsInitialized = true;

            LoadIcons();
            RefreshUpscalers();
        }

        private void LoadIcons()
        {
            closeIconData = System.IO.File.ReadAllBytes("Assets/UntoldByte/GAINS/Icons/close_icon.png");
            upscaleIconData = System.IO.File.ReadAllBytes("Assets/UntoldByte/GAINS/Icons/upscale_icon.png");
            bakeIconData = System.IO.File.ReadAllBytes("Assets/UntoldByte/GAINS/Icons/bake_icon.png");
            exportBakedIconData = System.IO.File.ReadAllBytes("Assets/UntoldByte/GAINS/Icons/export_baked_icon.png");
            refreshIconData = System.IO.File.ReadAllBytes("Assets/UntoldByte/GAINS/Icons/refresh_icon.png");
        }

        void RefreshUpscalers()
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            RefreshUpscalersAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        async Task RefreshUpscalersAsync()
        {
            gettingUpscalers = true;
            upscalers = await CommonUtilities.RefreshUpscalersAsync();
            gettingUpscalers = false;

            SetupUpscaler();

            entityMaterialManagerHost.Repaint();
        }

        void SetupUpscaler()
        {
            upscalerIndex = upscalers.ToList().FindIndex(upscaler => upscaler.Equals(Upscaler, System.StringComparison.Ordinal));
            if (upscalerIndex < 0)
                upscalerIndex = 0;
        }

    }
}