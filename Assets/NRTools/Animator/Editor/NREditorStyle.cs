using UnityEngine;

namespace NRTools.CustomAnimator
{
    public class NREditorStyle
    {
        private static GUIStyle eventStyle;

        public static void InitializeStyles()
        {
            if (eventStyle == null)
            {
                eventStyle = new GUIStyle(GUI.skin.box);
                eventStyle.normal.background = MakeTex(1, 1, new Color(1f, 0.5f, 0f));
            }
        }

        private static Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

    }
}