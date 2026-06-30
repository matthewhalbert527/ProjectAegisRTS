using ProjectAegisRTS.UnityClient.Performance;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Feedback
{
    public sealed class FeedbackVisualMarker : MonoBehaviour
    {
        public float durationSeconds = 0.7f;
        public float floatSpeed = 0.6f;
        float elapsed;
        ObjectPoolService objectPoolService;
        VfxFeedbackController owner;

        void OnEnable()
        {
            elapsed = 0f;
        }

        public void Configure(float duration, float speed, ObjectPoolService poolService, VfxFeedbackController ownerController)
        {
            durationSeconds = duration;
            floatSpeed = speed;
            objectPoolService = poolService;
            owner = ownerController;
            elapsed = 0f;
        }

        void Update()
        {
            AdvanceLifetime(Time.deltaTime);
        }

        public void AdvanceLifetime(float deltaTime)
        {
            elapsed += deltaTime;
            if (floatSpeed > 0f)
                transform.position += Vector3.up * floatSpeed * deltaTime;
            if (elapsed >= durationSeconds)
                ReleaseOrDestroy();
        }

        void ReleaseOrDestroy()
        {
            if (owner != null)
                owner.NotifyMarkerReleased();
            if (objectPoolService != null)
                objectPoolService.Release(gameObject);
            else
                FeedbackObjectUtility.DestroyObject(gameObject);
        }
    }
}
