using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using static UntoldByte.GAINS.Editor.CommonUtilities;


namespace UntoldByte.GAINS.Editor
{
    [Serializable]
    public class SymbolCreator : EditorWindow, IPrototypeUpdater, IHostEditorWindow
    {
        #region Icons backing fields and properties

        private byte[] symbolCreatorIconData;
        private byte[] adjustIconData;
        private byte[] cameraBlackIconData;
        private byte[] cameraDoubleIconData;
        private byte[] cameraWhiteIconData;
        private byte[] closeIconData;
        private byte[] editIconData;
        private byte[] generateIconData;
        private byte[] plusIconData;
        private byte[] previewIconData;
        private byte[] refreshIconData;
        private byte[] saveIconData;

        [NonSerialized] private Texture2D symbol_creator_icon;
        [NonSerialized] private Texture2D adjust_icon;
        [NonSerialized] private Texture2D camera_black_icon;
        [NonSerialized] private Texture2D camera_double_icon;
        [NonSerialized] private Texture2D camera_white_icon;
        [NonSerialized] private Texture2D close_icon;
        [NonSerialized] private Texture2D edit_icon;
        [NonSerialized] private Texture2D generate_icon;
        [NonSerialized] private Texture2D plus_icon;
        [NonSerialized] private Texture2D preview_icon;
        [NonSerialized] private Texture2D refresh_icon;
        [NonSerialized] private Texture2D save_icon;

        private Texture2D SymbolCreatorIcon
        {
            get
            {
                if (symbol_creator_icon != null) return symbol_creator_icon;
                if (symbolCreatorIconData == null) return null;
                symbol_creator_icon = TextureUtilities.ByteArrayToTexture2D(symbolCreatorIconData);
                return symbol_creator_icon;
            }
        }
        private Texture2D AdjustIcon
        {
            get
            {
                if (adjust_icon != null) return adjust_icon;
                if (adjustIconData == null) return null;
                adjust_icon = TextureUtilities.ByteArrayToTexture2D(adjustIconData);
                return adjust_icon;
            }
        }
        private Texture2D CameraBlackIcon
        {
            get
            {
                if (camera_black_icon != null) return camera_black_icon;
                if (cameraBlackIconData == null) return null;
                camera_black_icon = TextureUtilities.ByteArrayToTexture2D(cameraBlackIconData);
                return camera_black_icon;
            }
        }
        private Texture2D CameraDoubleIcon
        {
            get
            {
                if (camera_double_icon != null) return camera_double_icon;
                if (cameraDoubleIconData == null) return null;
                camera_double_icon = TextureUtilities.ByteArrayToTexture2D(cameraDoubleIconData);
                return camera_double_icon;
            }
        }
        private Texture2D CameraWhiteIcon
        {
            get
            {
                if (camera_white_icon != null) return camera_white_icon;
                if (cameraWhiteIconData == null) return null;
                camera_white_icon = TextureUtilities.ByteArrayToTexture2D(cameraWhiteIconData);
                return camera_white_icon;
            }
        }
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
        private Texture2D EditIcon
        {
            get
            {
                if (edit_icon != null) return edit_icon;
                if (editIconData == null) return null;
                edit_icon = TextureUtilities.ByteArrayToTexture2D(editIconData);
                return edit_icon;
            }
        }
        private Texture2D GenerateIcon
        {
            get
            {
                if (generate_icon != null) return generate_icon;
                if (generateIconData == null) return null;
                generate_icon = TextureUtilities.ByteArrayToTexture2D(generateIconData);
                return generate_icon;
            }
        }
        private Texture2D PlusIcon
        {
            get
            {
                if (plus_icon != null) return plus_icon;
                if (plusIconData == null) return null;
                plus_icon = TextureUtilities.ByteArrayToTexture2D(plusIconData);
                return plus_icon;
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
        private Texture2D SaveIcon
        {
            get
            {
                if (save_icon != null) return save_icon;
                if (saveIconData == null) return null;
                save_icon = TextureUtilities.ByteArrayToTexture2D(saveIconData);
                return save_icon;
            }
        }

        #endregion Icons backing fields and properties

        private int previousSelectedTab;
        private int selectedTab;
        private readonly string[] tabNames = { "Prototype", "Generate", "Adjust" };

        private int previousSelectedPrototypeTab;
        private int selectedPrototypeTab;
        private readonly string[] prototypeTabNames = { "Sketch", "Color", "Depth" }; //, "Color + Depth" 

        private bool generateModeSelected;
        private int previousSelectedGenerateModeIndex = -1;
        private int selectedGenerateModeIndex;
        private readonly string[] generateModeOptions = { "None", "Sketch", "Color", "Depth" }; //, "Color + Depth"

        private bool windowVisible;
        private bool SceneFrameVisible => windowVisible && selectedTab == 0 && selectedPrototypeTab > 0;

#pragma warning disable CA2235 // Mark all non-serializable fields
        private Vector2 prototypeSketchScrollPosition;
        private Vector2 prototypeSnapScrollPosition;
        private Vector2 prototypeDepthScrollPosition;
        private Vector2 prototypeColorDepthScrollPosition;
#pragma warning restore CA2235 // Mark all non-serializable fields

        private StableDiffusionMode stableDiffusionMode;
        [NonSerialized]
        private List<Prototype> prototypes = new List<Prototype>();
        private List<Prototype> Prototypes
        {
            get
            {
                if (prototypes == null || prototypes.Count == 0)
                    AssignPrototypes();
                return prototypes;
            }
        }
        private List<Prototype> sketchPrototypes = new List<Prototype>();
        private List<Prototype> colorPrototypes = new List<Prototype>();
        private List<Prototype> depthPrototypes = new List<Prototype>();
        private List<Prototype> colorDepthPrototypes = new List<Prototype>();
        private int lastSketchPrototypeId = 0;
        private int lastColorPrototypeId = 0;
        private int lastDepthPrototypeId = 0;
        private int lastColorDepthPrototypeId = 0;

        private bool generatingInitialized = false;
        private bool generating = false;
        private float generateProgress;
        private string generateProgressText;
        [NonSerialized]
        private Texture2D previewTexture;
        private List<StableDiffusionResult> generatedResults;
#pragma warning disable CA2235 // Mark all non-serializable fields
        private Vector2 generateScrollPosition;
#pragma warning restore CA2235 // Mark all non-serializable fields

        private int upscalerIndex = 0;
        private string[] upscalers = Array.Empty<string>();
        private bool gettingUpscalers = false;
#pragma warning disable CA2235 // Mark all non-serializable fields
        private Vector2 adjustScrollPosition;
#pragma warning restore CA2235 // Mark all non-serializable fields

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
        private GUIStyle buttonStyle;
        private GUIStyle ButtonStyle
        {
            get
            {
                if (buttonStyle != null) return buttonStyle;

#pragma warning disable IDE0017 // Simplify object initialization
                buttonStyle = new GUIStyle(GUI.skin.button);
#pragma warning restore IDE0017 // Simplify object initialization
                buttonStyle.padding = new RectOffset(6, 6, 0, 0);

                return buttonStyle;
            }
        }

        #region Generate
        [SerializeField]
        private StableDiffusionModule stableDiffusionModule;
        #endregion Generate

        #region Adjust
        private StableDiffusionResult stableDiffusionResultForAdjustment;
        #endregion  Adjust

        private bool assetsInitialized = false;
        private bool sdClientInitialized = false;

        [NonSerialized]
        private Texture2D texture;

        [MenuItem("Window/UntoldByte/Symbol Creator")]
        private static void ShowWindow()
        {
            SymbolCreator window = GetWindow<SymbolCreator>();
            window.Focus();
            window.Repaint();
        }

        void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            Undo.undoRedoPerformed += UndoRedoPerformed;
            EditorApplication.playModeStateChanged += PlayModeStateChange;
            EditorApplication.quitting += Quitting;
            AssemblyReloadEvents.afterAssemblyReload += AfterAssemblyReload;

            Repaint();
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            Undo.undoRedoPerformed -= UndoRedoPerformed;
            EditorApplication.playModeStateChanged -= PlayModeStateChange;
            EditorApplication.quitting -= Quitting;
            AssemblyReloadEvents.afterAssemblyReload -= AfterAssemblyReload;
        }

        private void OnLostFocus()
        {
            windowVisible = false;
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (SceneFrameVisible)
                CameraUtilities.DrawSquareCameraPolyLine(sceneView.camera);
        }

        private void UndoRedoPerformed()
        {
            Repaint();
        }

        private void PlayModeStateChange(PlayModeStateChange playModeState)
        {
            if (playModeState == UnityEditor.PlayModeStateChange.EnteredEditMode)
            {
                SetHostOnStableDiffusionModule();
                Repaint();
            }

            SetWindowTitle();
            Repaint();
        }

        private void Quitting()
        {
            SavePrototypes();
        }

        private void AfterAssemblyReload()
        {
            if (!UpscalersLoaded())
                sdClientInitialized = false;

            SetWindowTitle();
        }

        private void OnGUI()
        {
            InitializeAssets();
            InitializeSDClient();

            RepaintSceneView();

            wantsMouseMove = false;
            windowVisible = true;

            if (titleContent.image == null) SetWindowTitle();

            GUILayout.Space(5);
            selectedTab = GUILayout.Toolbar(selectedTab, tabNames, ButtonStyle, GUILayout.Height(28));
            switch (selectedTab)
            {
                case 0:
                    OnGUIPrototype();
                    break;
                case 1:
                    OnGUIGenerate();
                    break;
                case 2:
                    OnGUIAdjust();
                    break;
            }
        }

        private void OnGUIPrototype()
        {
            GUILayout.Space(5);
            selectedPrototypeTab = GUILayout.Toolbar(selectedPrototypeTab, prototypeTabNames, GUILayout.Height(28));
            switch (selectedPrototypeTab)
            {
                case 0:
                    GUILayout.BeginHorizontal();
                    if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.T)
                    {
                        Texture2D packedTexture = TextureUtilities.PackPrototypes(sketchPrototypes);
                        Texture2D[] unpackedTextures = TextureUtilities.UnpackTexture(packedTexture, sketchPrototypes.Count);

                        Texture2D secondPackedTexture = TextureUtilities.PackTextures(unpackedTextures);

                        var testWindow = GetWindow<ScreenshotPreviewWindow>(true, "SD Texturer - screenshot test");
                        testWindow.generatedImageTexture = packedTexture;
                        testWindow.generatedDepthTexture = secondPackedTexture;
                        testWindow.destroyTexturesOnClose = true;

                        testWindow.ShowAuxWindow();
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Space(3);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(5);
                    prototypeSketchScrollPosition = EditorGUILayout.BeginScrollView(prototypeSketchScrollPosition);
                    int numberOfHorizontalItems = Mathf.Max(1, (int)(position.width - 5) / 130);
                    GUILayout.BeginVertical();
                    bool horizontalOpened = false;
                    if (sketchPrototypes != null)
                        for (int i = 0; i < sketchPrototypes.Count; i++)
                        {
                            if (i % numberOfHorizontalItems == 0)
                            {
                                GUILayout.BeginHorizontal();
                                horizontalOpened = true;
                            }

                            if (sketchPrototypes[i].Texture != null)
                            {
                                RepaintOnMouseMove();

                                GUILayout.Label(new GUIContent(sketchPrototypes[i].Texture),
                                    new GUIStyle()
                                    {
                                        fixedHeight = 128,
                                        fixedWidth = 128,
                                        margin = new RectOffset(0, 3, 0, 3),
                                        padding = new RectOffset(0, 0, 0, 0),
                                    }
                                );

                                Rect lastRect = GUILayoutUtility.GetLastRect();
                                if (lastRect.Contains(Event.current.mousePosition))
                                {
                                    Rect firstButtonRect = new Rect(lastRect.x + lastRect.width - 28, lastRect.y + 4, 24, 24);
                                    if (GUI.Button(firstButtonRect, new GUIContent("", "Remove")))
                                    {
                                        if (sketchPrototypes[i].Texture != null)
                                        {
                                            DestroyImmediate(sketchPrototypes[i].Texture);
                                        }
                                        sketchPrototypes.RemoveAt(i);
                                    }
                                    GUI.DrawTexture(firstButtonRect, CloseIcon);

                                    Rect secondButtonRect = new Rect(lastRect.x + lastRect.width - 54, lastRect.y + 4, 24, 24);
                                    if (GUI.Button(secondButtonRect, new GUIContent("", "Edit")))
                                    {
                                        Sketcher window = Sketcher.ShowWindow();
                                        window.SetParent(this);
                                        window.SetPrototype(sketchPrototypes[i]);
                                    }
                                    GUI.DrawTexture(secondButtonRect, EditIcon);
                                }
                            }

                            if (i % numberOfHorizontalItems == numberOfHorizontalItems - 1)
                            {
                                GUILayout.EndHorizontal();
                                horizontalOpened = false;
                            }
                        }

                    GUILayout.Space(3);
                    GUILayout.Space(0);
                    Rect selectSketchRect = GUILayoutUtility.GetLastRect();
                    selectSketchRect.size = new Vector2(128, 128);

                    Vector2 addSketchPosition = selectSketchRect.position + selectSketchRect.size / 2 - new Vector2(18, 18);
                    Rect addSketchRect = new Rect(addSketchPosition, new Vector2(36, 36));
                    if (GUI.Button(addSketchRect, "Add") || (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.BackQuote)) //button is visualy behind but is processing clicks
                    {
                        Sketcher window = Sketcher.ShowWindow();
                        window.SetParent(this);
                    }

                    texture = (Texture2D)EditorGUI.ObjectField(selectSketchRect, texture, typeof(Texture2D), false);
                    GUI.Button(addSketchRect, new GUIContent("", "Add"));
                    GUI.DrawTexture(addSketchRect, PlusIcon);

                    if (texture != null)
                    {
                        sketchPrototypes.Add(new Prototype
                        {
                            Id = ++lastSketchPrototypeId,
                            Type = PrototypeType.Sketch,
                            Texture = TextureUtilities.CreateReadableTexture2D(texture)
                        });
                        texture = null;
                    }

                    if (horizontalOpened)
                        GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                    EditorGUILayout.EndScrollView();
                    GUILayout.EndHorizontal();
                    break;
                case 1:
                    GUILayout.BeginHorizontal();
                    if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.T)
                    {
                        Texture2D packedTexture = TextureUtilities.PackPrototypes(colorPrototypes);
                        PreviewWindow.Create("Packed snaps", packedTexture, destroyTextureOnClose: true);
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Space(3);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(5);
                    prototypeSnapScrollPosition = EditorGUILayout.BeginScrollView(prototypeSnapScrollPosition);
                    numberOfHorizontalItems = Mathf.Max(1, (int)(position.width - 5) / 130);
                    GUILayout.BeginVertical();
                    horizontalOpened = false;
                    for (int i = 0; i < colorPrototypes.Count; i++)
                    {
                        if (i % numberOfHorizontalItems == 0)
                        {
                            GUILayout.BeginHorizontal();
                            horizontalOpened = true;
                        }

                        if (colorPrototypes[i].Texture != null)
                        {
                            RepaintOnMouseMove();

                            GUILayout.Label(new GUIContent(colorPrototypes[i].Texture),
                                new GUIStyle()
                                {
                                    fixedHeight = 128,
                                    fixedWidth = 128,
                                    margin = new RectOffset(0, 3, 0, 3),
                                    padding = new RectOffset(0, 0, 0, 0),
                                });

                            Rect lastRect = GUILayoutUtility.GetLastRect();
                            if (lastRect.Contains(Event.current.mousePosition))
                            {
                                Rect firstButtonRect = new Rect(lastRect.x + lastRect.width - 28, lastRect.y + 4, 24, 24);
                                if (GUI.Button(firstButtonRect, new GUIContent("", "Remove")))
                                {
                                    if (colorPrototypes[i].Texture != null)
                                    {
                                        DestroyImmediate(colorPrototypes[i].Texture);
                                    }
                                    colorPrototypes.RemoveAt(i);
                                }
                                GUI.DrawTexture(firstButtonRect, CloseIcon);
                            }
                        }

                        if (i % numberOfHorizontalItems == numberOfHorizontalItems - 1)
                        {
                            GUILayout.EndHorizontal();
                            horizontalOpened = false;
                        }
                    }

                    GUILayout.Space(3);
                    GUILayout.Space(0);
                    Rect selectSnapRect = GUILayoutUtility.GetLastRect();
                    selectSnapRect.size = new Vector2(128, 128);

                    Vector2 addSnapPosition = selectSnapRect.position + selectSnapRect.size / 2 - new Vector2(18, 18);
                    Rect addSnapRect = new Rect(addSnapPosition, new Vector2(36, 36));
                    if (GUI.Button(addSnapRect, "Snap") || (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.BackQuote)) //button is visualy behind but is processing clicks
                    {
                        Camera camera = CameraUtilities.GetSceneViewCamera();
                        Texture2D snap = CameraUtilities.GetCameraRenderTexture(camera, 512, 512);
                        CameraUtilities.ReleaseSceneViewCamera();
                        Prototype snapPrototype = new Prototype
                        {
                            Id = ++lastColorPrototypeId,
                            Type = PrototypeType.ColorSnap,
                            Texture = snap
                        };
                        DestroyImmediate(snap);
                        colorPrototypes.Add(snapPrototype);

                        Repaint();
                        Event.current.Use();
                    }

                    texture = (Texture2D)EditorGUI.ObjectField(selectSnapRect, texture, typeof(Texture2D), false);
                    GUI.Button(addSnapRect, new GUIContent("","Color snap"));
                    GUI.DrawTexture(addSnapRect, CameraWhiteIcon);

                    if (texture != null)
                    {
                        colorPrototypes.Add(new Prototype
                        {
                            Id = ++lastSketchPrototypeId,
                            Type = PrototypeType.ColorSnap,
                            Texture = TextureUtilities.CreateReadableTexture2D(texture)
                        });
                        texture = null;
                    }

                    if (horizontalOpened)
                        GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                    EditorGUILayout.EndScrollView();
                    GUILayout.EndHorizontal();
                    break;
                case 2:
                    GUILayout.BeginHorizontal();
                    if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.T)
                    {
                        Texture2D packedTexture = TextureUtilities.PackPrototypes(depthPrototypes, Color.black);
                        PreviewWindow.Create("Packed depths", packedTexture, destroyTextureOnClose: true);
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Space(3);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(5);
                    prototypeDepthScrollPosition = EditorGUILayout.BeginScrollView(prototypeDepthScrollPosition);
                    numberOfHorizontalItems = Mathf.Max(1, (int)(position.width - 5) / 130);
                    GUILayout.BeginVertical();
                    horizontalOpened = false;
                    for (int i = 0; i < depthPrototypes.Count; i++)
                    {
                        if (i % numberOfHorizontalItems == 0)
                        {
                            GUILayout.BeginHorizontal();
                            horizontalOpened = true;
                        }

                        if (depthPrototypes[i].Texture != null)
                        {
                            RepaintOnMouseMove();

                            GUILayout.Label(new GUIContent(depthPrototypes[i].Texture),
                                new GUIStyle()
                                {
                                    fixedHeight = 128,
                                    fixedWidth = 128,
                                    margin = new RectOffset(0, 3, 0, 3),
                                    padding = new RectOffset(0, 0, 0, 0),
                                });

                            Rect lastRect = GUILayoutUtility.GetLastRect();
                            if (lastRect.Contains(Event.current.mousePosition))
                            {
                                Rect firstButtonRect = new Rect(lastRect.x + lastRect.width - 28, lastRect.y + 4, 24, 24);
                                if (GUI.Button(firstButtonRect, new GUIContent(CloseIcon, "Remove")))
                                {
                                    if (depthPrototypes[i].Texture != null)
                                    {
                                        DestroyImmediate(depthPrototypes[i].Texture);
                                    }
                                    depthPrototypes.RemoveAt(i);
                                }
                                GUI.DrawTexture(firstButtonRect, CloseIcon);
                            }
                        }

                        if (i % numberOfHorizontalItems == numberOfHorizontalItems - 1)
                        {
                            GUILayout.EndHorizontal();
                            horizontalOpened = false;
                        }
                    }

                    GUILayout.Space(3);
                    GUILayout.Space(0);
                    Rect selectDepthRect = GUILayoutUtility.GetLastRect();
                    selectDepthRect.size = new Vector2(128, 128);

                    Vector2 addDepthPosition = selectDepthRect.position + selectDepthRect.size / 2 - new Vector2(18, 18);
                    Rect addDepthRect = new Rect(addDepthPosition, new Vector2(36, 36));
                    if (GUI.Button(addDepthRect, "Depth") || (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.BackQuote)) //button is visualy behind but is processing clicks
                    {
                        Camera camera = CameraUtilities.GetSceneViewCamera();
                        Texture2D depth = CameraUtilities.GetCameraDepthTexture2D(camera, 512, 512);
                        CameraUtilities.ReleaseSceneViewCamera();
                        Prototype depthPrototype = new Prototype
                        {
                            Id = ++lastDepthPrototypeId,
                            Type = PrototypeType.DepthSnap,
                            Texture = depth
                        };
                        DestroyImmediate(depth);
                        depthPrototypes.Add(depthPrototype);

                        Repaint();
                        Event.current.Use();
                    }

                    texture = (Texture2D)EditorGUI.ObjectField(selectDepthRect, texture, typeof(Texture2D), false);
                    GUI.Button(addDepthRect, new GUIContent("", "Depth snap"));
                    GUI.DrawTexture(addDepthRect, CameraBlackIcon);

                    if (texture != null)
                    {
                        depthPrototypes.Add(new Prototype
                        {
                            Id = ++lastSketchPrototypeId,
                            Type = PrototypeType.ColorSnap,
                            Texture = TextureUtilities.CreateReadableTexture2D(texture)
                        });
                        texture = null;
                    }

                    if (horizontalOpened)
                        GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                    EditorGUILayout.EndScrollView();
                    GUILayout.EndHorizontal();
                    break;
                case 3:
                    GUILayout.BeginHorizontal();
                    if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.T)
                    {
                        Texture2D packedTexture = TextureUtilities.PackPrototypes(colorDepthPrototypes, Color.white);
                        PreviewWindow.Create("Packed color + depth", packedTexture, destroyTextureOnClose: true);
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Space(3);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(5);
                    prototypeColorDepthScrollPosition = EditorGUILayout.BeginScrollView(prototypeColorDepthScrollPosition);
                    numberOfHorizontalItems = Mathf.Max(1, (int)(position.width - 5) / 130);
                    GUILayout.BeginVertical();
                    horizontalOpened = false;
                    for (int i = 0; i < colorDepthPrototypes.Count; i++)
                    {
                        if (i % numberOfHorizontalItems == 0)
                        {
                            GUILayout.BeginHorizontal();
                            horizontalOpened = true;
                        }

                        if (colorDepthPrototypes[i].Texture != null && colorDepthPrototypes[i].SecondTexture != null)
                        {
                            RepaintOnMouseMove();
                            GUILayout.Box("",
                                new GUIStyle()
                                {
                                    fixedHeight = 128,
                                    fixedWidth = 128,
                                    margin = new RectOffset(0, 3, 0, 3),
                                    padding = new RectOffset(0, 0, 0, 0)
                                });

                            Rect prototypeRect = GUILayoutUtility.GetLastRect();
                            Rect firstImageRect = new Rect(prototypeRect.position, prototypeRect.size - new Vector2(32,32));
                            Rect secondImageRect = new Rect(prototypeRect.position + new Vector2(32, 32), prototypeRect.size - new Vector2(32, 32));

                            if (secondImageRect.Contains(Event.current.mousePosition))
                            {
                                GUI.Label(firstImageRect, new GUIContent(colorDepthPrototypes[i].Texture),
                                    new GUIStyle()
                                    {
                                        fixedHeight = 96,
                                        fixedWidth = 96,
                                        margin = new RectOffset(0, 0, 0, 0),
                                        padding = new RectOffset(0, 0, 0, 0),
                                    });

                                GUI.Label(secondImageRect, new GUIContent(colorDepthPrototypes[i].SecondTexture),
                                    new GUIStyle()
                                    {
                                        fixedHeight = 96,
                                        fixedWidth = 96,
                                        margin = new RectOffset(0, 0, 0, 0),
                                        padding = new RectOffset(0, 0, 0, 0),
                                    });
                            }
                            else
                            {
                                GUI.Label(secondImageRect, new GUIContent(colorDepthPrototypes[i].SecondTexture),
                                    new GUIStyle()
                                    {
                                        fixedHeight = 96,
                                        fixedWidth = 96,
                                        margin = new RectOffset(0, 0, 0, 0),
                                        padding = new RectOffset(0, 0, 0, 0),
                                    });

                                GUI.Label(firstImageRect, new GUIContent(colorDepthPrototypes[i].Texture),
                                    new GUIStyle()
                                    {
                                        fixedHeight = 96,
                                        fixedWidth = 96,
                                        margin = new RectOffset(0, 0, 0, 0),
                                        padding = new RectOffset(0, 0, 0, 0),
                                    });
                            }

                            if (prototypeRect.Contains(Event.current.mousePosition))
                            {
                                Rect firstButtonRect = new Rect(prototypeRect.x + prototypeRect.width - 28, prototypeRect.y + 4, 24, 24);
                                if (GUI.Button(firstButtonRect, new GUIContent(CloseIcon, "Remove")))
                                {
                                    if (colorDepthPrototypes[i].Texture != null)
                                    {
                                        DestroyImmediate(colorDepthPrototypes[i].Texture);
                                    }

                                    if (colorDepthPrototypes[i].SecondTexture != null)
                                    {
                                        DestroyImmediate(colorDepthPrototypes[i].SecondTexture);
                                    }

                                    colorDepthPrototypes.RemoveAt(i);
                                }
                                GUI.DrawTexture(firstButtonRect, CloseIcon);
                            }
                        }

                        if (i % numberOfHorizontalItems == numberOfHorizontalItems - 1)
                        {
                            GUILayout.EndHorizontal();
                            horizontalOpened = false;
                        }
                    }

                    GUILayout.Space(3);
                    GUILayout.Space(0);
                    Rect selectColorDepthRect = GUILayoutUtility.GetLastRect();
                    selectColorDepthRect.size = new Vector2(128, 128);

                    Vector2 addColorDepthPosition = selectColorDepthRect.position + selectColorDepthRect.size / 2 - new Vector2(18, 18);
                    Rect addColorDepthRect = new Rect(addColorDepthPosition, new Vector2(36, 36));
                    if (GUI.Button(addColorDepthRect, "C+D") || (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.BackQuote)) //button is visualy behind but is processing clicks
                    {
                        Camera camera = CameraUtilities.GetSceneViewCamera();
                        Texture2D color = CameraUtilities.GetCameraRenderTexture(camera, 512, 512);
                        Texture2D depth = CameraUtilities.GetCameraDepthTexture2D(camera, 512, 512);
                        CameraUtilities.ReleaseSceneViewCamera();
                        Prototype colorDepthPrototype = new Prototype
                        {
                            Id = ++lastColorDepthPrototypeId,
                            Type = PrototypeType.ColorDepthSnap,
                            Texture = TextureUtilities.CreateReadableTexture2D(color),
                            SecondTexture = TextureUtilities.CreateReadableTexture2D(depth)
                        };
                        DestroyImmediate(color);
                        DestroyImmediate(depth);
                        colorDepthPrototypes.Add(colorDepthPrototype);

                        Repaint();
                        Event.current.Use();
                    }

                    GUI.Box(selectColorDepthRect, "");
                    GUI.Button(addColorDepthRect, new GUIContent("", "Color and Depth snap"));
                    GUI.DrawTexture(addColorDepthRect, CameraDoubleIcon);

                    if (horizontalOpened)
                        GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                    EditorGUILayout.EndScrollView();
                    GUILayout.EndHorizontal();
                    break;
            }
        }

        private void OnGUIGenerate()
        {
            SetupSelectedPrototypeMode();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Use prototype", EditorStyles.boldLabel, GUILayout.ExpandHeight(true), GUILayout.Height(28));
            selectedGenerateModeIndex = EditorGUILayout.Popup(selectedGenerateModeIndex, generateModeOptions, PopupStyle, GUILayout.ExpandHeight(true), GUILayout.Height(28));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            SelectPrototypes();

            if (stableDiffusionModule != null)
                stableDiffusionModule.OnGUI();

            EditorGUI.BeginDisabledGroup(generating);
            GUILayout.Space(5);
            if (generateProgress == 0)
            {
                if (GUILayout.Button(new GUIContent("Generate", GenerateIcon), GUILayout.ExpandHeight(true), GUILayout.Height(36)))
                {
                    generatingInitialized = true;
                    generating = true;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    GenerateAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                    CleanPreviewTexture();
                }
            }
            else
            {
                GUILayout.Box("Generating...", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.Height(36));
            }
            Rect generateButtonRect = GUILayoutUtility.GetLastRect();
            EditorGUI.EndDisabledGroup();

            if (generateProgress > 0)
            {
                float progressWidth = generateButtonRect.width - 75;
                Rect progressBarRect = new Rect(generateButtonRect.position, new Vector2(progressWidth, generateButtonRect.height));
                EditorGUI.ProgressBar(progressBarRect, generateProgress, generateProgressText);

                Rect cancelRect = new Rect(generateButtonRect.position + new Vector2(progressWidth + 3, 0), new Vector2(75 - 3, generateButtonRect.height));
                if (GUI.Button(cancelRect, "Cancel"))
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Cancel();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    generateProgress = 0;
                    generating = false;
                    generatingInitialized = false;
                }
            }

            GUILayout.Space(5);

            int numberOfHorizontalItems = Mathf.Max(1, (int)(position.width - 6) / 259);
            bool horizontalOpened = false;

            GUILayout.BeginVertical();
            generateScrollPosition = EditorGUILayout.BeginScrollView(generateScrollPosition);

            if (generating && previewTexture != null)
            {
                GUILayout.Space(1);
                int width = (int)position.width - 14;
                int height = (int)position.width * previewTexture.height / previewTexture.width;
                //workaround to get the image to expand
                GUILayout.Label("", GUILayout.Width(width), GUILayout.Height(height));
                GUI.DrawTexture(GUILayoutUtility.GetLastRect(), previewTexture);
            }

            if (generatedResults != null && generatedResults.Count > 0)
                for (int i = 0; i < generatedResults.Count; i++)
                {
                    if (i % numberOfHorizontalItems == 0)
                    {
                        GUILayout.BeginHorizontal();
                        horizontalOpened = true;
                        GUILayout.Space(2);
                    }

                    GUILayout.Space(3);

                    RepaintOnMouseMove();

                    GUILayout.Label(new GUIContent(generatedResults[i].Texture), new GUIStyle()
                    {
                        fixedHeight = 256,
                        fixedWidth = 256,
                    });

                    Rect lastRect = GUILayoutUtility.GetLastRect();
                    if (lastRect.Contains(Event.current.mousePosition))
                    {
                        Rect firstButtonRect = new Rect(lastRect.x + lastRect.width - 28, lastRect.y + 4, 24, 24);
                        if (GUI.Button(firstButtonRect, new GUIContent("", "Remove")))
                        {
                            if (generatedResults[i] != null)
                            {
                                generatedResults[i].Clear();
                            }
                            generatedResults.RemoveAt(i);
                        }
                        GUI.DrawTexture(firstButtonRect, CloseIcon);

                        Rect secondButtonRect = new Rect(lastRect.x + lastRect.width - 54, lastRect.y + 4, 24, 24);
                        if (GUI.Button(secondButtonRect, new GUIContent("", "Adjust")))
                        {
                            selectedTab++;
                            stableDiffusionResultForAdjustment = generatedResults[i].DeepClone();
                        }
                        GUI.DrawTexture(secondButtonRect, AdjustIcon);

                        Rect thirdButtonRect = new Rect(lastRect.x + lastRect.width - 80, lastRect.y + 4, 24, 24);
                        if (GUI.Button(thirdButtonRect, new GUIContent("", "Save")))
                        {
                            TextureUtilities.ExportGenerated(generatedResults[i].Texture);
                        }
                        GUI.DrawTexture(thirdButtonRect, SaveIcon);

                        Rect fourthButtonRect = new Rect(lastRect.x + lastRect.width - 106, lastRect.y + 4, 24, 24);
                        if (GUI.Button(fourthButtonRect, new GUIContent("", "Preview")))
                        {
                            PreviewWindow.Create("Preview", generatedResults[i].Texture);
                        }
                        GUI.DrawTexture(fourthButtonRect, PreviewIcon);
                    }

                    if (i % numberOfHorizontalItems == numberOfHorizontalItems - 1)
                    {
                        GUILayout.Space(3);
                        GUILayout.EndHorizontal();
                        horizontalOpened = false;
                        GUILayout.Space(3);
                    }
                }

            if (horizontalOpened)
                GUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void SetupSelectedPrototypeMode()
        {
            if (!generateModeSelected)
            {
                generateModeSelected = true;
                selectedGenerateModeIndex = selectedPrototypeTab + 1;
            }
        }

        private void SelectPrototypes()
        {
            if (previousSelectedGenerateModeIndex != selectedGenerateModeIndex)
            {
                previousSelectedGenerateModeIndex = selectedGenerateModeIndex;
                stableDiffusionMode = (StableDiffusionMode)selectedGenerateModeIndex;

                stableDiffusionModule.SetMode(stableDiffusionMode);
                AssignPrototypes();
            }
        }

        private void AssignPrototypes()
        {
            switch (stableDiffusionMode)
            {
                case StableDiffusionMode.None:
                    prototypes = null;
                    break;
                case StableDiffusionMode.Sketch:
                    prototypes = sketchPrototypes;
                    break;
                case StableDiffusionMode.Color:
                    prototypes = colorPrototypes;
                    break;
                case StableDiffusionMode.Depth:
                    prototypes = depthPrototypes;
                    break;
                case StableDiffusionMode.ColorDepth:
                    prototypes = colorDepthPrototypes;
                    break;
                default:
                    break;
            }
        }

        private bool adjusting = false;
        private float adjustProgress;
        private string adjustProgressText;
        private bool removeBackground;

        private List<Texture2D> finalTextures;

        private void OnGUIAdjust()
        {
            var selectedResult = stableDiffusionResultForAdjustment;
            if (selectedResult != null && selectedResult.Texture != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(3);
                if (GUILayout.Button(new GUIContent(selectedResult.Texture), new GUIStyle()
                {
                    fixedHeight = 256,
                    fixedWidth = 256,
                    contentOffset = new Vector2(2, 2),
                }))
                {
                    PreviewWindow.Create("Preview", selectedResult.Texture);
                }
                GUILayout.Space(4);
                GUILayout.BeginVertical();
                removeBackground = GUILayout.Toggle(removeBackground, "Remove background");

                GUILayout.BeginHorizontal();
                GUILayout.Label("Upscaler", EditorStyles.boldLabel, GUILayout.ExpandHeight(true), GUILayout.Height(28));
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

                if (GUILayout.Button(new GUIContent("Process", GenerateIcon), GUILayout.ExpandHeight(true), GUILayout.Height(36)))
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    AdjustAsync(selectedResult);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
                if (adjusting)
                {
                    Rect lastRect = GUILayoutUtility.GetLastRect();
                    EditorGUI.ProgressBar(lastRect, adjustProgress, adjustProgressText);
                }
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                int numberOfHorizontalItems = Mathf.Max(1, (int)(position.width) / 259);
                bool horizontalOpened = false;

                GUILayout.BeginVertical();
                adjustScrollPosition = EditorGUILayout.BeginScrollView(adjustScrollPosition);

                if (finalTextures != null && finalTextures.Count > 0)
                    for (int i = 0; i < finalTextures.Count; i++)
                    {
                        if (i % numberOfHorizontalItems == 0)
                        {
                            GUILayout.BeginHorizontal();
                            horizontalOpened = true;
                        }

                        GUILayout.Space(3);

                        RepaintOnMouseMove();

                        GUILayout.Label(new GUIContent(finalTextures[i]),
                                    new GUIStyle()
                                    {
                                        fixedHeight = 256,
                                        fixedWidth = 256,
                                    });

                        Rect lastRect = GUILayoutUtility.GetLastRect();
                        if (lastRect.Contains(Event.current.mousePosition))
                        {
                            Rect firstButtonRect = new Rect(lastRect.x + lastRect.width - 28, lastRect.y + 4, 24, 24);
                            if (GUI.Button(firstButtonRect, new GUIContent("", "Remove")))
                            {
                                if (finalTextures[i] != null)
                                {
                                    DestroyImmediate(finalTextures[i]);
                                    finalTextures[i] = null;
                                }
                                finalTextures.RemoveAt(i);
                            }
                            GUI.DrawTexture(firstButtonRect, CloseIcon);

                            Rect secondButtonRect = new Rect(lastRect.x + lastRect.width - 54, lastRect.y + 4, 24, 24);
                            if (GUI.Button(secondButtonRect, new GUIContent("", "Save")))
                            {
                                TextureUtilities.ExportGenerated(finalTextures[i]);
                            }
                            GUI.DrawTexture(secondButtonRect, SaveIcon);

                            Rect thirdButtonRect = new Rect(lastRect.x + lastRect.width - 80, lastRect.y + 4, 24, 24);
                            if (GUI.Button(thirdButtonRect, new GUIContent("", "Preview")))
                            {
                                PreviewWindow.Create("Preview", finalTextures[i]);
                            }
                            GUI.DrawTexture(thirdButtonRect, PreviewIcon);
                        }

                        if (i % numberOfHorizontalItems == numberOfHorizontalItems - 1)
                        {
                            GUILayout.Space(3);
                            GUILayout.EndHorizontal();
                            horizontalOpened = false;
                            GUILayout.Space(3);
                        }

                    }
                if (horizontalOpened)
                    GUILayout.EndHorizontal();
                EditorGUILayout.EndScrollView();
                GUILayout.EndVertical();
            }
        }

        private void RepaintOnMouseMove()
        {
            wantsMouseMove = true;
            if (Event.current.type == EventType.MouseMove)
                Repaint();
        }

        private void SetExactAdjustProgress(float adjustProgress, string adjustText)
        {
            this.adjustProgress = adjustProgress;
            this.adjustProgressText = adjustText;
        }

        public void UpdatePrototype(Prototype prototype)
        {
            if (prototype.Type == PrototypeType.Sketch)
            {
                UpdateSketch(prototype);
            }
            //else if (prototype.Type == PrototypeType.ColorSnap)
            //{
            //    UpdateColorSnap(prototype);
            //}
            //else if (prototype.Type == PrototypeType.DepthSnap)
            //{
            //    UpdateDepthSnap(prototype);
            //}

            Repaint();
        }

        private void UpdateSketch(Prototype sketch)
        {
            if (sketch.Type != PrototypeType.Sketch) return;

            int prototypeIndex = sketchPrototypes.FindIndex(p => p.Id == sketch.Id);
            if (prototypeIndex >= 0)
            {
                DestroyImmediate(sketchPrototypes[prototypeIndex].Texture);
                sketchPrototypes.RemoveAt(prototypeIndex);
                sketchPrototypes.Insert(prototypeIndex, sketch);
            }
            else
            {
                sketch.Id = ++lastSketchPrototypeId;
                sketchPrototypes.Add(sketch);
            }
        }

        //private void UpdateColorSnap(IPrototype colorSnap)
        //{

        //}

        //private void UpdateDepthSnap(IPrototype depthSnap)
        //{

        //}

        private void LoadIcons()
        {
            symbolCreatorIconData = System.IO.File.ReadAllBytes("Assets/UntoldByte/GAINS/Icons/symbol_creator_icon.png");
            adjustIconData = System.IO.File.ReadAllBytes("Assets/UntoldByte/GAINS/Icons/adjust_icon.png");
            cameraBlackIconData = System.IO.File.ReadAllBytes("Assets/UntoldByte/GAINS/Icons/camera_black_icon.png");
            cameraDoubleIconData = System.IO.File.ReadAllBytes("Assets/UntoldByte/GAINS/Icons/camera_double_icon.png");
            cameraWhiteIconData = System.IO.File.ReadAllBytes("Assets/UntoldByte/GAINS/Icons/camera_white_icon.png");
            closeIconData = System.IO.File.ReadAllBytes("Assets/UntoldByte/GAINS/Icons/close_icon.png");
            editIconData = System.IO.File.ReadAllBytes("Assets/UntoldByte/GAINS/Icons/edit_icon.png");
            generateIconData = System.IO.File.ReadAllBytes("Assets/UntoldByte/GAINS/Icons/generate_icon.png");
            plusIconData = System.IO.File.ReadAllBytes("Assets/UntoldByte/GAINS/Icons/plus_icon.png");
            previewIconData = System.IO.File.ReadAllBytes("Assets/UntoldByte/GAINS/Icons/preview_icon.png");
            refreshIconData = System.IO.File.ReadAllBytes("Assets/UntoldByte/GAINS/Icons/refresh_icon.png");
            saveIconData = System.IO.File.ReadAllBytes("Assets/UntoldByte/GAINS/Icons/save_icon.png");
        }

        private void LoadPrototypes()
        {
            LoadSketchPrototypes();
            LoadColorPrototypes();
            LoadDepthPrototypes();
            LoadColorDepthPrototypes();
            AssignPrototypes();
        }

        private void LoadSketchPrototypes()
        {
            string sketchPrototypesDirectory = "Assets/UntoldByte/GAINSExports/Prototypes/Sketch";
            sketchPrototypes = TextureUtilities.ReadPrototypes(sketchPrototypesDirectory, PrototypeType.Sketch).ToList();
            lastSketchPrototypeId = sketchPrototypes.Count;
        }

        private void LoadColorPrototypes()
        {
            string colorPrototypesDirectory = "Assets/UntoldByte/GAINSExports/Prototypes/Color";
            colorPrototypes = TextureUtilities.ReadPrototypes(colorPrototypesDirectory, PrototypeType.ColorSnap).ToList();
            lastColorPrototypeId = colorPrototypes.Count;
        }

        private void LoadDepthPrototypes()
        {
            string depthPrototypesDirectory = "Assets/UntoldByte/GAINSExports/Prototypes/Depth";
            depthPrototypes = TextureUtilities.ReadPrototypes(depthPrototypesDirectory, PrototypeType.DepthSnap).ToList();
            lastDepthPrototypeId = depthPrototypes.Count;
        }

        private void LoadColorDepthPrototypes()
        {
            string colorDepthPrototypesDirectory = "Assets/UntoldByte/GAINSExports/Prototypes/ColorDepth";
            colorDepthPrototypes = TextureUtilities.ReadPrototypes(colorDepthPrototypesDirectory, PrototypeType.ColorDepthSnap).ToList();
            lastColorDepthPrototypeId = colorDepthPrototypes.Count;
        }

        private void SavePrototypes()
        {
            SaveSketchPrototypes();
            SaveColorPrototypes();
            SaveDepthPrototypes();
            SaveColorDepthPrototypes();
        }

        private void SaveSketchPrototypes()
        {
            string sketchPrototypesDirectory = "Assets/UntoldByte/GAINSExports/Prototypes/Sketch";
            if (sketchPrototypes != null)
                TextureUtilities.SavePrototypes(sketchPrototypes, sketchPrototypesDirectory);
        }

        private void SaveColorPrototypes()
        {
            string colorPrototypesDirectory = "Assets/UntoldByte/GAINSExports/Prototypes/Color";
            if (colorPrototypes != null)
                TextureUtilities.SavePrototypes(colorPrototypes, colorPrototypesDirectory);
        }

        private void SaveDepthPrototypes()
        {
            string depthPrototypesDirectory = "Assets/UntoldByte/GAINSExports/Prototypes/Depth";
            if (depthPrototypes != null)
                TextureUtilities.SavePrototypes(depthPrototypes, depthPrototypesDirectory);
        }

        private void SaveColorDepthPrototypes()
        {
            string colorDepthPrototypesDirectory = "Assets/UntoldByte/GAINSExports/Prototypes/ColorDepth";
            if (colorDepthPrototypes != null)
                TextureUtilities.SavePrototypes(colorDepthPrototypes, colorDepthPrototypesDirectory);
        }

        private void InitializeAssets()
        {
            if (assetsInitialized) return;

            assetsInitialized = true;

            LoadIcons();
            LoadPrototypes();
        }

        private void InitializeSDClient()
        {
            if (sdClientInitialized) return;

            sdClientInitialized = true;

            #region SDClient
            LoadSettings();
            stableDiffusionModule = new StableDiffusionModule();
            stableDiffusionModule.WindowSetup(this);
            #endregion SDClient

            #region Adjust
            RefreshUpscalers();
            #endregion Adjust
        }

        private void SetWindowTitle()
        {
            if (titleContent == null)
            {
                titleContent = new GUIContent("Symbol Creator", SymbolCreatorIcon);
                return;
            }

            if (titleContent.text == null || titleContent.text.IndexOf('.') > 0)
                titleContent.text = "Symbol Creator";

            if (titleContent.image == null)
                titleContent.image = SymbolCreatorIcon;
        }

        private bool UpscalersLoaded()
        {
            if (upscalers != null && upscalers.Length > 0)
                return true;
            return !gettingUpscalers;
        }

        private void RepaintSceneView()
        {
            if (previousSelectedTab != selectedTab || previousSelectedPrototypeTab != selectedPrototypeTab)
            {
                SceneView.RepaintAll();
                previousSelectedTab = selectedTab;
                previousSelectedPrototypeTab = selectedPrototypeTab;
            }
        }

        private void OnDestroy()
        {
            SavePrototypes();
        }

        #region IHostEditorWindow

        bool IHostEditorWindow.GeneratingInitialized => generatingInitialized;

        void IHostEditorWindow.RefreshUI()
        {
            Repaint();
        }

        void IHostEditorWindow.UpdateProgress(float generateProgress, string generateProgressText)
        {
            this.generateProgress = generateProgress;
            this.generateProgressText = generateProgressText;
        }

        void IHostEditorWindow.UpdatePreview(bool generating, Texture2D previewTexture)
        {
            this.generating = generating;
            if (previewTexture != null)
            {
                CleanGeneratedTextures();
                CleanPreviewTexture();
                this.previewTexture = previewTexture;
            }
        }

        void IHostEditorWindow.Update(bool generating, List<StableDiffusionResult> generatedResults)
        {
            this.generating = generating;
            generatingInitialized = generating;
            if (generatedResults != null && generatedResults.Count > 0)
            {
                CleanGeneratedTextures();
                this.generatedResults = generatedResults;
            }
        }

        Texture2D IHostEditorWindow.PackControlnetImage()
        {
            Texture2D packedTexture = null;

            if (stableDiffusionMode == StableDiffusionMode.Sketch || stableDiffusionMode == StableDiffusionMode.Color || stableDiffusionMode == StableDiffusionMode.Depth)
            {
                packedTexture = TextureUtilities.PackPrototypes(Prototypes);

                if (stableDiffusionMode == StableDiffusionMode.Sketch && stableDiffusionModule.StableDiffusionModuleData.controlnetPreprocessorIndex == 0)
                {
                    Texture2D invertedColorTexture = TextureUtilities.InvertInputColors(packedTexture);
                    DestroyImmediate(packedTexture);
                    packedTexture = invertedColorTexture;
                }
            }

            return packedTexture;
        }

        #endregion IHostEditorWindow

        async Task GenerateAsync()
        {
            if (Prototypes == null || Prototypes.Count == 0)
            {
                EditorUtility.DisplayDialog("No Prototypes", "Plesea add some prototypes of selected type, or select type of prototype that has some captured", "OK");
                generateProgress = 0;
                generating = false;
                generatingInitialized = false;
                return;
            }

            Texture2D packedTexture = null;
            Texture2D packedImageToImageTexture = null;

            if (stableDiffusionMode == StableDiffusionMode.Sketch || stableDiffusionMode == StableDiffusionMode.Color || stableDiffusionMode == StableDiffusionMode.Depth)
            {
                packedTexture = ((IHostEditorWindow)this).PackControlnetImage(); //TextureUtilities.PackPrototypes(Prototypes);

                //if (stableDiffusionMode == StableDiffusionMode.Sketch && stableDiffusionModule.StableDiffusionModuleData.controlnetPreprocessorIndex == 0)
                //{
                //    Texture2D invertedColorTexture = TextureUtilities.InvertInputColors(packedTexture);
                //    DestroyImmediate(packedTexture);
                //    packedTexture = invertedColorTexture;
                //}

                stableDiffusionModule.SetControlnetPicture(packedTexture);
                stableDiffusionModule.SetNumberOfTiles(Prototypes.Count);
            }

            //if (stableDiffusionMode == StableDiffusionMode.ColorDepth)
            //{
            //    packedImageToImageTexture = TextureUtilities.PackPrototypes(Prototypes);
            //    stableDiffusionModule.SetImageToImagePicture(packedImageToImageTexture);

            //    packedTexture = TextureUtilities.PackPrototypesSecondImage(Prototypes);
            //    stableDiffusionModule.SetControlnetPicture(packedTexture);
            //    stableDiffusionModule.SetNumberOfTiles(Prototypes.Count);
            //}

            SetHostOnStableDiffusionModule();

            await stableDiffusionModule.MakeRequest();

            if (packedTexture != null)
                DestroyImmediate(packedTexture);

            if (packedImageToImageTexture != null)
                DestroyImmediate(packedImageToImageTexture);
        }

        async Task Cancel()
        {
            await stableDiffusionModule.Cancel();
        }

        void CleanGeneratedTextures()
        {
            if (generatedResults == null || generatedResults.Count == 0) return;

            foreach (StableDiffusionResult generatedResult in generatedResults)
            {
                generatedResult.Clear();
            }

            generatedResults = null;
        }

        void CleanPreviewTexture()
        {
            if (previewTexture == null) return;
            DestroyImmediate(previewTexture);
        }

        private async Task AdjustAsync(StableDiffusionResult selectedResult)
        {
            adjusting = true;
            SetExactAdjustProgress(0, "Unpacking");

            finalTextures = TextureUtilities.UnpackTexture(selectedResult.Texture, Prototypes == null ? 1 : Prototypes.Count).ToList();
            SetExactAdjustProgress(0, "Upscaling");

            if (finalTextures.Count != 1)
                finalTextures = (await CommonUtilities.UpscaleImages(finalTextures, Upscaler)).ToList();

            SetExactAdjustProgress(0, "Removing background");

            if (removeBackground)
            {
                finalTextures = (await stableDiffusionModule.RemBg(finalTextures)).ToList();
            }

            adjusting = false;
            SetExactAdjustProgress(1, "Finished");
            Repaint();

            //GUIUtility.ExitGUI();
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

            Repaint();
        }

        void SetupUpscaler()
        {
            upscalerIndex = upscalers.ToList().FindIndex(upscaler => upscaler.Equals(Upscaler, StringComparison.Ordinal));
            if (upscalerIndex < 0)
                upscalerIndex = 0;
        }

        private void SetHostOnStableDiffusionModule()
        {
            stableDiffusionModule.SetHost(this);
        }
    }

}