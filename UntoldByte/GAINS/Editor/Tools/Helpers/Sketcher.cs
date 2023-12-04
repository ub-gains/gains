using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace UntoldByte.GAINS.Editor
{
    internal class Sketcher : EditorWindow, IPrototypeEditor
    {
        private IPrototypeUpdater prototypeUpdater;
        private int sketchId;

        private RenderTexture renderTexture = null;
        private RenderTexture tmpRenderTexture = null;
        private Material material = null;
        private bool isDrawing = false;
        private int brushSize = 6;
        private LinkedList<RenderTexture> undoCollection;
        private readonly int maxNumberOfUndoSteps = 10;

        private bool editingPrevious = false;

        private readonly int position0Id = Shader.PropertyToID("_Position0");
        private readonly int position1Id = Shader.PropertyToID("_Position1");
        private readonly int brushSizeId = Shader.PropertyToID("_brushSize");
        private readonly int brushColorId = Shader.PropertyToID("_brushColor");

        private Vector2 previousMousePosition = -Vector2.one;

        internal static Sketcher ShowWindow()
        {
            Sketcher window = GetWindow<Sketcher>(true, "Sketcher");
            window.Setup();
            window.ShowUtility();
            window.Focus();
            window.Repaint();

            return window;
        }

        public void SetParent(IPrototypeUpdater prototypeUpdater)
        {
            this.prototypeUpdater = prototypeUpdater;
        }

        public void SetPrototype(IPrototype prototype)
        {
            editingPrevious = true;
            sketchId = prototype.Id;
            var previousActiveRenderTexture = RenderTexture.active;
            Graphics.Blit(prototype.Texture, renderTexture);
            RenderTexture.active = previousActiveRenderTexture;
            TextureUtilities.Flush();
            Repaint();
        }

        internal protected void Setup()
        {
            renderTexture = new RenderTexture(512, 512, 0, RenderTextureFormat.Default);
            TextureUtilities.SetFilteringForTexture(renderTexture);
            renderTexture.Create();
            tmpRenderTexture = new RenderTexture(512, 512, 0, RenderTextureFormat.Default);
            TextureUtilities.SetFilteringForTexture(tmpRenderTexture);
            tmpRenderTexture.Create();

            var previousActiveRenderTexture = RenderTexture.active;
            RenderTexture.active = renderTexture;
            GL.Clear(true, true, Color.white);
            RenderTexture.active = previousActiveRenderTexture;

            material = new Material(Shader.Find("Hidden/UntoldByte/GAINS/SketcherShader"));

            undoCollection = new LinkedList<RenderTexture>();

            TextureUtilities.Flush();
        }

        protected void OnGUI()
        {
            minSize = maxSize = new Vector2(512 + 12, 512 + 38);
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Space(4);
            brushSize = EditorGUILayout.IntSlider("Brush size", brushSize, 2, 75);
            if (GUILayout.Button(new GUIContent("Undo"), EditorStyles.miniButton))
                Undo();
            if (GUILayout.Button(new GUIContent("Clear"), EditorStyles.miniButton))
                Clear();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent(editingPrevious ? "Update" : "Add"), EditorStyles.miniButton))
                UpdateSketch();
            GUILayout.Space(3);
            GUILayout.EndHorizontal();
            GUI.DrawTexture(new Rect(6, 29, 512, 512), renderTexture);
            HandleLineDrawing();
        }

        private void Undo()
        {
            if (undoCollection.Count == 0) return;

            RenderTexture undoStep = undoCollection.Last();
            undoCollection.RemoveLast();
            renderTexture.Release();

            RenderTexture newTexture = new RenderTexture(512, 512, 0, RenderTextureFormat.Default);
            TextureUtilities.SetFilteringForTexture(newTexture);
            var previousActiveRenderTexture = RenderTexture.active;
            Graphics.Blit(undoStep, newTexture);
            RenderTexture.active = previousActiveRenderTexture;
            undoStep.Release();

            TextureUtilities.Flush();

            renderTexture = newTexture;
        }

        private void RecordUndoStep()
        {
            RenderTexture undoStep = new RenderTexture(512, 512, 0, RenderTextureFormat.Default);
            TextureUtilities.SetFilteringForTexture(undoStep);

            var previousActiveRenderTexture = RenderTexture.active;
            Graphics.Blit(renderTexture, undoStep);
            RenderTexture.active = previousActiveRenderTexture;

            TextureUtilities.Flush();

            undoCollection.AddLast(undoStep);

            while (undoCollection.Count > maxNumberOfUndoSteps)
            {
                RenderTexture firstUndoStep = undoCollection.First();
                undoCollection.RemoveFirst();
                firstUndoStep.Release();
            }
        }

        private void Clear()
        {
            var previousActiveRenderTexture = RenderTexture.active;
            RenderTexture.active = renderTexture;
            GL.Clear(true, true, Color.white);
            RenderTexture.active = previousActiveRenderTexture;
            TextureUtilities.Flush();
            ClearUndoCollection();
            Repaint();
        }

        private void ClearUndoCollection()
        {
            while (undoCollection.Count > 0)
            {
                RenderTexture lastUndoStep = undoCollection.Last();
                lastUndoStep.Release();
                undoCollection.RemoveLast();
            }
        }

        private void HandleLineDrawing()
        {
            if (!Event.current.isMouse) return;

            if (Event.current.rawType == EventType.MouseDown && Event.current.button == 0)
            {
                RecordUndoStep();
                AddCurrentMousePositionToCurrentLine();
                isDrawing = true;
            }

            if (Event.current.type == EventType.MouseDrag && Event.current.button == 0 && isDrawing)
            {
                AddCurrentMousePositionToCurrentLine();
            }

            if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
            {
                AddCurrentMousePositionToCurrentLine();
                isDrawing = false;
                previousMousePosition = -Vector2.one;
            }
        }

        private void AddCurrentMousePositionToCurrentLine()
        {
            if (renderTexture == null)
                return;

            if (previousMousePosition.x < 0 || previousMousePosition.x > 512 || previousMousePosition.y < 0 || previousMousePosition.y > 512)
                previousMousePosition = -Vector2.one;

            Vector2 mousePosition = Event.current.mousePosition - new Vector2(6, 29);
            mousePosition.y = 512 - mousePosition.y;
            if (previousMousePosition == -Vector2.one)
                previousMousePosition = mousePosition;
            material.SetVector(position0Id, previousMousePosition * new Vector2(1f / 512, 1f / 512));
            material.SetVector(position1Id, mousePosition * new Vector2(1f / 512, 1f / 512));
            material.SetFloat(brushSizeId, brushSize / 512f);
            material.SetColor(brushColorId, Color.black);
            var previousActiveRenderTexture = RenderTexture.active;
            Graphics.Blit(renderTexture, tmpRenderTexture, material);
            Graphics.Blit(tmpRenderTexture, renderTexture);
            RenderTexture.active = previousActiveRenderTexture;

            TextureUtilities.Flush();

            previousMousePosition = mousePosition;
            Repaint();
        }

        private void OnDestroy()
        {
            CleanUp();
        }

        private void UpdateSketch()
        {
            if (prototypeUpdater == null)
            {
                Debug.LogWarning("Closing Drawing window, warning prototypeUpdater not found - drawing will not be saved");
                CleanUp();
                return;
            }
            Prototype sketch = new Prototype()
            {
                Id = sketchId,
                Type = PrototypeType.Sketch,
                Texture = TextureUtilities.ToTexture2D(renderTexture)
            };
            prototypeUpdater.UpdatePrototype(sketch);

            Close();
        }

        private void CleanUp()
        {
            renderTexture.Release();
            renderTexture = null;
            tmpRenderTexture.Release();
            tmpRenderTexture = null;
            DestroyImmediate(material);
            material = null;
            prototypeUpdater = null;
            ClearUndoCollection();
        }
    }
}