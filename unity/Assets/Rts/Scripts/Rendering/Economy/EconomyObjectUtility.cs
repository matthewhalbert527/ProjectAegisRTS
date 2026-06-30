using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Economy
{
    internal static class EconomyObjectUtility
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
