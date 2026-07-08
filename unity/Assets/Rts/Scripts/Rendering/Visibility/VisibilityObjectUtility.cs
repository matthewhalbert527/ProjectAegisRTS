using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Visibility
{
    internal static class VisibilityObjectUtility
    {
        public static void DestroyObject(Object target)
        {
            if (target == null)
                return;
            if (Application.isPlaying)
                Object.Destroy(target);
            else
                Object.DestroyImmediate(target);
        }
    }
}
