using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Feedback
{
    public static class FeedbackObjectUtility
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
