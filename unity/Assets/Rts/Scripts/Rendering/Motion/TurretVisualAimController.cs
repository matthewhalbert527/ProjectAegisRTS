using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Motion
{
    public sealed class TurretVisualAimController : MonoBehaviour
    {
        public ActorVisualMotionController motionController;
        public Transform turretRoot;

        public float CurrentTurretYaw { get; private set; }
        public float TargetTurretYaw { get; private set; }
        public float RecoilAmount { get; private set; }
        public bool IsAiming { get; private set; }

        public void Initialize(ActorVisualMotionController baseController, Transform turret)
        {
            motionController = baseController;
            turretRoot = turret;
            CurrentTurretYaw = turretRoot == null ? 0f : turretRoot.localEulerAngles.y;
            TargetTurretYaw = CurrentTurretYaw;
        }

        public void TickVisual(float deltaTime)
        {
            if (motionController == null)
                return;

            TargetTurretYaw = motionController.CurrentVisualFacing;
            var profile = motionController.ActiveProfile;
            var lag = profile == null ? 6f : profile.turretLag;
            CurrentTurretYaw = Mathf.LerpAngle(CurrentTurretYaw, TargetTurretYaw, Mathf.Clamp01(lag * deltaTime));
            RecoilAmount = Mathf.MoveTowards(RecoilAmount, 0f, deltaTime * 4f);
            IsAiming = Mathf.Abs(Mathf.DeltaAngle(CurrentTurretYaw, TargetTurretYaw)) > 2f;

            if (turretRoot != null)
                turretRoot.localRotation = Quaternion.Euler(0f, CurrentTurretYaw - motionController.CurrentVisualFacing, 0f);
        }

        public void TriggerRecoil()
        {
            var profile = motionController == null ? null : motionController.ActiveProfile;
            RecoilAmount = profile == null ? 0.06f : profile.recoilVisualStrength;
        }
    }
}
