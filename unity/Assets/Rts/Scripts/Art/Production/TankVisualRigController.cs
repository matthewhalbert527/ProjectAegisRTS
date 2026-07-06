using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Art.Production
{
    /// <summary>
    /// Visual-only tank rig controller for turret yaw, barrel recoil, track phase, and suspension.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TankVisualRigController : MonoBehaviour
    {
        [Header("Rig")]
        public Transform bodyRoot;
        public Transform turretRoot;
        public Transform barrelRoot;
        public Transform trackLeftRoot;
        public Transform trackRightRoot;
        public Transform[] wheelLeft;
        public Transform[] wheelRight;

        [Header("Visual Tuning")]
        public float turretTurnDegreesPerSecond = 180f;
        public float barrelRecoilDistance = 0.08f;
        public float recoilReturnSpeed = 8f;
        public float wheelRotationDegreesPerMeter = 650f;
        public float trackScrollUnitsPerMeter = 1.6f;
        public float suspensionBobMeters = 0.025f;
        public float suspensionFrequency = 5f;
        public bool estimateMotionFromTransform = true;

        [Header("Debug State")]
        public float currentVisualSpeed;
        public float trackPhaseLeft;
        public float trackPhaseRight;
        public float recoil01;
        public Vector3 desiredAimWorldDirection = Vector3.forward;

        private Vector3 _lastPosition;
        private bool _hasLastPosition;
        private Vector3 _barrelInitialLocalPosition;
        private bool _hasBarrelInitialPosition;

        private void OnEnable()
        {
            _lastPosition = transform.position;
            _hasLastPosition = true;
            CacheInitials();
        }

        public void SetAimDirection(Vector3 worldDirection)
        {
            if (worldDirection.sqrMagnitude > 0.0001f)
                desiredAimWorldDirection = worldDirection.normalized;
        }

        public void TriggerRecoil(float amount = 1f)
        {
            recoil01 = Mathf.Clamp01(Mathf.Max(recoil01, amount));
        }

        public void ApplyVisualMotion(Vector3 worldVelocity, float deltaTime)
        {
            CacheInitials();
            currentVisualSpeed = worldVelocity.magnitude;
            float distance = currentVisualSpeed * Mathf.Max(deltaTime, 0f);
            trackPhaseLeft += distance * trackScrollUnitsPerMeter;
            trackPhaseRight += distance * trackScrollUnitsPerMeter;

            float wheelDegrees = distance * wheelRotationDegreesPerMeter;
            RotateWheels(wheelLeft, wheelDegrees);
            RotateWheels(wheelRight, wheelDegrees);

            if (bodyRoot != null)
            {
                float bob = Mathf.Sin(Time.time * suspensionFrequency) * suspensionBobMeters * Mathf.Clamp01(currentVisualSpeed);
                Vector3 p = bodyRoot.localPosition;
                p.y = bob;
                bodyRoot.localPosition = p;
            }

            UpdateTurret(deltaTime);
            UpdateRecoil(deltaTime);
        }

        private void CacheInitials()
        {
            if (barrelRoot != null && !_hasBarrelInitialPosition)
            {
                _barrelInitialLocalPosition = barrelRoot.localPosition;
                _hasBarrelInitialPosition = true;
            }
        }

        private void LateUpdate()
        {
            if (!estimateMotionFromTransform)
                return;

            if (!_hasLastPosition)
            {
                _lastPosition = transform.position;
                _hasLastPosition = true;
                return;
            }

            float dt = Mathf.Max(Time.deltaTime, 0.0001f);
            Vector3 velocity = (transform.position - _lastPosition) / dt;
            _lastPosition = transform.position;
            ApplyVisualMotion(velocity, dt);
        }

        private void RotateWheels(Transform[] wheels, float wheelDegrees)
        {
            if (wheels == null)
                return;

            for (int i = 0; i < wheels.Length; i++)
            {
                if (wheels[i] == null)
                    continue;
                wheels[i].Rotate(Vector3.right, wheelDegrees, Space.Self);
            }
        }

        private void UpdateTurret(float deltaTime)
        {
            if (turretRoot == null || desiredAimWorldDirection.sqrMagnitude < 0.0001f)
                return;

            Vector3 flat = Vector3.ProjectOnPlane(desiredAimWorldDirection, Vector3.up);
            if (flat.sqrMagnitude < 0.0001f)
                return;

            Quaternion target = Quaternion.LookRotation(flat.normalized, Vector3.up);
            turretRoot.rotation = Quaternion.RotateTowards(turretRoot.rotation, target, turretTurnDegreesPerSecond * deltaTime);
        }

        private void UpdateRecoil(float deltaTime)
        {
            if (barrelRoot == null || !_hasBarrelInitialPosition)
                return;

            recoil01 = Mathf.MoveTowards(recoil01, 0f, recoilReturnSpeed * deltaTime);
            Vector3 p = _barrelInitialLocalPosition;
            p.z -= barrelRecoilDistance * recoil01;
            barrelRoot.localPosition = p;
        }
    }
}
