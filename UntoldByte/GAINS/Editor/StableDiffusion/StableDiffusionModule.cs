using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using static UntoldByte.GAINS.Editor.CommonUtilities;

namespace UntoldByte.GAINS.Editor
{
    [Serializable]
    public class StableDiffusionModule : IStableDiffusionModule
    {
        #region Icons backing fields and properties

        private byte[] randomIconData;
        private byte[] recycleIconData;
        private byte[] refreshIconData;
        private byte[] previewIconData;

        [NonSerialized]
        private Texture2D random_icon;
        [NonSerialized]
        private Texture2D recycle_icon;
        [NonSerialized]
        private Texture2D refresh_icon;
        [NonSerialized]
        private Texture2D preview_icon;

        private Texture2D RandomIcon
        {
            get
            {
                if (random_icon != null) return random_icon;
                if (randomIconData == null) return null;
                random_icon = TextureUtilities.ByteArrayToTexture2D(randomIconData);
                return random_icon;
            }
        }
        private Texture2D RecycleIcon
        {
            get
            {
                if (recycle_icon != null) return recycle_icon;
                if (recycleIconData == null) return null;
                recycle_icon = TextureUtilities.ByteArrayToTexture2D(recycleIconData);
                return recycle_icon;
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
        private Texture2D PreviewIcon
        {
            get
            {
                if (preview_icon != null) return preview_icon;
                if (previewIconData == null) return null;
                preview_icon = TextureUtilities.ByteArrayToTexture2D(previewIconData);
                return preview_icon;
            }
        }

        #endregion Icons backing fields and properties

        StableDiffusionModuleData stableDiffusionModuleData;

        internal StableDiffusionModuleData StableDiffusionModuleData
        {
            set
            {
                stableDiffusionModuleData = value;
            }
            get
            {
                if (stableDiffusionModuleData != null) return stableDiffusionModuleData;

                StableDiffusionModuleData tmpStableDiffusionModuleData = ScriptableObject.CreateInstance<StableDiffusionModuleData>();
                if (!ReferenceEquals(stableDiffusionModuleData, (StableDiffusionModuleData)null))
                    EditorUtility.CopySerializedManagedFieldsOnly(stableDiffusionModuleData, tmpStableDiffusionModuleData);
                stableDiffusionModuleData = tmpStableDiffusionModuleData;

                return stableDiffusionModuleData;
            }
        }

        int ModelIndex { get { return StableDiffusionModuleData.modelIndex; } set { StableDiffusionModuleData.modelIndex = value; } }
        string PositivePrompt { get { return StableDiffusionModuleData.positivePrompt; } set { StableDiffusionModuleData.positivePrompt = value; } }
        string NegativePrompt { get { return StableDiffusionModuleData.negativePrompt; } set { StableDiffusionModuleData.negativePrompt = value; } }
        int SamplerIndex { get { return StableDiffusionModuleData.samplerIndex; } set { StableDiffusionModuleData.samplerIndex = value; } }
        int NumberOfSteps { get { return StableDiffusionModuleData.numberOfSteps; } set { StableDiffusionModuleData.numberOfSteps = value; } }
        float CFGScale { get { return StableDiffusionModuleData.CFGScale; } set { StableDiffusionModuleData.CFGScale = value; } }
        string Seed { get { return StableDiffusionModuleData.seed; } set { StableDiffusionModuleData.seed = value; } }
        float DenoisingStrength { get { return StableDiffusionModuleData.denoisingStrength; } set { StableDiffusionModuleData.denoisingStrength = value; } }
        int BatchSize { get { return StableDiffusionModuleData.batchSize; } set { StableDiffusionModuleData.batchSize = value; } }
        int Width { get { return StableDiffusionModuleData.width; } set { StableDiffusionModuleData.width = value; } }
        int Height { get { return StableDiffusionModuleData.height; } set { StableDiffusionModuleData.height = value; } }
        int ControlnetPreprocessorIndex { get { return StableDiffusionModuleData.controlnetPreprocessorIndex; } set { StableDiffusionModuleData.controlnetPreprocessorIndex = value; } }
        int ControlnetModelIndex { get { return StableDiffusionModuleData.controlnetModelIndex; } set { StableDiffusionModuleData.controlnetModelIndex = value; } }
        float ControlnetWeight { get { return StableDiffusionModuleData.controlnetWeight; } set { StableDiffusionModuleData.controlnetWeight = value; } }
        float ControlnetGuidanceStart { get { return StableDiffusionModuleData.controlnetGuidanceStart; } set { StableDiffusionModuleData.controlnetGuidanceStart = value; } }
        float ControlnetGuidanceEnd { get { return StableDiffusionModuleData.controlnetGuidanceEnd; } set { StableDiffusionModuleData.controlnetGuidanceEnd = value; } }

        //string ControlNetSelectedPreprocessor { get { return StableDiffusionModuleData.controlnetSelectedPreprocessor; } set { StableDiffusionModuleData.controlnetSelectedPreprocessor = value; } }
        float ControlNetPreprocessorValue { get { return StableDiffusionModuleData.controlnetPreprocessorValue; } set { StableDiffusionModuleData.controlnetPreprocessorValue = value; } }
        float ControlNetPreprocessorThresholdA { get { return StableDiffusionModuleData.controlnetPreprocessorThreshorldA; } set { StableDiffusionModuleData.controlnetPreprocessorThreshorldA = value; } }
        float ControlNetPreprocessorThresholdB { get { return StableDiffusionModuleData.controlnetPreprocessroThresholdB; } set { StableDiffusionModuleData.controlnetPreprocessroThresholdB = value; } }

        bool TiledVAEPresent { get { return StableDiffusionModuleData.tiledVAEPresent; } set { StableDiffusionModuleData.tiledVAEPresent = value; } }
        bool ShowTiledVAE { get { return StableDiffusionModuleData.showTiledVAE; } set { StableDiffusionModuleData.showTiledVAE = value; } }
        bool TiledVAEEnabled { get { return StableDiffusionModuleData.tiledVAEEnabled; } set { StableDiffusionModuleData.tiledVAEEnabled = value; } }
        int TiledVAEEncoderTileSize { get { return StableDiffusionModuleData.tiledVAEEncoderTileSize; } set { StableDiffusionModuleData.tiledVAEEncoderTileSize = value; } }
        int TiledVAEDecoderTileSize { get { return StableDiffusionModuleData.tiledVAEDecoderTileSize; } set { StableDiffusionModuleData.tiledVAEDecoderTileSize = value; } }
        bool TiledVAEVAEToGpu { get { return StableDiffusionModuleData.tiledVAEVAEToGpu; } set { StableDiffusionModuleData.tiledVAEVAEToGpu = value; } }
        bool TiledVAEFastDecoder { get { return StableDiffusionModuleData.tiledVAEFastDecoder; } set { StableDiffusionModuleData.tiledVAEFastDecoder = value; } }
        bool TiledVAEFastEncoder { get { return StableDiffusionModuleData.tiledVAEFastEncoder; } set { StableDiffusionModuleData.tiledVAEFastEncoder = value; } }
        bool TiledVAEColorFix { get { return StableDiffusionModuleData.tiledVAEColorFix; } set { StableDiffusionModuleData.tiledVAEColorFix = value; } }

        bool ShowHiResFix { get { return StableDiffusionModuleData.showHiResFix; } set { StableDiffusionModuleData.showHiResFix = value; } }
        bool HiResFixEnabled { get { return StableDiffusionModuleData.hiResFixEnabled; } set { StableDiffusionModuleData.hiResFixEnabled = value; } }
        int HiResFixUpscalerIndex { get { return StableDiffusionModuleData.hiResFixUpscalerIndex; } set { StableDiffusionModuleData.hiResFixUpscalerIndex = value; } }
        int HiResFixSteps { get { return StableDiffusionModuleData.hiResFixSteps; } set { StableDiffusionModuleData.hiResFixSteps = value; } }
        float HiResFixUpscaleBy { get { return StableDiffusionModuleData.hiResFixUpscaleBy; } set { StableDiffusionModuleData.hiResFixUpscaleBy = value; } }

        int tmpModelIndex;
        string tmpPositivePrompt;
        string tmpNegativePrompt;
        int tmpSamplerIndex;
        int tmpNumberOfSteps;
        float tmpCFGScale;
        string tmpSeed;
        float tmpDenoisingStrength;
        int tmpBatchSize;
        int tmpWidth;
        int tmpHeight;
        int tmpControlnetPreprocessorIndex;
        int tmpControlnetModelIndex;
        float tmpControlnetWeight;
        float tmpControlnetGuidanceStart;
        float tmpControlnetGuidanceEnd;
        float tmpControlnetPreprocessorValue;

        bool tmpTiledVAEEnabled;
        int tmpTiledVAEEncoderTileSize;
        int tmpTiledVAEDecoderTileSize;
        bool tmpTiledVAEVAEToGpu;
        bool tmpTiledVAEFastDecoder;
        bool tmpTiledVAEFastEncoder;
        bool tmpTiledVAEColorFix;
        readonly string tiledVAETurnedOnString = "Tiled VAE (on)";
        readonly string tiledVAETurnedOffString = "Tiled VAE (off)";

        bool tmpHiResFixEnabled;
        int tmpUpscalerIndex;
        int tmpHiResFixSteps;
        float tmpHiResFixUpscaleBy;
        readonly string hiResFixTurnedOnString = "HiRes Fix (on)";
        readonly string hiResFixTurnedOffString = "HiRes Fix (off)";

        string[] tmpControlnetPreprocessors = Array.Empty<string>();
        string[] tmpControlnetModels = Array.Empty<string>();

        string[] models = Array.Empty<string>();
        bool gettingModels = false;

        string[] samplers = Array.Empty<string>();
        bool gettingSamplers = false;

        string[] controlnetPreprocessors = Array.Empty<string>();
        bool gettingControlnetModules = false;

        bool gettingPreviewControlnetPreprocessor = false;

        string[] controlnetModels = Array.Empty<string>();
        bool gettingControlnetModels = false;

        bool gettingScripts = false;

        string[] upscalers = Array.Empty<string>();
        bool gettingUpscalers = false;

        [NonSerialized]
        Texture2D imageToImagePicture;
        [NonSerialized]
        Texture2D controlNetInputPicture;
        int numberOfTiles = 1;
        StableDiffusionMode stableDiffusionMode;
        bool showControlnetOptions;

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

        private GUIStyle textFieldStyle;
        private GUIStyle TextFieldStyle
        {
            get
            {
                if (textFieldStyle != null) return textFieldStyle;

#pragma warning disable IDE0017 // Simplify object initialization
                textFieldStyle = new GUIStyle(EditorStyles.textField);
#pragma warning restore IDE0017 // Simplify object initialization
                textFieldStyle.wordWrap = true;

                return textFieldStyle;
            }
        }

        private GUIStyle simpleTextFieldStyle;
        private GUIStyle SimpleTextFieldStyle
        {
            get
            {
                if (simpleTextFieldStyle != null) return simpleTextFieldStyle;

#pragma warning disable IDE0017 // Simplify object initialization
                simpleTextFieldStyle = new GUIStyle(EditorStyles.textField);
#pragma warning restore IDE0017 // Simplify object initialization
                simpleTextFieldStyle.alignment = TextAnchor.MiddleLeft;

                return simpleTextFieldStyle;
            }
        }

        string currentSeed = "-1";

        public IHostEditorWindow hostEditorWindow;

        #region IStableDiffusionClient

        internal void SetupReferences(IHostEditorWindow hostEditorWindow)
        {
            this.hostEditorWindow = hostEditorWindow;
        }

        internal void RefreshLists()
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            RefreshModelsAsync();
            RefreshSamplersAsync();
            RefreshControlnetModulesAsync();
            RefreshControlnetModelsAsync();
            RefreshScriptsAsync();
            RefreshUpscalersAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public void WindowSetup(IHostEditorWindow hostEditorWindow)
        {
            this.hostEditorWindow = hostEditorWindow;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            RefreshModelsAsync();
            RefreshSamplersAsync();
            RefreshControlnetModulesAsync();
            RefreshControlnetModelsAsync();
            RefreshScriptsAsync();
            RefreshUpscalersAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            LoadIcons();
        }

        internal void LoadIcons()
        {
            randomIconData = System.IO.File.ReadAllBytes("Assets/UntoldByte/GAINS/Icons/random_icon.png");
            recycleIconData = System.IO.File.ReadAllBytes("Assets/UntoldByte/GAINS/Icons/recycle_icon.png");
            refreshIconData = System.IO.File.ReadAllBytes("Assets/UntoldByte/GAINS/Icons/refresh_icon.png");
            previewIconData = System.IO.File.ReadAllBytes("Assets/UntoldByte/GAINS/Icons/preview_icon.png");
        }

        public void SetHost(IHostEditorWindow hostEditorWindow)
        {
            this.hostEditorWindow = hostEditorWindow;
        }

        public void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Model", EditorStyles.boldLabel, GUILayout.ExpandHeight(true), GUILayout.Height(28));
            EditorGUI.BeginChangeCheck();
            tmpModelIndex = ModelIndex;
            tmpModelIndex = EditorGUILayout.Popup(tmpModelIndex, models, PopupStyle, GUILayout.ExpandHeight(true), GUILayout.Height(28));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(StableDiffusionModuleData, "Model");
                ModelIndex = tmpModelIndex;
                EditorUtility.SetDirty(StableDiffusionModuleData);
            }
            if (gettingModels)
            {
                GUILayout.Label(new GUIContent("Loading..."), EditorStyles.label, GUILayout.ExpandHeight(true), GUILayout.Height(28));
            }
            else if (GUILayout.Button(new GUIContent("Refresh", RefreshIcon), GUILayout.ExpandHeight(true), GUILayout.Height(28)))
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                RefreshModelsAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Label("Positive prompt", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            tmpPositivePrompt = PositivePrompt;
            tmpPositivePrompt = GUILayout.TextField(tmpPositivePrompt, TextFieldStyle, GUILayout.ExpandWidth(true), GUILayout.Height(80f));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(StableDiffusionModuleData, "Positive prompt");
                PositivePrompt = tmpPositivePrompt;
                EditorUtility.SetDirty(StableDiffusionModuleData);
            }

            GUILayout.Label("Negative Prompt", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            tmpNegativePrompt = NegativePrompt;
            tmpNegativePrompt = GUILayout.TextArea(tmpNegativePrompt, TextFieldStyle, GUILayout.ExpandWidth(true), GUILayout.Height(80f));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(StableDiffusionModuleData, "Negative prompt");
                NegativePrompt = tmpNegativePrompt;
                EditorUtility.SetDirty(StableDiffusionModuleData);
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Sampler", EditorStyles.boldLabel, GUILayout.ExpandHeight(true), GUILayout.Height(28));
            EditorGUI.BeginChangeCheck();
            tmpSamplerIndex = SamplerIndex;
            tmpSamplerIndex = EditorGUILayout.Popup(tmpSamplerIndex, samplers, PopupStyle, GUILayout.ExpandHeight(true), GUILayout.Height(28));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(StableDiffusionModuleData, "Sampler");
                SamplerIndex = tmpSamplerIndex;
                EditorUtility.SetDirty(StableDiffusionModuleData);
            }
            if (gettingSamplers)
            {
                GUILayout.Label(new GUIContent("Loading..."), EditorStyles.label, GUILayout.ExpandHeight(true), GUILayout.Height(28));
            }
            else if (GUILayout.Button(new GUIContent("Refresh", RefreshIcon), GUILayout.ExpandHeight(true), GUILayout.Height(28)))
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                RefreshSamplersAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            tmpNumberOfSteps = NumberOfSteps;
            tmpNumberOfSteps = (int)EditorGUILayout.IntSlider(new GUIContent("Steps"), tmpNumberOfSteps, 1, 150);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(StableDiffusionModuleData, "Steps");
                NumberOfSteps = tmpNumberOfSteps;
                EditorUtility.SetDirty(StableDiffusionModuleData);
            }

            GUILayout.Space(3);
            EditorGUI.BeginChangeCheck();
            tmpWidth = Width;
            tmpWidth = EditorGUILayout.IntSlider(new GUIContent("Width"), tmpWidth, 64, 1536);
            tmpWidth = (int)Math.Round((double)tmpWidth / 8, MidpointRounding.AwayFromZero) * 8;
            if (EditorGUI.EndChangeCheck() && tmpWidth != Width)
            {
                Undo.RecordObject(StableDiffusionModuleData, "Width");
                Width = tmpWidth;
                EditorUtility.SetDirty(StableDiffusionModuleData);
            }
            EditorGUI.BeginChangeCheck();
            tmpHeight = Height;
            tmpHeight = EditorGUILayout.IntSlider(new GUIContent("Height"), tmpHeight, 64, 1536);
            tmpHeight = (int)Math.Round((double)tmpHeight / 8, MidpointRounding.AwayFromZero) * 8;
            if (EditorGUI.EndChangeCheck() && tmpHeight != Height)
            {
                Undo.RecordObject(StableDiffusionModuleData, "Height");
                Height = tmpHeight;
                EditorUtility.SetDirty(StableDiffusionModuleData);
            }
            GUILayout.Space(3);
            EditorGUI.BeginChangeCheck();
            tmpBatchSize = BatchSize;
            tmpBatchSize = EditorGUILayout.IntSlider(new GUIContent("Batch size"), tmpBatchSize, 1, 8);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(StableDiffusionModuleData, "Batch size");
                BatchSize = tmpBatchSize;
                EditorUtility.SetDirty(StableDiffusionModuleData);
            }
            GUILayout.Space(3);
            EditorGUI.BeginChangeCheck();
            tmpCFGScale = CFGScale;
            tmpCFGScale = EditorGUILayout.Slider(new GUIContent("CFG Scale"), tmpCFGScale, 2, 60);
            tmpCFGScale = (float)Math.Round(tmpCFGScale * 2, MidpointRounding.AwayFromZero) / 2;
            if (EditorGUI.EndChangeCheck() && tmpCFGScale != CFGScale)
            {
                Undo.RecordObject(StableDiffusionModuleData, "CFG Scale");
                CFGScale = tmpCFGScale;
                EditorUtility.SetDirty(StableDiffusionModuleData);
            }

            GUILayout.Space(3);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Seed", EditorStyles.boldLabel, GUILayout.ExpandHeight(true), GUILayout.Height(28));
            EditorGUI.BeginChangeCheck();

            tmpSeed = Seed;
            tmpSeed = GUILayout.TextField(tmpSeed, SimpleTextFieldStyle, GUILayout.ExpandHeight(true), GUILayout.Height(28));
            tmpSeed = System.Text.RegularExpressions.Regex.Replace(tmpSeed, "[^0-9\\-]", "");
            if (tmpSeed.Trim().Length == 0)
                tmpSeed = "-1";
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(StableDiffusionModuleData, "Seed");
                Seed = tmpSeed;
                EditorUtility.SetDirty(StableDiffusionModuleData);
            }

            if (GUILayout.Button(new GUIContent("RAND", RandomIcon), GUILayout.ExpandHeight(true), GUILayout.Height(28), GUILayout.Width(80)))
            {
                if (!Seed.Equals("-1", StringComparison.Ordinal))
                {
                    Undo.RecordObject(StableDiffusionModuleData, "Seed");
                    Seed = "-1";
                    EditorUtility.SetDirty(StableDiffusionModuleData);
                }
            }
            if (GUILayout.Button(new GUIContent("RECY", RecycleIcon), GUILayout.ExpandHeight(true), GUILayout.Height(28), GUILayout.Width(80)))
            {
                if (!Seed.Equals(currentSeed, StringComparison.Ordinal))
                {
                    Undo.RecordObject(StableDiffusionModuleData, "Seed");
                    Seed = currentSeed;
                    EditorUtility.SetDirty(StableDiffusionModuleData);
                }
            }
            EditorGUILayout.EndHorizontal();

            if (stableDiffusionMode == StableDiffusionMode.ColorDepth)
            {
                GUILayout.Space(3);
                EditorGUI.BeginChangeCheck();
                tmpDenoisingStrength = DenoisingStrength;
                tmpDenoisingStrength = EditorGUILayout.Slider(new GUIContent("Denoising"), tmpDenoisingStrength, 0, 1);
                tmpDenoisingStrength = (float)Math.Round(tmpDenoisingStrength, 2);
                if (EditorGUI.EndChangeCheck() && tmpDenoisingStrength != DenoisingStrength)
                {
                    Undo.RecordObject(StableDiffusionModuleData, "Denoising strength");
                    DenoisingStrength = tmpDenoisingStrength;
                    EditorUtility.SetDirty(StableDiffusionModuleData);
                }
            }

            ShowHiResFix = EditorGUILayout.Foldout(ShowHiResFix, HiResFixEnabled ? hiResFixTurnedOnString : hiResFixTurnedOffString, EditorStyles.foldoutHeader);

            if (ShowHiResFix)
            {
                EditorGUI.BeginChangeCheck();
                tmpHiResFixEnabled = HiResFixEnabled;
                tmpHiResFixEnabled = GUILayout.Toggle(tmpHiResFixEnabled, new GUIContent("Enable HiRes Fix"), GUILayout.ExpandHeight(true), GUILayout.Height(28));
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(StableDiffusionModuleData, "Enable HiRes Fix");
                    HiResFixEnabled = tmpHiResFixEnabled;
                    EditorUtility.SetDirty(StableDiffusionModuleData);
                }

                GUILayout.BeginHorizontal();
                GUILayout.Label("Upscaler", EditorStyles.label, GUILayout.ExpandHeight(true), GUILayout.Height(28));

                EditorGUI.BeginChangeCheck();
                tmpUpscalerIndex = HiResFixUpscalerIndex;
                tmpUpscalerIndex = EditorGUILayout.Popup(tmpUpscalerIndex, upscalers, PopupStyle, GUILayout.ExpandHeight(true), GUILayout.Height(28));
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(StableDiffusionModuleData, "HiRes Fix Upscaler");
                    HiResFixUpscalerIndex = tmpUpscalerIndex;
                    EditorUtility.SetDirty(StableDiffusionModuleData);
                }
                if (gettingUpscalers)
                {
                    GUILayout.Label(new GUIContent("Loading..."), EditorStyles.label, GUILayout.ExpandHeight(true), GUILayout.Height(28));
                }
                else if (GUILayout.Button(new GUIContent("Refresh", RefreshIcon), GUILayout.ExpandHeight(true), GUILayout.Height(28)))
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    RefreshUpscalersAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
                GUILayout.EndHorizontal();

                EditorGUI.BeginChangeCheck();
                tmpHiResFixSteps = HiResFixSteps;
                tmpHiResFixSteps = EditorGUILayout.IntSlider(new GUIContent("Steps"), tmpHiResFixSteps, 0, 150);
                if (EditorGUI.EndChangeCheck() && tmpHiResFixSteps != HiResFixSteps)
                {
                    Undo.RecordObject(StableDiffusionModuleData, "HiRes Fix Steps");
                    HiResFixSteps = tmpHiResFixSteps;
                    EditorUtility.SetDirty(StableDiffusionModuleData);
                }

                EditorGUI.BeginChangeCheck();
                tmpDenoisingStrength = DenoisingStrength;
                tmpDenoisingStrength = EditorGUILayout.Slider(new GUIContent("Denoising strength"), tmpDenoisingStrength, 0, 1);
                tmpDenoisingStrength = (float)Math.Round(tmpDenoisingStrength, 2);
                if (EditorGUI.EndChangeCheck() && tmpDenoisingStrength != DenoisingStrength)
                {
                    Undo.RecordObject(StableDiffusionModuleData, "HiRes Fix Denoising strength");
                    DenoisingStrength = tmpDenoisingStrength;
                    EditorUtility.SetDirty(StableDiffusionModuleData);
                }

                EditorGUI.BeginChangeCheck();
                tmpHiResFixUpscaleBy = HiResFixUpscaleBy;
                tmpHiResFixUpscaleBy = EditorGUILayout.Slider(new GUIContent("Upscale By"), tmpHiResFixUpscaleBy, 1, 4);
                tmpHiResFixUpscaleBy = (float)Math.Round(tmpHiResFixUpscaleBy * 20, MidpointRounding.AwayFromZero) / 20;
                if (EditorGUI.EndChangeCheck() && tmpHiResFixUpscaleBy != HiResFixUpscaleBy)
                {
                    Undo.RecordObject(StableDiffusionModuleData, "HiRes Fix Upscale by");
                    HiResFixUpscaleBy = tmpHiResFixUpscaleBy;
                    EditorUtility.SetDirty(StableDiffusionModuleData);
                }
            }

            EditorGUI.BeginDisabledGroup(gettingScripts);

            if (TiledVAEPresent)
                ShowTiledVAE = EditorGUILayout.Foldout(ShowTiledVAE, TiledVAEEnabled ? tiledVAETurnedOnString : tiledVAETurnedOffString, EditorStyles.foldoutHeader);

            if (TiledVAEPresent && ShowTiledVAE)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUI.BeginChangeCheck();
                tmpTiledVAEEnabled = TiledVAEEnabled;
                tmpTiledVAEEnabled = GUILayout.Toggle(tmpTiledVAEEnabled, new GUIContent("Enable Tiled VAE"), GUILayout.ExpandHeight(true), GUILayout.Height(28));
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(StableDiffusionModuleData, "Enable Tiled VAE");
                    TiledVAEEnabled = tmpTiledVAEEnabled;
                    EditorUtility.SetDirty(StableDiffusionModuleData);
                }

                EditorGUI.BeginChangeCheck();
                tmpTiledVAEVAEToGpu = TiledVAEVAEToGpu;
                tmpTiledVAEVAEToGpu = GUILayout.Toggle(tmpTiledVAEVAEToGpu, new GUIContent("Move VAE to GPU"), GUILayout.ExpandHeight(true), GUILayout.Height(28));
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(StableDiffusionModuleData, "Move VAE to GPU");
                    TiledVAEVAEToGpu = tmpTiledVAEVAEToGpu;
                    EditorUtility.SetDirty(StableDiffusionModuleData);
                }

                EditorGUILayout.EndHorizontal();

                EditorGUI.BeginChangeCheck();
                tmpTiledVAEEncoderTileSize = TiledVAEEncoderTileSize;
                tmpTiledVAEEncoderTileSize = (int)EditorGUILayout.IntSlider(new GUIContent("Encoder Tile Size"), tmpTiledVAEEncoderTileSize, 256, 4096);
                tmpTiledVAEEncoderTileSize = (int)Math.Round(tmpTiledVAEEncoderTileSize / 8.0, MidpointRounding.AwayFromZero) * 8;
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(StableDiffusionModuleData, "Encoder Tile Size");
                    TiledVAEEncoderTileSize = tmpTiledVAEEncoderTileSize;
                    EditorUtility.SetDirty(StableDiffusionModuleData);
                }

                EditorGUI.BeginChangeCheck();
                tmpTiledVAEDecoderTileSize = TiledVAEDecoderTileSize;
                tmpTiledVAEDecoderTileSize = (int)EditorGUILayout.IntSlider(new GUIContent("Decoder Tile Size"), tmpTiledVAEDecoderTileSize, 48, 512);
                tmpTiledVAEDecoderTileSize = (int)Math.Round(tmpTiledVAEDecoderTileSize / 8.0, MidpointRounding.AwayFromZero) * 8;
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(StableDiffusionModuleData, "Decoder Tile Size");
                    TiledVAEDecoderTileSize = tmpTiledVAEDecoderTileSize;
                    EditorUtility.SetDirty(StableDiffusionModuleData);
                }

                EditorGUILayout.BeginHorizontal();

                EditorGUI.BeginChangeCheck();
                tmpTiledVAEFastEncoder = TiledVAEFastEncoder;
                tmpTiledVAEFastEncoder = GUILayout.Toggle(tmpTiledVAEFastEncoder, new GUIContent("Fast Encoder"), GUILayout.ExpandHeight(true), GUILayout.Height(28)); 
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(StableDiffusionModuleData, "Fast Encoder");
                    TiledVAEFastEncoder = tmpTiledVAEFastEncoder;
                    EditorUtility.SetDirty(StableDiffusionModuleData);
                }

                EditorGUI.BeginChangeCheck();
                tmpTiledVAEColorFix = TiledVAEColorFix;
                tmpTiledVAEColorFix = GUILayout.Toggle(tmpTiledVAEColorFix, new GUIContent("Fast Encoder Color Fix"), GUILayout.ExpandHeight(true), GUILayout.Height(28));
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(StableDiffusionModuleData, "Fast Encoder Color Fix");
                    TiledVAEColorFix = tmpTiledVAEColorFix;
                    EditorUtility.SetDirty(StableDiffusionModuleData);
                }

                EditorGUI.BeginChangeCheck();
                tmpTiledVAEFastDecoder = TiledVAEFastDecoder;
                tmpTiledVAEFastDecoder = GUILayout.Toggle(tmpTiledVAEFastDecoder, new GUIContent("Fast Decoder"), GUILayout.ExpandHeight(true), GUILayout.Height(28));
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(StableDiffusionModuleData, "Fast Decoder");
                    TiledVAEFastDecoder = tmpTiledVAEFastDecoder;
                    EditorUtility.SetDirty(StableDiffusionModuleData);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.EndDisabledGroup();

            if (stableDiffusionMode != StableDiffusionMode.None)
                showControlnetOptions = EditorGUILayout.Foldout(showControlnetOptions, "Controlnet", EditorStyles.foldoutHeader);

            if (showControlnetOptions)
            {
                if (stableDiffusionMode == StableDiffusionMode.Sketch || stableDiffusionMode == StableDiffusionMode.Color)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Preprocessor", EditorStyles.boldLabel, GUILayout.ExpandHeight(true), GUILayout.Height(28));
                    EditorGUI.BeginChangeCheck();
                    tmpControlnetPreprocessorIndex = ControlnetPreprocessorIndex;
                    tmpControlnetPreprocessorIndex = EditorGUILayout.Popup(tmpControlnetPreprocessorIndex, tmpControlnetPreprocessors, PopupStyle, GUILayout.ExpandHeight(true), GUILayout.MaxHeight(32));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(StableDiffusionModuleData, "Controlnet Preprocessor");
                        ControlnetPreprocessorIndex = tmpControlnetPreprocessorIndex;

                        string preprocessor = tmpControlnetPreprocessors[tmpControlnetPreprocessorIndex];
                        var allPreprocessorSettings = StableDiffusionControlNetPreprocessorSettings.GetControlNetPreprocessorSettings();
                        if (allPreprocessorSettings.ContainsKey(preprocessor))
                        {
                            var preprocessorSettings = allPreprocessorSettings[preprocessor];

                            if (preprocessorSettings != null && preprocessorSettings.Length > 0)
                            {
                                for (int i = 0; i < preprocessorSettings.Length; i++)
                                {
                                    var preprocessorSettingsItem = preprocessorSettings[i];
                                    if (preprocessorSettingsItem == null) continue;

                                    tmpControlnetPreprocessorValue = preprocessorSettingsItem.value;
                                    if (i == 0)
                                    {
                                        ControlNetPreprocessorValue = tmpControlnetPreprocessorValue;
                                    }
                                    else if (i == 1)
                                    {
                                        ControlNetPreprocessorThresholdA = tmpControlnetPreprocessorValue;
                                    }
                                    else if (i == 2)
                                    {
                                        ControlNetPreprocessorThresholdB = tmpControlnetPreprocessorValue;
                                    }
                                }
                            }
                        }

                        EditorUtility.SetDirty(StableDiffusionModuleData);
                    }
                    if (gettingControlnetModules)
                    {
                        GUILayout.Label(new GUIContent("Loading..."), EditorStyles.label, GUILayout.ExpandHeight(true), GUILayout.Height(28));
                    }
                    else if (GUILayout.Button(new GUIContent("Refresh", RefreshIcon), GUILayout.ExpandHeight(true), GUILayout.Height(28)))
                    {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        RefreshControlnetModulesAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
                    EditorGUI.BeginDisabledGroup(gettingPreviewControlnetPreprocessor);
                    if (GUILayout.Button(new GUIContent("Peview", PreviewIcon), GUILayout.ExpandHeight(true), GUILayout.Height(28)))
                    {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        PreviewControlnetPreprocessorAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndHorizontal();
                }

                if (stableDiffusionMode != StableDiffusionMode.None)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Model", EditorStyles.boldLabel, GUILayout.ExpandHeight(true), GUILayout.Height(28));
                    EditorGUI.BeginChangeCheck();
                    tmpControlnetModelIndex = ControlnetModelIndex;
                    tmpControlnetModelIndex = EditorGUILayout.Popup(tmpControlnetModelIndex, tmpControlnetModels, PopupStyle, GUILayout.ExpandHeight(true), GUILayout.Height(28));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(StableDiffusionModuleData, "Controlnet Model");
                        ControlnetModelIndex = tmpControlnetModelIndex;
                        EditorUtility.SetDirty(StableDiffusionModuleData);
                    }
                    if (gettingControlnetModels)
                    {
                        GUILayout.Label(new GUIContent("Loading..."), EditorStyles.label, GUILayout.ExpandHeight(true), GUILayout.Height(28));
                    }
                    else if (GUILayout.Button(new GUIContent("Refresh", RefreshIcon), GUILayout.ExpandHeight(true), GUILayout.Height(28)))
                    {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        RefreshControlnetModelsAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(3);
                    EditorGUI.BeginChangeCheck();
                    tmpControlnetWeight = ControlnetWeight;
                    tmpControlnetWeight = EditorGUILayout.Slider(new GUIContent("Weight"), tmpControlnetWeight, 0, 2);
                    tmpControlnetWeight = (float)Math.Round(tmpControlnetWeight, 2);
                    if (EditorGUI.EndChangeCheck() && tmpControlnetWeight != ControlnetWeight)
                    {
                        Undo.RecordObject(StableDiffusionModuleData, "Controlnet Weight");
                        ControlnetWeight = tmpControlnetWeight;
                        EditorUtility.SetDirty(StableDiffusionModuleData);
                    }

                    GUILayout.Space(3);
                    EditorGUI.BeginChangeCheck();
                    tmpControlnetGuidanceStart = ControlnetGuidanceStart;
                    tmpControlnetGuidanceStart = EditorGUILayout.Slider(new GUIContent("Guidance Start"), tmpControlnetGuidanceStart, 0, 1);
                    tmpControlnetGuidanceStart = (float)Math.Round(tmpControlnetGuidanceStart, 2);
                    if (EditorGUI.EndChangeCheck() && tmpControlnetGuidanceStart != ControlnetGuidanceStart)
                    {
                        Undo.RecordObject(StableDiffusionModuleData, "Controlnet Guidance Start");
                        ControlnetGuidanceStart = tmpControlnetGuidanceStart;
                        EditorUtility.SetDirty(StableDiffusionModuleData);
                    }

                    GUILayout.Space(3);
                    EditorGUI.BeginChangeCheck();
                    tmpControlnetGuidanceEnd = ControlnetGuidanceEnd;
                    tmpControlnetGuidanceEnd = EditorGUILayout.Slider(new GUIContent("Guidance End"), tmpControlnetGuidanceEnd, 0, 1);
                    tmpControlnetGuidanceEnd = (float)Math.Round(tmpControlnetGuidanceEnd, 2);
                    if (EditorGUI.EndChangeCheck() && tmpControlnetGuidanceEnd != ControlnetGuidanceEnd)
                    {
                        Undo.RecordObject(StableDiffusionModuleData, "Controlnet Guidance End");
                        ControlnetGuidanceEnd = tmpControlnetGuidanceEnd;
                        EditorUtility.SetDirty(StableDiffusionModuleData);
                    }

                    if (tmpControlnetPreprocessors != null && tmpControlnetPreprocessors.Length > 1)
                    {
                        string preprocessor = tmpControlnetPreprocessors[tmpControlnetPreprocessorIndex];
                        var allPreprocessorSettings = StableDiffusionControlNetPreprocessorSettings.GetControlNetPreprocessorSettings();
                        if (allPreprocessorSettings.ContainsKey(preprocessor))
                        {
                            var preprocessorSettings = allPreprocessorSettings[preprocessor];

                            if (preprocessorSettings != null && preprocessorSettings.Length > 0)
                            {
                                for (int i = 0; i < preprocessorSettings.Length; i++)
                                {

                                    GUILayout.Space(3);
                                    EditorGUI.BeginChangeCheck();

                                    var preprocessorSettingsItem = preprocessorSettings[i];
                                    if (preprocessorSettingsItem == null) continue;

                                    if (i == 0)
                                    {
                                        tmpControlnetPreprocessorValue = ControlNetPreprocessorValue;
                                    }
                                    else if (i == 1)
                                    {
                                        tmpControlnetPreprocessorValue = ControlNetPreprocessorThresholdA;
                                    }
                                    else if (i == 2)
                                    {
                                        tmpControlnetPreprocessorValue = ControlNetPreprocessorThresholdB;
                                    }

                                    float tmpControlnetPreprocessorPreviousValue = tmpControlnetPreprocessorValue;

                                    tmpControlnetPreprocessorValue = EditorGUILayout.Slider(new GUIContent(preprocessorSettingsItem.name), tmpControlnetPreprocessorValue,
                                        preprocessorSettingsItem.min, preprocessorSettingsItem.max);
                                    tmpControlnetPreprocessorValue = (float)Math.Round((float)Math.Round(tmpControlnetPreprocessorValue / preprocessorSettingsItem.step, MidpointRounding.AwayFromZero) * preprocessorSettingsItem.step, 2);

                                    if (EditorGUI.EndChangeCheck() && tmpControlnetPreprocessorValue != tmpControlnetPreprocessorPreviousValue)
                                    {
                                        Undo.RecordObject(StableDiffusionModuleData, "Controlnet " + preprocessorSettingsItem.name);

                                        if (i == 0)
                                        {
                                            ControlNetPreprocessorValue = tmpControlnetPreprocessorValue;
                                        }
                                        else if (i == 1)
                                        {
                                            ControlNetPreprocessorThresholdA = tmpControlnetPreprocessorValue;
                                        }
                                        else if (i == 2)
                                        {
                                            ControlNetPreprocessorThresholdB = tmpControlnetPreprocessorValue;
                                        }

                                        EditorUtility.SetDirty(StableDiffusionModuleData);
                                    }

                                }
                            }
                        }
                    }
                }
            }
        }


        public async Task MakeRequest()
        {
            SDRequest request;
            if (stableDiffusionMode == StableDiffusionMode.ColorDepth)
                request = new SDColorDepthRequest();
            else if (stableDiffusionMode != StableDiffusionMode.None)
                request = new SDControlnetRequest();
            else
                request = new SDRequest();

            request.override_settings = new SDRequestOverrideSettings();
            if (models.Length > ModelIndex)
                request.override_settings.sd_model_checkpoint = models[ModelIndex];
            request.override_settings_restore_afterwards = false;

            request.prompt = PositivePrompt;
            if (samplers.Length > SamplerIndex)
                request.sampler_name = samplers[SamplerIndex];
            request.batch_size = BatchSize;
            request.steps = NumberOfSteps;
            request.cfg_scale = CFGScale;
            request.width = Width;
            request.height = Height;
            request.negative_prompt = NegativePrompt;
            request.seed = Seed;
            if (samplers.Length > SamplerIndex)
                request.sampler_index = samplers[SamplerIndex];

            if (HiResFixEnabled)
            {
                request.enable_hr = true;
                request.hr_upscaler = upscalers[HiResFixUpscalerIndex];
                request.hr_second_pass_steps = HiResFixSteps;
                request.denoising_strength = DenoisingStrength;
                request.hr_scale = HiResFixUpscaleBy;
            }

            //ignore for now
            if (stableDiffusionMode == StableDiffusionMode.ColorDepth)
            {
                var inputPng = imageToImagePicture.EncodeToPNG();
                var inputBase64String = Convert.ToBase64String(inputPng);

                var tmpRequest = request as SDColorDepthRequest;

                tmpRequest.init_images = new string[] { inputBase64String };
                tmpRequest.include_init_images = true;
                tmpRequest.denoising_strength = DenoisingStrength;
            }

            if (stableDiffusionMode != StableDiffusionMode.None)
            {
                var inputPng = controlNetInputPicture.EncodeToPNG();
                var inputBase64String = Convert.ToBase64String(inputPng);

#pragma warning disable IDE0017 // Simplify object initialization
                var controlnetRequest = new SDControlnetRequestControlnet();
#pragma warning restore IDE0017 // Simplify object initialization
                controlnetRequest.enabled = true;
                controlnetRequest.input_image = inputBase64String;

                controlnetRequest.module = tmpControlnetPreprocessors[ControlnetPreprocessorIndex];
                controlnetRequest.model = tmpControlnetModels[ControlnetModelIndex];

                controlnetRequest.weight = ControlnetWeight;
                controlnetRequest.guidance = 1;
                controlnetRequest.guidance_start = ControlnetGuidanceStart;
                controlnetRequest.guidance_end = ControlnetGuidanceEnd;
                controlnetRequest.lowvram = LowVRAM;
                if (ControlnetPreprocessorIndex > 0)
                {
                    controlnetRequest.processor_res = ControlNetPreprocessorValue;
                    controlnetRequest.threshold_a = ControlNetPreprocessorThresholdA;
                    controlnetRequest.threshold_b = ControlNetPreprocessorThresholdB;
                }

                var tmpRequest = request as SDControlnetRequest;

                tmpRequest.alwayson_scripts = new SDControlnetScriptRequest { ControlNet = new ControlNetArgs { args = new SDControlnetRequestControlnet[] { controlnetRequest } } };

                if (TiledVAEPresent)
                    tmpRequest.alwayson_scripts.TiledVAE = new TiledVAEArgs
                    {
                        enabled = TiledVAEEnabled,
                        encoder_tile_size = TiledVAEEncoderTileSize,
                        decoder_tile_size = TiledVAEDecoderTileSize,
                        vae_to_gpu = TiledVAEVAEToGpu,
                        fast_decoder = TiledVAEFastDecoder,
                        fast_encoder = TiledVAEFastEncoder,
                        color_fix = TiledVAEColorFix
                    };
            }

            var client = GetHttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(request));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            ReportProgress();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            string postEndpoint = stableDiffusionMode == StableDiffusionMode.ColorDepth ? "/sdapi/v1/img2img" : "/sdapi/v1/txt2img";
            HttpResponseMessage httpResponseMessage;
            try
            {
                httpResponseMessage = await client.PostAsync(postEndpoint, content);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
            {
                Debug.LogError("Could not make request to Stable Diffusion Web UI API. Check your settings and try again.");
                hostEditorWindow.Update(false);
                hostEditorWindow.RefreshUI();
                return;
            }
#pragma warning restore CA1031 // Do not catch general exception types

            string jsonString;
            if (httpResponseMessage.IsSuccessStatusCode)
                jsonString = await httpResponseMessage.Content.ReadAsStringAsync();
            else
            {
                hostEditorWindow.UpdateProgress(0, "");
                hostEditorWindow.UpdatePreview(false);
                hostEditorWindow.Update(false);
                hostEditorWindow.RefreshUI();
                Debug.LogError("Failed to generate image. \r\n");
                return;
            }

            var sdResponse = JsonConvert.DeserializeObject<SDResponse>(jsonString);
            var responseInfo = JsonConvert.DeserializeObject<SDResponseInfo>(sdResponse.info);
            currentSeed = responseInfo.seed;
            if (sdResponse.images != null && sdResponse.images.Length > 0)
            {
                List<StableDiffusionResult> generatedResults = new List<StableDiffusionResult>();

                foreach (string imageString in sdResponse.images)
                {
                    byte[] decoded = Convert.FromBase64String(imageString);
                    StableDiffusionResult stableDiffusionResult = new StableDiffusionResult()
                    {
                        numberOfTiles = numberOfTiles,
                        imageData = decoded
                    };
                    generatedResults.Add(stableDiffusionResult);
                }

                await WaitForHostReference();

                hostEditorWindow.Update(false, generatedResults);
                hostEditorWindow.UpdateProgress(0, "");
            }

            hostEditorWindow.Update(false);
            hostEditorWindow.RefreshUI();
        }

        private async Task WaitForHostReference()
        {
            while (((UnityEngine.Object)hostEditorWindow) == null)
                await Task.Delay(500);
        }

        public async Task Cancel()
        {
            HttpClient client = GetShortHttpClient();
            await client.PostAsync("/sdapi/v1/interrupt", null);
        }

        public void SetMode(StableDiffusionMode stableDiffusionMode)
        {
            Undo.RecordObject(StableDiffusionModuleData, "Use prototype");
            this.stableDiffusionMode = stableDiffusionMode;
            ControlnetPreprocessorIndex = 0;
            ControlnetModelIndex = 0;
            EditorUtility.SetDirty(StableDiffusionModuleData);

            SetupControlnetDropdowns();
        }

        private void SetupControlnetDropdowns()
        {
            List<string> preprocessors = new List<string> { "none" };
            List<string> models = new List<string> { "none" };

            switch (stableDiffusionMode)
            {
                case StableDiffusionMode.None:
                    break;
                case StableDiffusionMode.Sketch:
                    preprocessors.AddRange(controlnetPreprocessors.Where(p => p.Contains("scribble") || p.Contains("sketch") || p.Contains("hed") || p.Contains("canny")));
                    models.AddRange(controlnetModels.Where(m => m.Contains("scribble") || m.Contains("sketch") || m.Contains("hed") || m.Contains("canny")));

                    ControlnetPreprocessorIndex = 0;
                    string scribbleModelName = models.FirstOrDefault(m => m.Contains("control") && m.Contains("scribble"));
                    if (!string.IsNullOrEmpty(scribbleModelName))
                        ControlnetModelIndex = models.IndexOf(scribbleModelName);
                    break;
                case StableDiffusionMode.Color:
                    preprocessors.AddRange(controlnetPreprocessors.Where(p => !p.Equals("none", StringComparison.OrdinalIgnoreCase)));
                    models.AddRange(controlnetModels.Where(m => !m.Equals("none", StringComparison.OrdinalIgnoreCase)));

                    foreach (string preprocessor in preprocessors.Skip(1))
                    {
                        string matchingControlnetModel = models.FirstOrDefault(m => m.Contains(preprocessor));
                        if (string.IsNullOrWhiteSpace(matchingControlnetModel))
                            continue;
                        ControlnetPreprocessorIndex = preprocessors.IndexOf(preprocessor);
                        ControlnetModelIndex = models.IndexOf(matchingControlnetModel);
                        break;
                    }
                    break;
                case StableDiffusionMode.Depth:
                case StableDiffusionMode.ColorDepth:
                    models.AddRange(controlnetModels.Where(m => m.Contains("depth")));

                    ControlnetPreprocessorIndex = 0;
                    string depthModelName = models.FirstOrDefault(m => m.Contains("control") && m.Contains("depth"));
                    if (!string.IsNullOrEmpty(depthModelName))
                        ControlnetModelIndex = models.IndexOf(depthModelName);
                    break;
                default:
                    break;
            }

            tmpControlnetPreprocessors = preprocessors.ToArray();
            tmpControlnetModels = models.ToArray();
        }

        internal void SetImageToImagePicture(Texture2D imageToImagePicture)
        {
            this.imageToImagePicture = imageToImagePicture;
        }

        public void SetControlnetPicture(Texture2D controlNetPicture)
        {
            this.controlNetInputPicture = controlNetPicture;
        }

        internal void SetNumberOfTiles(int numberOfTiles)
        {
            this.numberOfTiles = numberOfTiles;
        }
        #endregion IStableDiffusionClient


        async Task RefreshModelsAsync()
        {
            gettingModels = true;

            string jsonModelsResponse = await RepeatGetStringAsync(() => GetShortHttpClient(), "/sdapi/v1/sd-models");
            SDModel[] modelsResponse = JsonConvert.DeserializeObject<SDModel[]>(jsonModelsResponse);
            models = modelsResponse.Select(sr => sr.title).ToArray();
            gettingModels = false;
            string jsonOptions = await GetShortHttpClient().GetStringAsync("/sdapi/v1/options");
            SDOptions options = JsonConvert.DeserializeObject<SDOptions>(jsonOptions);
            ModelIndex = Array.IndexOf(models, options.sd_model_checkpoint);

            hostEditorWindow.RefreshUI();
        }

        async Task RefreshSamplersAsync()
        {
            gettingSamplers = true;
            string jsonSamplersResponse = await RepeatGetStringAsync(() => GetShortHttpClient(), "/sdapi/v1/samplers");
            SDSampler[] samplersResponse = JsonConvert.DeserializeObject<SDSampler[]>(jsonSamplersResponse);
            samplers = samplersResponse.Select(sr => sr.name).ToArray();
            gettingSamplers = false;

            hostEditorWindow.RefreshUI();
        }

        async Task RefreshControlnetModulesAsync()
        {
            gettingControlnetModules = true;

            string jsonControlnetModulesResponse = await RepeatGetStringAsync(() => GetShortHttpClient(), "/controlnet/module_list?alias_names=true");
            SDControlnetModules controlnetModulesResponse = JsonConvert.DeserializeObject<SDControlnetModules>(jsonControlnetModulesResponse);
            controlnetPreprocessors = controlnetModulesResponse.module_list;
            gettingControlnetModules = false;

            SetupControlnetDropdowns();

            hostEditorWindow.RefreshUI();
        }

        async Task PreviewControlnetPreprocessorAsync()
        {
            gettingPreviewControlnetPreprocessor = true;

            var inputTexture = hostEditorWindow.PackControlnetImage();

            if (ControlnetPreprocessorIndex > 0)
            {
                var inputPng = inputTexture.EncodeToPNG();
                //UnityEngine.Object.DestroyImmediate(inputTexture);
                var inputBase64String = Convert.ToBase64String(inputPng);

#pragma warning disable IDE0017 // Simplify object initialization
                var controlnetRequest = new SDControlnetPreprocessorRequest();
#pragma warning restore IDE0017 // Simplify object initialization

                controlnetRequest.controlnet_input_images = new string[] { inputBase64String };

                controlnetRequest.controlnet_module = tmpControlnetPreprocessors[ControlnetPreprocessorIndex];

                controlnetRequest.controlnet_processor_res = ControlNetPreprocessorValue;
                controlnetRequest.controlnet_threshold_a = ControlNetPreprocessorThresholdA;
                controlnetRequest.controlnet_threshold_b = ControlNetPreprocessorThresholdB;
                var client = GetHttpClient();
                var content = new StringContent(JsonConvert.SerializeObject(controlnetRequest));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage httpResponseMessage;
                try
                {
                    httpResponseMessage = await client.PostAsync("/controlnet/detect", content);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch
                {
                    Debug.LogError("Could not make request to Stable Diffusion Web UI API. Check your settings and try again.");
                    gettingPreviewControlnetPreprocessor = false;
                    return;
                }
#pragma warning restore CA1031 // Do not catch general exception types

                string jsonString;
                if (httpResponseMessage.IsSuccessStatusCode)
                    jsonString = await httpResponseMessage.Content.ReadAsStringAsync();
                else
                {
                    Debug.LogError("Failed to preprocess image. \r\n");
                    gettingPreviewControlnetPreprocessor = false;
                    return;
                }

                var sdResponse = JsonConvert.DeserializeObject<SDResponse>(jsonString);

                if (sdResponse.images != null && sdResponse.images.Length > 0)
                {
                    byte[] decoded = Convert.FromBase64String(sdResponse.images[0]);
                    Texture2D packedPreprocessedTexture = TextureUtilities.ByteArrayToTexture2D(decoded);
                    PreviewWindow.Create("Packed preprocessed preview", packedPreprocessedTexture, destroyTextureOnClose: true);
                }
            }
            else
            {
                PreviewWindow.Create("Packed preprocessed preview", inputTexture, destroyTextureOnClose: true);
            }

            gettingPreviewControlnetPreprocessor = false;
        }

        async Task RefreshControlnetModelsAsync()
        {
            gettingControlnetModels = true;
            string jsonControlnetModelsResponse = await RepeatGetStringAsync(() => GetShortHttpClient(), "/controlnet/model_list");
            SDControlnetModels controlnetModelsResponse = JsonConvert.DeserializeObject<SDControlnetModels>(jsonControlnetModelsResponse);
            controlnetModels = controlnetModelsResponse.model_list;
            gettingControlnetModels = false;

            SetupControlnetDropdowns();

            hostEditorWindow.RefreshUI();
        }

        async Task RefreshScriptsAsync()
        {
            gettingScripts = true;
            string jsonScriptsResponse = await RepeatGetStringAsync(() => GetShortHttpClient(), "/sdapi/v1/scripts");
            SDScripts scriptsResponse = JsonConvert.DeserializeObject<SDScripts>(jsonScriptsResponse);
            TiledVAEPresent = scriptsResponse.txt2img.Contains("tiled vae", StringComparer.OrdinalIgnoreCase);
            gettingScripts = false;

            hostEditorWindow.RefreshUI();
        }

        async Task RefreshUpscalersAsync()
        {
            gettingUpscalers = true;

            string[] tmpLatentUpscalers = await CommonUtilities.RefreshLatentUpscalersAsync();
            string[] tmpUpscalers = await CommonUtilities.RefreshUpscalersAsync();
            upscalers = tmpLatentUpscalers.Union(tmpUpscalers).ToArray();

            gettingUpscalers = false;

            hostEditorWindow.RefreshUI();
        }

        public async Task<IEnumerable<Texture2D>> RemBg(IEnumerable<Texture2D> images)
        {
            List<Texture2D> texturesWithBackgroundRemoved = new List<Texture2D>();
            foreach (Texture2D image in images)
            {
                string encodedImage = Convert.ToBase64String(image.EncodeToPNG());

#pragma warning disable IDE0017 // Simplify object initialization
                RembgRequest request = new RembgRequest();
#pragma warning restore IDE0017 // Simplify object initialization
                request.input_image = encodedImage;
                request.model = "u2net";
                request.return_mask = false;
                request.alpha_matting = false;
                request.alpha_matting_erode_size = 10;
                request.alpha_matting_background_threshold = 10;
                request.alpha_matting_foreground_threshold = 240;

                HttpClient client = GetHttpClient();

                var content = new StringContent(JsonConvert.SerializeObject(request));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await client.PostAsync("/rembg", content);

                string jsonString;
                if (response.IsSuccessStatusCode)
                    jsonString = await response.Content.ReadAsStringAsync();
                else
                {
                    Debug.LogError("Failed to remove background.");
                    continue;
                }

                var sdResponse = JsonConvert.DeserializeObject<RembgResponse>(jsonString);

                string imageString = sdResponse.image;
                var decoded = Convert.FromBase64String(imageString);

                Texture2D textureWithBackgroundRemoved = new Texture2D(1, 1);
                textureWithBackgroundRemoved.LoadImage(decoded);
                texturesWithBackgroundRemoved.Add(textureWithBackgroundRemoved);
            }

            return texturesWithBackgroundRemoved.ToArray();
        }

        async Task ReportProgress()
        {
            var client = GetShortHttpClient();

            SDProgressResponse progressResponse = new SDProgressResponse();

            int counter = 0;
            char[] sc = { '-', '\\', '|', '/' };

            do
            {
                try
                {
                    await Task.Delay(1000);

                    var response = await client.GetAsync("/sdapi/v1/progress?skip_current_image=false");
                    var jsonProgressResponse = await response.Content.ReadAsStringAsync();
                    progressResponse = JsonConvert.DeserializeObject<SDProgressResponse>(jsonProgressResponse);

                    float generateProgress = progressResponse.progress;
                    //var estimatedTimeLeft = TimeSpan.FromSeconds(progressResponse.eta_relative);
                    //var timeFormatString = (estimatedTimeLeft.Hours > 0 ? @"h\h\ \:\ " : "") + @"mm\m\ \:ss\s";
                    string generateProgressText = generateProgress != 0f ? $"Steps {progressResponse.state.sampling_step}/{progressResponse.state.sampling_steps} ({(generateProgress * 100):n2}%) {sc[(counter++) % 4]}" : " "; //{estimatedTimeLeft.ToString(timeFormatString)}
                    hostEditorWindow.UpdateProgress(generateProgress, generateProgressText);

                    if (progressResponse.current_image != null)
                    {
                        byte[] imageByteArray = Convert.FromBase64String(progressResponse.current_image);
                        Texture2D imageTexture = new Texture2D(1, 1);
                        imageTexture.LoadImage(imageByteArray);
                        hostEditorWindow.UpdatePreview(true, imageTexture);
                    }

                    hostEditorWindow.RefreshUI();
                    //Debug.Log($"eta:{progressResponse.eta_relative}, progress:{progressResponse.progress}, step:{progressResponse.state.sampling_step}/{progressResponse.state.sampling_steps}, gen_init:{hostEditorWindow.GeneratingInitialized}");
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (TaskCanceledException)
                {
                    //just continue on progress request cancellation.
                }
                catch (HttpRequestException)
                {
                    //just continue on progress request cancellation.
                }
#pragma warning restore CA1031 // Do not catch general exception types
            } while (progressResponse.eta_relative > 0 || hostEditorWindow.GeneratingInitialized);
        }
    }

    [Serializable]
    public class StableDiffusionModuleData : ScriptableObject
    {
        public int modelIndex = 0;
        public string positivePrompt = "";
        public string negativePrompt = "";
        public int samplerIndex = 0;
        public int numberOfSteps = 20;
        public float CFGScale = 6.5f;
        public string seed = "-1";
        public float denoisingStrength = 0.75f;
        public int batchSize = 1;
        public int width = 512;
        public int height = 512;
        public int controlnetPreprocessorIndex = 0;
        public int controlnetModelIndex = 0;
        public float controlnetWeight = 0.95f;
        public float controlnetGuidanceStart = 0.0f;
        public float controlnetGuidanceEnd = 0.9f;

        //public string controlnetSelectedPreprocessor = "none";
        public float controlnetPreprocessorValue = 0;
        public float controlnetPreprocessorThreshorldA = 0;
        public float controlnetPreprocessroThresholdB = 0;

        public bool tiledVAEPresent = true;
        public bool showTiledVAE = false;
        public bool tiledVAEEnabled = false;
        public int tiledVAEEncoderTileSize = 768;
        public int tiledVAEDecoderTileSize = 64;
        public bool tiledVAEVAEToGpu = true;
        public bool tiledVAEFastDecoder = true;
        public bool tiledVAEFastEncoder = true;
        public bool tiledVAEColorFix = false;

        public bool showHiResFix = false;
        public bool hiResFixEnabled = false;
        public int hiResFixUpscalerIndex = 0;
        public int hiResFixSteps = 0;
        public float hiResFixUpscaleBy = 2;
    }

    [Serializable]
    public class StableDiffusionResult
    {
        public int numberOfTiles;
        public byte[] imageData;
        [NonSerialized]
        private Texture2D texture;
        public Texture2D Texture
        {
            get
            {
                if (texture != null) return texture;
                if (imageData == null || imageData.Length == 0) return null;
                texture = TextureUtilities.ByteArrayToTexture2D(imageData);
                return texture;
            }
        }

        public StableDiffusionResult DeepClone()
        {
#pragma warning disable IDE0017 // Simplify object initialization
            var clone = new StableDiffusionResult();
#pragma warning restore IDE0017 // Simplify object initialization
            clone.imageData = (byte[])imageData.Clone();
            return clone;
        }

        public void Clear()
        {
            imageData = Array.Empty<byte>();
            UnityEngine.Object.DestroyImmediate(texture);
        }
    }
}
