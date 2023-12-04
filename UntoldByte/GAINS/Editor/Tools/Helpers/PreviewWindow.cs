using UnityEditor;
using UnityEngine;

namespace UntoldByte.GAINS.Editor
{
    internal class PreviewWindow : EditorWindow
    {
        private bool destroyTextureOnClose;

        private Texture previewTexture = null;

        internal static void Create(string title, Texture previewTexture, bool destroyTextureOnClose = false)
        {
            var testWindow = GetWindow<PreviewWindow>(true, title);
            testWindow.previewTexture = previewTexture;
            testWindow.destroyTextureOnClose = destroyTextureOnClose;
            testWindow.ShowAuxWindow();
        }

        private void OnGUI()
        {
            if (previewTexture == null) return;

            int width = previewTexture.width + 6 + 6;
            int height = previewTexture.height + 9 + 3;

            minSize = maxSize = new Vector2(width, height);
            GUILayout.Label(new GUIContent(previewTexture));
        }

        private void OnDestroy()
        {
            if (destroyTextureOnClose)
            {
                DestroyImmediate(previewTexture);
            }
        }
    }
}
