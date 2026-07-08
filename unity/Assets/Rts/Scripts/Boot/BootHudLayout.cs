using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Boot
{
    public static class BootHudLayout
    {
        public const float ReferenceWidth = 1280f;
        public const float ReferenceHeight = 720f;
        public const float MaxScale = 1.65f;

        public static float CurrentScale()
        {
            return ScaleForScreen(Screen.width, Screen.height);
        }

        public static float ScaleForScreen(int width, int height)
        {
            if (width <= 0 || height <= 0)
                return 1f;

            var scale = Mathf.Min(width / ReferenceWidth, height / ReferenceHeight);
            return Mathf.Clamp(scale, 1f, MaxScale);
        }

        public static Matrix4x4 BeginArea(Rect area)
        {
            var previousMatrix = GUI.matrix;
            var scale = CurrentScale();
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));
            GUILayout.BeginArea(ClampAreaToVisibleScreen(area, scale), GUI.skin.box);
            return previousMatrix;
        }

        public static void EndArea(Matrix4x4 previousMatrix)
        {
            GUILayout.EndArea();
            GUI.matrix = previousMatrix;
        }

        static Rect ClampAreaToVisibleScreen(Rect area, float scale)
        {
            var logicalWidth = Mathf.Max(1f, Screen.width / Mathf.Max(0.001f, scale));
            var logicalHeight = Mathf.Max(1f, Screen.height / Mathf.Max(0.001f, scale));
            var margin = 16f;
            area.x = Mathf.Clamp(area.x, margin, Mathf.Max(margin, logicalWidth - area.width - margin));
            area.y = Mathf.Clamp(area.y, margin, Mathf.Max(margin, logicalHeight - area.height - margin));
            return area;
        }
    }
}
