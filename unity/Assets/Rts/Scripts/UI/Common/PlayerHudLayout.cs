using UnityEngine;

namespace ProjectAegisRTS.UnityClient.UI.Common
{
    public static class PlayerHudLayout
    {
        public static readonly Rect ObjectiveArea = new Rect(12f, 12f, 420f, 258f);
        public static readonly Rect PromptArea = new Rect(12f, 282f, 420f, 106f);
        public static readonly Rect MatchArea = new Rect(12f, 400f, 420f, 132f);
        public static readonly Rect ChecklistArea = new Rect(452f, 12f, 430f, 326f);

        public static Matrix4x4 BeginArea(Rect area)
        {
            var previousMatrix = GUI.matrix;
            var scale = HudScale();
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));
            GUI.color = Color.white;
            GUI.contentColor = Color.white;
            GUI.backgroundColor = new Color(0.08f, 0.09f, 0.10f, 0.96f);
            GUILayout.BeginArea(area, GUI.skin.box);
            return previousMatrix;
        }

        public static void EndArea(Matrix4x4 previousMatrix)
        {
            GUILayout.EndArea();
            GUI.matrix = previousMatrix;
            GUI.color = Color.white;
            GUI.contentColor = Color.white;
            GUI.backgroundColor = Color.white;
        }

        static float HudScale()
        {
            var widthScale = Screen.width / 1920f;
            var heightScale = Screen.height / 1080f;
            return Mathf.Clamp(Mathf.Min(widthScale, heightScale), 1f, 1.6f);
        }
    }
}
