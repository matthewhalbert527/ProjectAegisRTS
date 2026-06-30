using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Ai
{
    internal static class AiObjectUtility
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
