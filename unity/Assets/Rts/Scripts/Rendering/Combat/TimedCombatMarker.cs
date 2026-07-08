using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Combat
{
    public sealed class TimedCombatMarker : MonoBehaviour
    {
        public float duration = 0.5f;
        public bool floatUp;
        float elapsed;

        void Update()
        {
            elapsed += Time.deltaTime;
            if (floatUp)
                transform.position += Vector3.up * Time.deltaTime * 0.6f;
            if (elapsed >= duration)
                CombatObjectUtility.DestroyObject(gameObject);
        }
    }
}
