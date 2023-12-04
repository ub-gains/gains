using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using static UntoldByte.GAINS.Editor.CommonUtilities;

namespace UntoldByte.GAINS.Editor
{
    public class EntityPainter : EditorWindow, IHostEditorWindow, IEntityMaterialManagerHost
    {
        #region Icons backing fields and properties

        private byte[] entityPainterIconData;
        private byte[] adjustIconData;
        private byte[] closeIconData;
        private byte[] generateIconData;
        private byte[] plusIconData;
        private byte[] previewIconData;
        private byte[] tryIconData;

        private Texture2D entity_painter_icon;
        private Texture2D adjust_icon;
        private Texture2D close_icon;
        private Texture2D generate_icon;
        private Texture2D plus_icon;
        private Texture2D preview_icon;
        private Texture2D try_icon;

        private Texture2D EntityPainterIcon
        {
            get
            {
                if (entity_painter_icon != null) return entity_painter_icon;
                if (entityPainterIconData == null) return null;
                entity_painter_icon = TextureUtilities.ByteArrayToTexture2D(entityPainterIconData);
                return entity_painter_icon;
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
        private Texture2D TryIcon
        {
            get
            {
                if (try_icon != null) return try_icon;
                if (tryIconData == null) return null;
                try_icon = TextureUtilities.ByteArrayToTexture2D(tryIconData);
                return try_icon;
            }
        }

        #endregion Icons backing fields and properties

        private EntityPainterData entityPainterData;
        internal EntityPainterData EntityPainterData {
            set
            {
                entityPainterData = value;
            }
            get
            {
                if (entityPainterData != null) return entityPainterData;

                EntityPainterData tmpEntityPainterData = CreateInstance<EntityPainterData>();
                if (!ReferenceEquals(entityPainterData, (EntityPainterData)null))
                    EditorUtility.CopySerializedManagedFieldsOnly(entityPainterData, tmpEntityPainterData);
                entityPainterData = tmpEntityPainterData;

                return entityPainterData;
            }
        }

        private bool windowVisible;
        private bool SceneFrameVisible => selectedTab == 0 && windowVisible;
#pragma warning disable IDE1006 // Naming Styles
        private GameObject gameObject { get { return EntityPainterData.gameObject; } set { EntityPainterData.gameObject = value; } }
#pragma warning restore IDE1006 // Naming Styles
        private bool gameObjectPopulated;
        private bool gameObjectMeshPresent;

        private Vector2 prototypesScrollPosition;
        private List<EntityPrototype> prototypes = new List<EntityPrototype>();
        private int lastPrototypeId;

        private bool generatingInitialized = false;
        private bool generating = false;
        private float generateProgress;
        private string generateProgressText;
        private Texture2D previewTexture;
        private List<StableDiffusionResult> generatedResults;

        private ResultBinaryData[] bakedColorResultArraysData;
        private ResultBinaryData[] bakedDepthResultArraysData;
        private TextureFormat bakedColorResultArrayTextureFormat;
        private TextureFormat bakedDepthResultArrayTextureFormat;
        private Vector2 bakedColorResultArrayTextureDimensions;
        private Vector2 bakedDepthResultArrayTextureDimensions;
        private List<Texture2DArray> bakedColorResultArrays;
        private List<Texture2DArray> bakedDepthResultArrays;
        private List<Texture2DArray> BakedColorResultArrays
        {
            get
            {
                if (bakedColorResultArrays != null && bakedColorResultArrays.Count > 0 && bakedColorResultArrays.All(bakedColorResult => bakedColorResult != null))
                    return bakedColorResultArrays;
                if (bakedColorResultArraysData == null)
                    return null;
                bakedColorResultArrays = TextureUtilities.ToTexture2DArrayList(bakedColorResultArraysData.Select(x => x.projections.Select(y => y.imageData).ToArray()).ToList(),
                    bakedColorResultArrayTextureFormat, (int)bakedColorResultArrayTextureDimensions.x, (int)bakedColorResultArrayTextureDimensions.y);
                return bakedColorResultArrays;
            }
            set
            {
                if (value == null) return;
                Texture2DArray first = value.FirstOrDefault();
                if (first == null) return;
                bakedColorResultArrayTextureFormat = first.format;
                bakedColorResultArrayTextureDimensions = new Vector2(first.width, first.height);
                if (bakedColorResultArrays != null)
                    foreach (Texture2DArray bakedColorResult in bakedColorResultArrays)
                        DestroyImmediate(bakedColorResult);
                bakedColorResultArrays = null;
                bakedColorResultArraysData = TextureUtilities.ToByteArrayList(value).Select(x => new ResultBinaryData { projections = x.Select(y => new ProjectionBinaryData { imageData = y }).ToArray() }).ToArray();
            }
        }
        private List<Texture2DArray> BakedDepthResultArrays
        {
            get
            {
                if (bakedDepthResultArrays != null && bakedDepthResultArrays.Count > 0 && bakedDepthResultArrays.All(bakedDepthResult => bakedDepthResult != null))
                    return bakedDepthResultArrays;
                if (bakedDepthResultArraysData == null)
                    return null;
                bakedDepthResultArrays = TextureUtilities.ToTexture2DArrayList(bakedDepthResultArraysData.Select(x => x.projections.Select(y => y.imageData).ToArray()).ToList(),
                    bakedDepthResultArrayTextureFormat, (int)bakedDepthResultArrayTextureDimensions.x, (int)bakedDepthResultArrayTextureDimensions.y);
                return bakedDepthResultArrays;
            }
            set
            {
                if (value == null) return;
                Texture2DArray first = value.FirstOrDefault();
                if (first == null) return;
                bakedDepthResultArrayTextureFormat = first.format;
                bakedDepthResultArrayTextureDimensions = new Vector2(first.width, first.height);
                if (bakedDepthResultArrays != null)
                    foreach (Texture2DArray bakedDepthResult in bakedDepthResultArrays)
                        DestroyImmediate(bakedDepthResult);
                bakedDepthResultArrays = null;
                bakedDepthResultArraysData = TextureUtilities.ToByteArrayList(value).Select(x => new ResultBinaryData { projections = x.Select(y => new ProjectionBinaryData { imageData = y }).ToArray() }).ToArray();
            }
        }
        private Vector2 generateScrollPosition;

        private bool assetsInitialized = false;
        private bool sdClientInitialized = false;
        private StableDiffusionModule stableDiffusionModule;

        private int upscalerIndex = 0;
        private string[] upscalers = Array.Empty<string>();
        private bool gettingUpscalers = false;

        private int previousSelectedTab;
        private int selectedTab;
        private readonly string[] tabNames = { "Prototype", "Generate", "Adjust" };

        [NonSerialized]
        private bool adjustmentInitialized = false;
        private EntityMaterialManagerModule entityMaterialManagerModule;
        private bool quitting = false;

        [MenuItem("Window/UntoldByte/Entity Painter")]
        private static void ShowWindow()
        {
            EntityPainter window = GetWindow<EntityPainter>();
            window.Focus();
            window.Repaint();
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            Undo.undoRedoPerformed += UndoRedoPerformed;
            EditorApplication.playModeStateChanged += PlayModeStateChange;
            EditorApplication.quitting += Quitting;
            AssemblyReloadEvents.afterAssemblyReload += AfterAssemblyReload;

            Repaint();
            LoadScriptableState();
            SelectGameObject();
        }

        private void OnDisable()
        {
            if(!quitting)
                SaveScriptableState();

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
                adjustmentInitialized = false;
                Repaint();
            }

            SetWindowTitle();
            Repaint();
        }

        private void Quitting()
        {
            SavePrototypes();
            SaveScriptableState();

            quitting = true;
        }

        private void AfterAssemblyReload()
        {
            if (!UpscalersLoaded())
                sdClientInitialized = false;

            SetWindowTitle();
            Repaint();
        }

        private void OnGUI()
        {
            InitializeAssets();
            InitializeSDClient();

            RepaintSceneView();

            wantsMouseMove = false;
            windowVisible = true;

            if (titleContent.image == null) SetWindowTitle();
            if (gameObjectPopulated && gameObject == null) SelectGameObject();

            EditorGUI.BeginChangeCheck();
            GameObject tmpGameObject = gameObject;

            GUILayout.Space(5);
            tmpGameObject = (GameObject)EditorGUILayout.ObjectField("Object to texture", tmpGameObject, typeof(GameObject), true);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(EntityPainterData, "Object to texture");

                ChangeEntityMaterialManagerSelected(false);
                gameObject = tmpGameObject;
                gameObjectPopulated = gameObject != null;
                EnsureEntityMaterialManagerExists();
                ChangeEntityMaterialManagerSelected(true);
                adjustmentInitialized = false;

                EditorUtility.SetDirty(EntityPainterData);
            }
            RemoveReferenceToGameObjectIfRemoved();
            GUILayout.Space(5);
            selectedTab = GUILayout.Toolbar(selectedTab, tabNames, GUILayout.Height(28));
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
            gameObjectMeshPresent = MeshUtilities.GetSharedMesh(gameObject) != null;

            GUILayout.BeginHorizontal();
            if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.T)
            {
                Texture2D packedTexture = TextureUtilities.PackPrototypes(prototypes, Color.black);
                PreviewWindow.Create("Packed depths", packedTexture, destroyTextureOnClose: true);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(3);
            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            prototypesScrollPosition = EditorGUILayout.BeginScrollView(prototypesScrollPosition);
            int numberOfHorizontalItems = Mathf.Max(1, (int)(position.width - 5) / 130);
            GUILayout.BeginVertical();
            bool horizontalOpened = false;
            for (int i = 0; i < prototypes.Count; i++)
            {
                if (i % numberOfHorizontalItems == 0)
                {
                    GUILayout.BeginHorizontal();
                    horizontalOpened = true;
                }

                if (prototypes[i].Texture != null)
                {
                    RepaintOnMouseMove();

                    GUILayout.Label(new GUIContent(prototypes[i].Texture),
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
                            if (prototypes[i].Texture != null)
                                DestroyImmediate(prototypes[i].Texture);
                            if (prototypes[i].SecondTexture != null)
                                DestroyImmediate(prototypes[i].SecondTexture);
                            prototypes[i].UVs = null;
                            prototypes.RemoveAt(i);
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
            EditorGUI.BeginDisabledGroup(!gameObjectMeshPresent);
            if (GUI.Button(addSnapRect, "") || (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.BackQuote)) //button is visualy behind but is processing clicks
            {
                Camera camera = CameraUtilities.GetSceneViewCamera();
                Vector4[] UVs = MeshUtilities.GenerateSceneProjectionUVs(camera, gameObject);
                Vector2 nearFar = MeshUtilities.NearFarDepth(UVs);
                Texture2D depth = CameraUtilities.GetCameraDepthTexture2D(gameObject, camera, 512, 512, nearFar.x * 0.9f, nearFar.y * 1.1f);
                Texture2D depthNormal = CameraUtilities.GetCameraDepthNormalTexture2D(gameObject, camera, 512, 512, nearFar.x * 0.9f, nearFar.y * 1.1f);
                CameraUtilities.ReleaseSceneViewCamera();
                EntityPrototype prototype = new EntityPrototype
                {
                    Id = ++lastPrototypeId,
                    Type = PrototypeType.DepthSnap,
                    Texture = depth,
                    SecondTexture = depthNormal,
                    UVs = UVs
                };
                prototypes.Add(prototype);

                Repaint();
                Event.current.Use();
            }

            GUI.Box(selectSnapRect, "");
            GUI.Button(addSnapRect, new GUIContent("", "Take a snap"));
            if (PlusIcon != null)
                GUI.DrawTexture(addSnapRect, PlusIcon);
            EditorGUI.EndDisabledGroup();

            if (horizontalOpened)
                GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            GUILayout.EndHorizontal();
        }

        private void OnGUIGenerate()
        {
            if (stableDiffusionModule != null)
                stableDiffusionModule.OnGUI();

            EditorGUI.BeginDisabledGroup(generating);
            GUILayout.Space(5);
            if (generateProgress == 0)
            {
                if (GUILayout.Button(new GUIContent("Generate", GenerateIcon), GUILayout.ExpandHeight(true), GUILayout.Height(36)))
                {
                    generatingInitialized = true;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Generate();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                    generating = true;
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

                            if (BakedColorResultArrays != null && BakedColorResultArrays.Count > i)
                            {
                                DestroyImmediate(BakedColorResultArrays[i]);
                                BakedColorResultArrays.RemoveAt(i);
                            }

                            if (BakedDepthResultArrays != null && BakedDepthResultArrays.Count > i)
                            {
                                DestroyImmediate(BakedDepthResultArrays[i]);
                                BakedDepthResultArrays.RemoveAt(i);
                            }
                        }
                        GUI.DrawTexture(firstButtonRect, CloseIcon);

                        Rect secondButtonRect = new Rect(lastRect.x + lastRect.width - 54, lastRect.y + 4, 24, 24);
                        if (GUI.Button(secondButtonRect, new GUIContent("", "Adjust")))
                        {
                            selectedTab++;
                            TryOutProjection(i);
                        }
                        GUI.DrawTexture(secondButtonRect, AdjustIcon);

                        Rect thirdButtonRect = new Rect(lastRect.x + lastRect.width - 80, lastRect.y + 4, 24, 24);
                        if (GUI.Button(thirdButtonRect, new GUIContent("", "Test")))
                        {
                            TryOutProjection(i);
                        }
                        GUI.DrawTexture(thirdButtonRect, TryIcon);

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

        private void OnGUIAdjust()
        {
            InitializeAdjustmentIfNeeded();

            if (entityMaterialManagerModule != null)
                entityMaterialManagerModule.OnGUI();
        }

        private void InitializeAdjustmentIfNeeded()
        {
            if (adjustmentInitialized) return;
            adjustmentInitialized = true;

            SetupEntityMaterilManagerModule();
        }

        private void SetupEntityMaterilManagerModule()
        {
            if (gameObject == null) return;
            EntityMaterialManager entityMaterialManager = gameObject.GetComponent<EntityMaterialManager>();
            if (entityMaterialManager == null)
            {
                entityMaterialManagerModule = null;
                return;
            }

            if(entityMaterialManagerModule == null)
                entityMaterialManagerModule = new EntityMaterialManagerModule();
            entityMaterialManagerModule.SetHost(this);
            entityMaterialManagerModule.SetTarget(entityMaterialManager);
        }

        private EntityMaterialManager EnsureEntityMaterialManagerExists()
        {
            if (gameObject == null) return null;
            if (!MeshUtilities.CheckGameObjectRequirements(gameObject)) return null;
            if (gameObject.TryGetComponent(out EntityMaterialManager entityMaterialManager)) return entityMaterialManager;

            return gameObject.AddComponent<EntityMaterialManager>();
        }

        private void ChangeEntityMaterialManagerSelected(bool selected)
        {
            if (gameObject == null) return;
            if (!gameObject.TryGetComponent(out EntityMaterialManager entityMaterialManager)) return;
            entityMaterialManager.selected = selected;
        }

        private void RemoveReferenceToGameObjectIfRemoved()
        {
            if(gameObject != null && !gameObject.TryGetComponent<EntityMaterialManager>(out _))
            {
                gameObject = null;
                gameObjectPopulated = false;
                adjustmentInitialized = false;
            }
        }

        private void RepaintOnMouseMove()
        {
            wantsMouseMove = true;
            if (Event.current.type == EventType.MouseMove)
                Repaint();
        }

        private void LoadIcons()
        {
            entityPainterIconData = System.IO.File.ReadAllBytes("Assets/UntoldByte/GAINS/Icons/entity_painter_icon.png");
            adjustIconData = System.IO.File.ReadAllBytes("Assets/UntoldByte/GAINS/Icons/adjust_icon.png");
            closeIconData = System.IO.File.ReadAllBytes("Assets/UntoldByte/GAINS/Icons/close_icon.png");
            generateIconData = System.IO.File.ReadAllBytes("Assets/UntoldByte/GAINS/Icons/generate_icon.png");
            plusIconData = System.IO.File.ReadAllBytes("Assets/UntoldByte/GAINS/Icons/plus_icon.png");
            previewIconData = System.IO.File.ReadAllBytes("Assets/UntoldByte/GAINS/Icons/preview_icon.png");
            tryIconData = System.IO.File.ReadAllBytes("Assets/UntoldByte/GAINS/Icons/try_icon.png");
        }

        private void LoadPrototypes()
        {
            string entityPrototypesDirectory = "Assets/UntoldByte/GAINSExports/Prototypes/Entity";
            prototypes = TextureUtilities.ReadPrototypes(entityPrototypesDirectory).ToList();
            lastPrototypeId = prototypes.Count;
        }

        private void SavePrototypes()
        {
            string entityPrototypesDirectory = "Assets/UntoldByte/GAINSExports/Prototypes/Entity";
            if (prototypes != null)
                TextureUtilities.SavePrototypes(prototypes, entityPrototypesDirectory);
        }

        private void SelectGameObject()
        {
            if (gameObject != null) return;

#if UNITY_2020_3_45_OR_NEWER || UNITY_2021_3_18_OR_NEWER || UNITY_2022_2_5_OR_NEWER || UNITY_2023_1_OR_NEWER
            List<EntityMaterialManager> entityMaterialManagers = FindObjectsByType<EntityMaterialManager>(FindObjectsSortMode.None).ToList();
#else
            List<EntityMaterialManager> entityMaterialManagers = FindObjectsOfType<EntityMaterialManager>().ToList();
#endif

            EntityMaterialManager entityMaterialManager = entityMaterialManagers.FirstOrDefault(emm => emm.selected);

            if (entityMaterialManager == null) return;

            gameObject = entityMaterialManager.gameObject;
            gameObjectPopulated = gameObject != null;

            foreach(EntityMaterialManager emm in entityMaterialManagers.Where(emm => emm != entityMaterialManager && emm.selected))
            {
                emm.selected = false;
            }
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

            stableDiffusionModule.SetupReferences(this);
            stableDiffusionModule.RefreshLists();
            stableDiffusionModule.LoadIcons();
            stableDiffusionModule.SetMode(StableDiffusionMode.Depth);
            #endregion SDClient

            #region Adjust
            RefreshUpscalers();
            #endregion Adjust
        }

        private void SetWindowTitle()
        {
            if(titleContent == null)
            {
                titleContent = new GUIContent("Entity Painter", EntityPainterIcon);
                return;
            }

            if (titleContent.text == null || titleContent.text.IndexOf('.') > 0)
                titleContent.text = "Entity Painter";

            if(titleContent.image == null)
                titleContent.image = EntityPainterIcon;
        }

        private bool UpscalersLoaded()
        {
            if (upscalers != null && upscalers.Length > 0)
                return true;
            return !gettingUpscalers;
        }

        private void RepaintSceneView()
        {
            if (previousSelectedTab != selectedTab)
            {
                SceneView.RepaintAll();
                previousSelectedTab = selectedTab;
            }
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
                DestroyMaterialTextures();
                CreateMaterialTextures();
            }
        }

        Texture2D IHostEditorWindow.PackControlnetImage()
        {
            Texture2D packedTexture = TextureUtilities.PackPrototypes(prototypes);

            return packedTexture;
        }

        #endregion IHostEditorWindow

        async Task Generate()
        {
            Texture2D packedTexture = ((IHostEditorWindow)this).PackControlnetImage(); //TextureUtilities.PackPrototypes(prototypes);

            stableDiffusionModule.SetControlnetPicture(packedTexture);
            stableDiffusionModule.SetNumberOfTiles(prototypes.Count);

            SetHostOnStableDiffusionModule();

            await stableDiffusionModule.MakeRequest();

            DestroyImmediate(packedTexture);
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

        private void DestroyMaterialTextures()
        {
            for (int i = BakedColorResultArrays.Count - 1; i >= 0; i--)
            {
                DestroyImmediate(BakedColorResultArrays[i]);
                BakedColorResultArrays.RemoveAt(i);
            }

            for (int i = BakedDepthResultArrays.Count - 1; i >= 0; i--)
            {
                DestroyImmediate(BakedDepthResultArrays[i]);
                BakedDepthResultArrays.RemoveAt(i);
            }
        }

        private void CreateMaterialTextures()
        {
            Mesh sharedMesh = MeshUtilities.GetSharedMesh(gameObject);
            if (sharedMesh == null)
            {
                EditorUtility.DisplayDialog("Object not present", "Object to textue field is empty, skipping generating projection textures. In order to apply projection textures populate Object to texture field", "OK");
                return;
            }

            List<Vector4[]> UVs = prototypes.Select(p => p.UVs).ToList();
            if (UVs.Count == 0)
            {
                EditorUtility.DisplayDialog("Prototype UVs not present", "Prototype UVs are not present. Make sure to keep prototypes in order to be able to generate projection textures.", "OK");
                return;
            }

            if (generatedResults.Count == 0)
            {
                EditorUtility.DisplayDialog("Image(s) not generated", "Skipping projection texture generation, no images have been generated", "OK");
                return;
            }

            Mesh meshInstance = Instantiate(sharedMesh);

            List<Texture2DArray> bakedColorTextureArrays = new List<Texture2DArray>();
            List<Texture2DArray> bakedDepthTextureArrays = new List<Texture2DArray>();

            foreach (var item in generatedResults)
            {
                Texture2D[] colorProjections = TextureUtilities.UnpackTexture(item.Texture, prototypes.Count);
                Texture2D[] depthProjections = prototypes.Select(p => p.SecondTexture).ToArray();
                int size = Math.Min(colorProjections.Length, depthProjections.Length);

                Texture2D[] bakedColorProjections = new Texture2D[size];
                Texture2D[] bakedDepthProjections = new Texture2D[size];
                for (int i = 0; i < size; i++)
                {
                    meshInstance.SetUVs(1, UVs[i]);

                    Material colorProjectionMaterial = TextureUtilities.CreateColorProjectionMaterial(colorProjections[i]);
                    Texture2D bakedColorTexture = TextureUtilities.BakeTexture(meshInstance, colorProjectionMaterial, item.Texture.width, item.Texture.height, 0, 0);
                    bakedColorProjections[i] = bakedColorTexture;
                    DestroyImmediate(colorProjectionMaterial);

                    Material dapthProjectionMaterial = TextureUtilities.CreateDepthProjectionMaterial(depthProjections[i]);
                    Texture2D bakedDepthTexture = TextureUtilities.BakeTexture(meshInstance, dapthProjectionMaterial, item.Texture.width, item.Texture.height, 0, 0, TextureFormat.RGBAHalf);
                    bakedDepthProjections[i] = bakedDepthTexture;
                    DestroyImmediate(dapthProjectionMaterial);
                }

                Texture2DArray bakedColorTextureArray = TextureUtilities.PackTextureArray(bakedColorProjections);
                bakedColorTextureArrays.Add(bakedColorTextureArray);

                Texture2DArray bakedDepthTextureArray = TextureUtilities.PackTextureArray(bakedDepthProjections);
                bakedDepthTextureArrays.Add(bakedDepthTextureArray);

                foreach (Texture2D colorProjection in colorProjections)
                    DestroyImmediate(colorProjection);

                foreach (Texture2D bakedColorProjection in bakedColorProjections)
                    DestroyImmediate(bakedColorProjection);

                foreach (Texture2D bakedDepthProjection in bakedDepthProjections)
                    DestroyImmediate(bakedDepthProjection);
            }

            DestroyImmediate(meshInstance);

            BakedColorResultArrays = bakedColorTextureArrays;
            BakedDepthResultArrays = bakedDepthTextureArrays;
        }

        private void TryOutProjection(int index)
        {
            if (gameObject == null)
            {
                EditorUtility.DisplayDialog("Object not present", "Object to texture field is empty, please populate the field with scene object to texture", "OK");
                return;
            }

            if((BakedColorResultArrays == null || BakedColorResultArrays.Count == 0) && (generatedResults != null && generatedResults.Count > 0))
            {
                CreateMaterialTextures();
            }

            EntityMaterialManager entityMaterialManager = EnsureEntityMaterialManagerExists();

#pragma warning disable IDE0017 // Simplify object initialization
            Material material = new Material(Shader.Find("Hidden/UntoldByte/GAINS/ColorDepthProjectionShader"));
#pragma warning restore IDE0017 // Simplify object initialization
            material.name = "Color Depth Projection Material";
            material.SetFloat("_NumberOfProjections", BakedColorResultArrays[index].depth);
            material.SetTexture("_ProjectionColor", BakedColorResultArrays[index]);
            material.SetTexture("_ProjectionDepth", BakedDepthResultArrays[index]);
            material.renderQueue += 1;

            entityMaterialManager.SetProjectionMaterial(material);
        }

        private void LoadScriptableState()
        {
            entityPainterIconData = EntityPainterStateScriptableObject.instance.entityPainterIconData;
            adjustIconData = EntityPainterStateScriptableObject.instance.adjustIconData;
            closeIconData = EntityPainterStateScriptableObject.instance.closeIconData;
            generateIconData = EntityPainterStateScriptableObject.instance.generateIconData;
            plusIconData = EntityPainterStateScriptableObject.instance.plusIconData;
            previewIconData = EntityPainterStateScriptableObject.instance.previewIconData;
            tryIconData = EntityPainterStateScriptableObject.instance.tryIconData;

            windowVisible = EntityPainterStateScriptableObject.instance.windowVisible;

            gameObject = EntityPainterStateScriptableObject.instance.gameObject;
            gameObjectPopulated = EntityPainterStateScriptableObject.instance.gameObjectPopulated;
            prototypesScrollPosition = EntityPainterStateScriptableObject.instance.prototypeScrollPosition;
            prototypes = EntityPainterStateScriptableObject.instance.prototypes;
            lastPrototypeId = EntityPainterStateScriptableObject.instance.lastPrototypeId;

            generatedResults = EntityPainterStateScriptableObject.instance.generatedResults;

            bakedColorResultArraysData = EntityPainterStateScriptableObject.instance.bakedColorResultArraysData;
            bakedDepthResultArraysData = EntityPainterStateScriptableObject.instance.bakedDepthResultArraysData;
            bakedColorResultArrayTextureFormat = EntityPainterStateScriptableObject.instance.bakedColorResultArrayTextureFormat;
            bakedDepthResultArrayTextureFormat = EntityPainterStateScriptableObject.instance.bakedDepthResultArrayTextureFormat;
            bakedColorResultArrayTextureDimensions = EntityPainterStateScriptableObject.instance.bakedColorResultArrayTextureDimensions;
            bakedDepthResultArrayTextureDimensions = EntityPainterStateScriptableObject.instance.bakedDepthResultArrayTextureDimensions;

            assetsInitialized = EntityPainterStateScriptableObject.instance.assetsInitialized;
            sdClientInitialized = EntityPainterStateScriptableObject.instance.sdClientInitialized;
            stableDiffusionModule = EntityPainterStateScriptableObject.instance.stableDiffusionModule;

            selectedTab = EntityPainterStateScriptableObject.instance.selectedTab;

            if (stableDiffusionModule != null)
                stableDiffusionModule.SetupReferences(this);
        }

        private void SaveScriptableState()
        {
            EntityPainterStateScriptableObject.instance.entityPainterIconData = entityPainterIconData;
            EntityPainterStateScriptableObject.instance.adjustIconData = adjustIconData;
            EntityPainterStateScriptableObject.instance.closeIconData = closeIconData;
            EntityPainterStateScriptableObject.instance.generateIconData = generateIconData;
            EntityPainterStateScriptableObject.instance.plusIconData = plusIconData;
            EntityPainterStateScriptableObject.instance.previewIconData = previewIconData;
            EntityPainterStateScriptableObject.instance.tryIconData = tryIconData;

            EntityPainterStateScriptableObject.instance.windowVisible = windowVisible;

            EntityPainterStateScriptableObject.instance.gameObject = gameObject;
            EntityPainterStateScriptableObject.instance.gameObjectPopulated = gameObjectPopulated;
            EntityPainterStateScriptableObject.instance.prototypeScrollPosition = prototypesScrollPosition;
            EntityPainterStateScriptableObject.instance.prototypes = prototypes;
            EntityPainterStateScriptableObject.instance.lastPrototypeId = lastPrototypeId;

            EntityPainterStateScriptableObject.instance.generatedResults = generatedResults;

            EntityPainterStateScriptableObject.instance.bakedColorResultArraysData = bakedColorResultArraysData;
            EntityPainterStateScriptableObject.instance.bakedDepthResultArraysData = bakedDepthResultArraysData;
            EntityPainterStateScriptableObject.instance.bakedColorResultArrayTextureFormat = bakedColorResultArrayTextureFormat;
            EntityPainterStateScriptableObject.instance.bakedDepthResultArrayTextureFormat = bakedDepthResultArrayTextureFormat;
            EntityPainterStateScriptableObject.instance.bakedColorResultArrayTextureDimensions = bakedColorResultArrayTextureDimensions;
            EntityPainterStateScriptableObject.instance.bakedDepthResultArrayTextureDimensions = bakedDepthResultArrayTextureDimensions;

            EntityPainterStateScriptableObject.instance.assetsInitialized = assetsInitialized;
            EntityPainterStateScriptableObject.instance.sdClientInitialized = sdClientInitialized;
            EntityPainterStateScriptableObject.instance.stableDiffusionModule = stableDiffusionModule;

            EntityPainterStateScriptableObject.instance.selectedTab = selectedTab;
        }
    }

    [Serializable]
    public class ResultBinaryData
    {
        public ProjectionBinaryData[] projections;
    }

    [Serializable]
    public class ProjectionBinaryData
    {
        public byte[] imageData;
    }

    [Serializable]
    public class EntityPainterData : ScriptableObject
    {
        [NonSerialized] public GameObject gameObject;
    }

#if UNITY_2020_1_OR_NEWER
    [FilePath("UntoldByte/SaveSettings/EntityPainterState.data", FilePathAttribute.Location.PreferencesFolder)]
#endif
    public class EntityPainterStateScriptableObject : ScriptableSingleton<EntityPainterStateScriptableObject>
    {
        public byte[] entityPainterIconData;
        public byte[] adjustIconData;
        public byte[] closeIconData;
        public byte[] generateIconData;
        public byte[] plusIconData;
        public byte[] previewIconData;
        public byte[] refreshIconData;
        public byte[] saveIconData;
        public byte[] tryIconData;

        public bool windowVisible;

        public GameObject gameObject;
        public bool gameObjectPopulated;
        public Vector2 prototypeScrollPosition;
        public List<EntityPrototype> prototypes;
        public int lastPrototypeId;

        public List<StableDiffusionResult> generatedResults;

        public ResultBinaryData[] bakedColorResultArraysData;
        public ResultBinaryData[] bakedDepthResultArraysData;
        public TextureFormat bakedColorResultArrayTextureFormat;
        public TextureFormat bakedDepthResultArrayTextureFormat;
        public Vector2 bakedColorResultArrayTextureDimensions;
        public Vector2 bakedDepthResultArrayTextureDimensions;

        public bool assetsInitialized;
        public bool sdClientInitialized;
        public StableDiffusionModule stableDiffusionModule;

        public int selectedTab;
    }
}