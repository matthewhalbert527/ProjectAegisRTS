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
        public Renderer[] trackRenderers;
        public Transform muzzleFlashRoot;
        public Transform[] muzzleFlashRoots;
        public Light muzzleFlashLight;
        public Light[] muzzleFlashLights;

        [Header("Visual Tuning")]
        public float turretTurnDegreesPerSecond = 180f;
        public bool driveTurretFromDesiredAim;
        public float barrelRecoilDistance = 0.08f;
        public float recoilReturnSpeed = 8f;
        public float wheelRotationDegreesPerMeter = 650f;
        public float trackScrollUnitsPerMeter = 1.6f;
        public float suspensionBobMeters = 0.025f;
        public float suspensionFrequency = 5f;
        public float muzzleFlashDuration = 0.08f;
        public float muzzleFlashSpinDegreesPerSecond = 720f;
        public bool estimateMotionFromTransform = true;

        [Header("Debug State")]
        public float currentVisualSpeed;
        public float trackPhaseLeft;
        public float trackPhaseRight;
        public float recoil01;
        public float muzzleFlashTimer;
        public Vector3 desiredAimWorldDirection = Vector3.forward;

        private Vector3 _lastPosition;
        private bool _hasLastPosition;
        private Vector3 _barrelInitialLocalPosition;
        private bool _hasBarrelInitialPosition;
        private MaterialPropertyBlock _trackPropertyBlock;

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
            muzzleFlashTimer = Mathf.Max(muzzleFlashTimer, muzzleFlashDuration);
            UpdateMuzzleFlash(0f);
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
            ScrollTrackMaterials();

            if (bodyRoot != null)
            {
                float bob = Mathf.Sin(Time.time * suspensionFrequency) * suspensionBobMeters * Mathf.Clamp01(currentVisualSpeed);
                Vector3 p = bodyRoot.localPosition;
                p.y = bob;
                bodyRoot.localPosition = p;
            }

            UpdateTurret(deltaTime);
            UpdateRecoil(deltaTime);
            UpdateMuzzleFlash(deltaTime);
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
            if (!driveTurretFromDesiredAim)
                return;

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

        private void ScrollTrackMaterials()
        {
            if (trackRenderers == null)
                return;

            for (int i = 0; i < trackRenderers.Length; i++)
            {
                Renderer r = trackRenderers[i];
                if (r == null)
                    continue;

                Material m = r.sharedMaterial;
                if (m == null)
                    continue;

                var offset = new Vector2(0f, i == 0 ? -trackPhaseLeft : trackPhaseRight);
                _trackPropertyBlock ??= new MaterialPropertyBlock();
                r.GetPropertyBlock(_trackPropertyBlock);
                if (m.HasProperty("_BaseMap"))
                    _trackPropertyBlock.SetVector("_BaseMap_ST", new Vector4(1f, 1f, offset.x, offset.y));
                if (m.HasProperty("_MainTex"))
                    _trackPropertyBlock.SetVector("_MainTex_ST", new Vector4(1f, 1f, offset.x, offset.y));
                r.SetPropertyBlock(_trackPropertyBlock);
            }
        }

        private void UpdateMuzzleFlash(float deltaTime)
        {
            if (muzzleFlashTimer > 0f)
                muzzleFlashTimer = Mathf.Max(0f, muzzleFlashTimer - Mathf.Max(0f, deltaTime));

            bool active = muzzleFlashTimer > 0f;
            UpdateMuzzleFlashRoot(muzzleFlashRoot, active, deltaTime);
            if (muzzleFlashRoots != null)
            {
                for (int i = 0; i < muzzleFlashRoots.Length; i++)
                    UpdateMuzzleFlashRoot(muzzleFlashRoots[i], active, deltaTime);
            }

            UpdateMuzzleFlashLight(muzzleFlashLight, active);
            if (muzzleFlashLights != null)
            {
                for (int i = 0; i < muzzleFlashLights.Length; i++)
                    UpdateMuzzleFlashLight(muzzleFlashLights[i], active);
            }
        }

        private void UpdateMuzzleFlashRoot(Transform flashRoot, bool active, float deltaTime)
        {
            if (flashRoot == null)
                return;

            if (flashRoot.gameObject.activeSelf != active)
                flashRoot.gameObject.SetActive(active);
            if (!active)
                return;

            float t = muzzleFlashDuration <= 0.0001f ? 1f : muzzleFlashTimer / muzzleFlashDuration;
            flashRoot.localScale = Vector3.one * Mathf.Lerp(0.35f, 1.0f, t);
            flashRoot.Rotate(Vector3.forward, muzzleFlashSpinDegreesPerSecond * Mathf.Max(0f, deltaTime), Space.Self);
        }

        private void UpdateMuzzleFlashLight(Light flashLight, bool active)
        {
            if (flashLight == null)
                return;

            flashLight.enabled = active;
            flashLight.intensity = active ? Mathf.Lerp(0.5f, 3.0f, muzzleFlashTimer / Mathf.Max(0.0001f, muzzleFlashDuration)) : 0f;
        }
    }
}
