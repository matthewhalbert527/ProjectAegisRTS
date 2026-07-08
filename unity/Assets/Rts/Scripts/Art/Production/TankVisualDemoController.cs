using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Art.Production
{
    /// <summary>
    /// Review-scene helper that drives visual-only tank motion, turret aim, and muzzle flash.
    /// It is not used by deterministic gameplay.
    /// </summary>
    public sealed class TankVisualDemoController : MonoBehaviour
    {
        public TankVisualRigController rig;
        public Vector3 orbitCenter = Vector3.zero;
        public float orbitRadius = 1.2f;
        public float orbitSpeed = 0.35f;
        public float fireInterval = 1.6f;
        public float aimSweepDegrees = 55f;

        float fireTimer;
        Vector3 lastPosition;
        bool hasLastPosition;

        void OnEnable()
        {
            if (rig == null)
                rig = GetComponent<TankVisualRigController>();
            if (rig != null)
                rig.driveTurretFromDesiredAim = true;
            lastPosition = transform.position;
            hasLastPosition = true;
            fireTimer = fireInterval * 0.4f;
        }

        void Update()
        {
            if (rig == null)
                return;

            float t = Time.time * orbitSpeed;
            Vector3 local = new Vector3(Mathf.Sin(t) * orbitRadius, 0f, Mathf.Cos(t * 0.7f) * orbitRadius * 0.35f);
            Vector3 target = orbitCenter + local;
            Vector3 velocity = hasLastPosition ? (target - lastPosition) / Mathf.Max(Time.deltaTime, 0.0001f) : Vector3.zero;
            transform.position = target;
            lastPosition = target;
            hasLastPosition = true;

            float yaw = Mathf.Sin(Time.time * 0.75f) * aimSweepDegrees;
            Vector3 aim = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
            rig.SetAimDirection(aim);
            rig.ApplyVisualMotion(velocity, Time.deltaTime);

            fireTimer -= Time.deltaTime;
            if (fireTimer <= 0f)
            {
                rig.TriggerRecoil(1f);
                fireTimer = fireInterval;
            }
        }
    }
}
