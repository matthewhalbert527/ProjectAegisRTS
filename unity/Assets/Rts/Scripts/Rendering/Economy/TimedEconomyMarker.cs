using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Economy
{
    public sealed class TimedEconomyMarker : MonoBehaviour
    {
        public float duration = 0.5f;
        float elapsed;

        void Update()
        {
            elapsed += Time.deltaTime;
            transform.position += Vector3.up * Time.deltaTime * 0.35f;
            if (elapsed >= duration)
                EconomyObjectUtility.DestroyObject(gameObject);
        }
    }
}
