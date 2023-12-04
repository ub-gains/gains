using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace UntoldByte.GAINS.Editor
{
    public class SimpleUnwrapper : EditorWindow
    {
        private Mesh UnwrappedMesh;
        private Mesh MeshToUnwrap { get { return MeshToUnwrapData.meshToUnwrap; } set { MeshToUnwrapData.meshToUnwrap = value; } }

        private UnwrapperData meshToUnwrapData;
        internal UnwrapperData MeshToUnwrapData
        {
            set
            {
                meshToUnwrapData = value;
            }
            get
            {
                if (meshToUnwrapData != null) return meshToUnwrapData;

                UnwrapperData tmpEntityPainterData = CreateInstance<UnwrapperData>();
                if (!ReferenceEquals(meshToUnwrapData, (UnwrapperData)null))
                    EditorUtility.CopySerializedManagedFieldsOnly(meshToUnwrapData, tmpEntityPainterData);
                meshToUnwrapData = tmpEntityPainterData;

                return meshToUnwrapData;
            }
        }


        [SerializeField]
        private byte[] uvsPreviewTextureData;
        [System.NonSerialized]
        private Texture2D uvsPreviewTexture;
        internal Texture2D UVSPreviewTexture
        {
            get
            {
                if (uvsPreviewTexture != null) return uvsPreviewTexture;
                if (uvsPreviewTextureData == null || uvsPreviewTextureData.Length == 0) return null;
                uvsPreviewTexture = TextureUtilities.ByteArrayToTexture2D(uvsPreviewTextureData);
                return uvsPreviewTexture;
            }
            set
            {
                if (value == null)
                {
                    uvsPreviewTextureData = null;
                    DestroyImmediate(uvsPreviewTexture);
                }
                else
                {
                    uvsPreviewTexture = value;
                    uvsPreviewTextureData = value.EncodeToPNG();
                }
            }
        }

        private bool quitting = false;

        [MenuItem("Window/UntoldByte/Tool Helpers/Simple Unwrapper")]
        private static void ShowWindow()
        {
            SimpleUnwrapper window = GetWindow<SimpleUnwrapper>();
            window.Focus();
            window.Repaint();
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += PlayModeStateChange;
            EditorApplication.quitting += Quitting;
            AssemblyReloadEvents.afterAssemblyReload += AfterAssemblyReload;

            LoadScriptableState();
        }

        private void OnDisable()
        {
            if (!quitting)
                SaveScriptableState();

            EditorApplication.playModeStateChanged -= PlayModeStateChange;
            EditorApplication.quitting -= Quitting;
            AssemblyReloadEvents.afterAssemblyReload -= AfterAssemblyReload;
        }

        private void PlayModeStateChange(PlayModeStateChange playModeState)
        {
            SetWindowTitle();
        }

        private void AfterAssemblyReload()
        {
            SetWindowTitle();
        }

        private void Quitting()
        {
            SaveScriptableState();

            quitting = true;
        }

        private void OnGUI()
        {
            if (titleContent.image == null) SetWindowTitle();

            EditorGUI.BeginChangeCheck();
            Mesh tmpMeshToUnwrap = MeshToUnwrap;

            GUILayout.Space(5);
            tmpMeshToUnwrap = (Mesh)EditorGUILayout.ObjectField("Mesh to unwrap", tmpMeshToUnwrap, typeof(Mesh), true);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(MeshToUnwrapData, "Mesh to unwrap");

                //ChangeEntityMaterialManagerSelected(false);
                MeshToUnwrap = tmpMeshToUnwrap;
                UnwrappedMesh = null;
                //gameObjectPopulated = gameObject != null;
                //EnsureEntityMaterialManagerExists();
                //ChangeEntityMaterialManagerSelected(true);
                //adjustmentInitialized = false;

                EditorUtility.SetDirty(MeshToUnwrapData);
            }

            if (MeshToUnwrap != null && MeshToUnwrap.uv == null)
                GUILayout.Label("UVs on mesh not present");

            if (GUILayout.Button(new GUIContent("Preview mesh UVs")))
            {
                Material previewMaterial = new Material(Shader.Find("Hidden/UntoldByte/GAINS/UVMeshShader")); //Unlit/Color
                Mesh tmpMesh = UnwrappedMesh != null ? UnwrappedMesh : MeshToUnwrap;
                Mesh drawingMesh = new Mesh();
                drawingMesh.vertices = tmpMesh.vertices;
                drawingMesh.uv = tmpMesh.uv;

                //triangles to lines
                int[] lines = new int[tmpMesh.triangles.Length * 3];
                for (int t = 0; t < tmpMesh.triangles.Length; t += 3)
                {
                    lines[t * 2] = tmpMesh.triangles[t];
                    lines[t * 2 + 1] = tmpMesh.triangles[t + 1];

                    lines[(t + 1) * 2] = tmpMesh.triangles[t + 1];
                    lines[(t + 1) * 2 + 1] = tmpMesh.triangles[t + 2];

                    lines[(t + 2) * 2] = tmpMesh.triangles[t + 2];
                    lines[(t + 2) * 2 + 1] = tmpMesh.triangles[t];
                }

                drawingMesh.SetIndices(lines, MeshTopology.Lines, 0);
                UVSPreviewTexture = TextureUtilities.BakeTexture(drawingMesh, previewMaterial, 2048, 2048, 1, 1);
                DestroyImmediate(drawingMesh);
            }

            if (GUILayout.Button(new GUIContent("Unwrap mesh")))
            {
                Vector2[] uvs = Unwrapping.GeneratePerTriangleUV(MeshToUnwrap);

                uvs = AdjustUVsToTriangles(uvs, MeshToUnwrap);

                //clean if exists
                if (UnwrappedMesh != null)
                    DestroyImmediate(UnwrappedMesh);

                //clone mesh
                UnwrappedMesh = new Mesh();
                UnwrappedMesh.vertices = MeshToUnwrap.vertices;
                UnwrappedMesh.triangles = MeshToUnwrap.triangles;
                UnwrappedMesh.normals = MeshToUnwrap.normals;
                UnwrappedMesh.tangents = MeshToUnwrap.tangents;
                UnwrappedMesh.colors = MeshToUnwrap.colors;

                UnwrappedMesh.uv = uvs;
            }

            EditorGUI.BeginDisabledGroup(UnwrappedMesh == null);
            if (GUILayout.Button(new GUIContent("Export unwrapped")))
            {
                string path = "Assets/UntoldByte/GAINSExports/";
                string meshesPath = path + "Meshes/";

                if (!Directory.Exists(meshesPath))
                    Directory.CreateDirectory(meshesPath);

                string timeString = DateTime.Now.ToFileTime().ToString();

                string meshFileName = MeshToUnwrap.name + " " + timeString + ".asset";
                AssetDatabase.CreateAsset(UnwrappedMesh, meshesPath + meshFileName);
                AssetDatabase.SaveAssets();

                AssetDatabase.Refresh();
            }
            EditorGUI.EndDisabledGroup();

            if (UVSPreviewTexture != null)
            {
                Rect uvsPreviewTextureRect = GUILayoutUtility.GetLastRect();
                float largerDimension = EditorGUIUtility.currentViewWidth;
                uvsPreviewTextureRect.width = uvsPreviewTextureRect.height = largerDimension;

                GUILayout.Label(new GUIContent(UVSPreviewTexture),
                    new GUIStyle()
                    {
                        fixedHeight = largerDimension,
                        fixedWidth = largerDimension,
                    });

                Rect lastRect = GUILayoutUtility.GetLastRect();
                if (lastRect.Contains(Event.current.mousePosition))
                {
                    Rect firstButtonRect = new Rect(lastRect.x + lastRect.width - 28, lastRect.y + 4, 24, 24);
                    if (GUI.Button(firstButtonRect, new GUIContent("", "Clear")))
                    {
                        UVSPreviewTexture = null;
                    }
                    //GUI.DrawTexture(firstButtonRect, CloseIcon);
                }
            }
        }

        private Vector2[] AdjustUVsToTriangles(Vector2[] uvs, Mesh mesh)
        {
            Vector2[] simplifiedUVs = new Vector2[mesh.vertexCount];

            for (int t = 0; t < mesh.triangles.Length; t++)
            {
                if (simplifiedUVs[mesh.triangles[t]] == Vector2.zero)
                    simplifiedUVs[mesh.triangles[t]] = uvs[t];
            }

            return simplifiedUVs;
        }

        private void SetWindowTitle()
        {
            if (titleContent == null)
            {
                titleContent = new GUIContent("Simple Unwrapper"); //, EntityPainterIcon);
                return;
            }

            if (titleContent.text == null || titleContent.text.IndexOf('.') > 0)
                titleContent.text = "Simple Unwrapper";

            //if (titleContent.image == null)
            //    titleContent.image = EntityPainterIcon;
        }

        private void LoadScriptableState()
        {
            MeshToUnwrap = SimpleUnwrapperStateScriptableObject.instance.meshToUnwrap;
        }

        private void SaveScriptableState()
        {
            SimpleUnwrapperStateScriptableObject.instance.meshToUnwrap = MeshToUnwrap;
        }
    }


    [Serializable]
    public class UnwrapperData : ScriptableObject
    {
        [NonSerialized] public Mesh meshToUnwrap;
    }

#if UNITY_2020_1_OR_NEWER
    [FilePath("UntoldByte/SaveSettings/SimpleUnwrapperState.data", FilePathAttribute.Location.PreferencesFolder)]
#endif
    public class SimpleUnwrapperStateScriptableObject : ScriptableSingleton<SimpleUnwrapperStateScriptableObject>
    {
        public Mesh meshToUnwrap;
    }
}
