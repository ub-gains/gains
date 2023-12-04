using UnityEditor;
using UnityEngine;

namespace UntoldByte.GAINS.Editor
{
    internal class ScreenshotPreviewWindow : EditorWindow
    {
        internal bool destroyTexturesOnClose = false;
        internal Texture2D generatedImageTexture = null;
        internal Texture2D generatedDepthTexture = null;

        private void OnGUI()
        {
            minSize = maxSize = new Vector2(512, 1024);
            GUILayout.Label(new GUIContent(generatedImageTexture));
            GUILayout.Label(new GUIContent(generatedDepthTexture));
        }

        private void OnDestroy()
        {
            if (destroyTexturesOnClose)
            {
                DestroyImmediate(generatedImageTexture);
                DestroyImmediate(generatedDepthTexture);
            }
        }
    }
}