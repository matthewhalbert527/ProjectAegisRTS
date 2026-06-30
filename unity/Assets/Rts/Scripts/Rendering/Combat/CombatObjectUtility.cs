using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Combat
{
    internal static class CombatObjectUtility
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
