using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Feedback
{
    public sealed class FeedbackVisualMarker : MonoBehaviour
    {
        public float durationSeconds = 0.7f;
        public float floatSpeed = 0.6f;
        float elapsed;

        void Update()
        {
            elapsed += Time.deltaTime;
            if (floatSpeed > 0f)
                transform.position += Vector3.up * floatSpeed * Time.deltaTime;
            if (elapsed >= durationSeconds)
                FeedbackObjectUtility.DestroyObject(gameObject);
        }
    }
}
