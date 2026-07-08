using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Motion
{
    public sealed class VehicleVisualMotionController : MonoBehaviour
    {
        public ActorVisualMotionController motionController;
        public Transform leftTrackPlaceholder;
        public Transform rightTrackPlaceholder;
        public Transform suspensionRoot;

        float baseSuspensionY;

        public float TrackPhase { get; private set; }
        public float WheelPhase { get; private set; }
        public float SuspensionOffset { get; private set; }
        public float VisualTurnRate { get; private set; }
        public bool IsBraking { get; private set; }
        public bool IsTurning { get; private set; }

        public void Initialize(ActorVisualMotionController baseController)
        {
            motionController = baseController;
            if (suspensionRoot == null)
                suspensionRoot = transform;
            baseSuspensionY = suspensionRoot.localPosition.y;
        }

        public void TickVisual(float deltaTime)
        {
            if (motionController == null)
                return;

            var profile = motionController.ActiveProfile;
            var trackScale = profile == null ? 3f : profile.trackOrWheelAnimationScale;
            var suspension = profile == null ? 0.03f : profile.suspensionStrength;
            TrackPhase = Mathf.Repeat(TrackPhase + motionController.CurrentVisualSpeed * trackScale * deltaTime, 1f);
            WheelPhase = Mathf.Repeat(WheelPhase + motionController.CurrentVisualSpeed * trackScale * 2f * deltaTime, 1f);
            SuspensionOffset = Mathf.Sin(TrackPhase * Mathf.PI * 2f) * suspension * motionController.NormalizedVisualSpeed;
            VisualTurnRate = motionController.VisualTurnRate;
            IsBraking = motionController.CurrentMotionState == VisualMotionState.Braking || motionController.CurrentMotionState == VisualMotionState.Arriving;
            IsTurning = Mathf.Abs(VisualTurnRate) > 8f;

            if (leftTrackPlaceholder != null)
                leftTrackPlaceholder.localRotation = Quaternion.Euler(0f, TrackPhase * 360f, 0f);
            if (rightTrackPlaceholder != null)
                rightTrackPlaceholder.localRotation = Quaternion.Euler(0f, -TrackPhase * 360f, 0f);
            if (suspensionRoot != null && suspensionRoot != transform)
            {
                var local = suspensionRoot.localPosition;
                suspensionRoot.localPosition = new Vector3(local.x, baseSuspensionY + SuspensionOffset, local.z);
            }
        }
    }
}
