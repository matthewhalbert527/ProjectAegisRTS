using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Motion
{
    public sealed class InfantryVisualMotionController : MonoBehaviour
    {
        public ActorVisualMotionController motionController;
        public Transform bodyRoot;
        float baseBodyY;

        public float StepPhase { get; private set; }
        public string LocomotionState { get; private set; }
        public float AimBlendPlaceholder { get; private set; }
        public float FireBlendPlaceholder { get; private set; }

        public void Initialize(ActorVisualMotionController baseController)
        {
            motionController = baseController;
            if (bodyRoot == null)
                bodyRoot = transform;
            baseBodyY = bodyRoot.localPosition.y;
            LocomotionState = "idle";
        }

        public void TickVisual(float deltaTime)
        {
            if (motionController == null)
                return;

            var profile = motionController.ActiveProfile;
            var stepRate = profile == null ? 5f : profile.infantryStepRate;
            var stride = profile == null ? 0.35f : profile.infantryStrideLength;
            StepPhase = Mathf.Repeat(StepPhase + motionController.NormalizedVisualSpeed * stepRate * deltaTime, 1f);
            if (motionController.NormalizedVisualSpeed > 0.7f)
                LocomotionState = "run";
            else if (motionController.NormalizedVisualSpeed > 0.05f)
                LocomotionState = "walk";
            else
                LocomotionState = "idle";

            AimBlendPlaceholder = Mathf.MoveTowards(AimBlendPlaceholder, motionController.IsMoving ? 0f : 0.25f, deltaTime * 2f);
            FireBlendPlaceholder = Mathf.MoveTowards(FireBlendPlaceholder, 0f, deltaTime * 3f);

            if (bodyRoot != null && bodyRoot != transform)
            {
                var local = bodyRoot.localPosition;
                var bob = Mathf.Sin(StepPhase * Mathf.PI * 2f) * stride * 0.035f * motionController.NormalizedVisualSpeed;
                bodyRoot.localPosition = new Vector3(local.x, baseBodyY + bob, local.z);
            }
        }
    }
}
