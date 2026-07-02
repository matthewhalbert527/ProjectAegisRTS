using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Motion
{
    public sealed class AircraftVisualMotionController : MonoBehaviour
    {
        public ActorVisualMotionController motionController;
        public Transform aircraftRoot;
        float baseRootY;
        Transform initializedRoot;
        bool hasLogicAltitude;
        float logicAltitudeOffset;
        bool logicDocked;

        public float BankAngle { get; private set; }
        public float AltitudeOffset { get; private set; }
        public float HoverPhase { get; private set; }
        public bool IsHovering { get; private set; }
        public bool UsesLogicAltitude { get { return hasLogicAltitude; } }
        public bool IsDocked { get { return logicDocked; } }

        public void Initialize(ActorVisualMotionController baseController)
        {
            motionController = baseController;
            if (aircraftRoot == null)
                aircraftRoot = transform;
            if (initializedRoot != aircraftRoot)
            {
                initializedRoot = aircraftRoot;
                baseRootY = aircraftRoot.localPosition.y;
            }
        }

        public void ApplyLogicAltitude(float altitudeOffset, bool isDocked)
        {
            hasLogicAltitude = true;
            logicAltitudeOffset = Mathf.Max(0f, altitudeOffset);
            logicDocked = isDocked;
            ApplyAltitudeToRoot(logicAltitudeOffset, 0f);
        }

        public void TickVisual(float deltaTime)
        {
            if (motionController == null)
                return;

            var profile = motionController.ActiveProfile;
            var bankMax = profile == null ? 18f : profile.aircraftBankAmount;
            var altitude = hasLogicAltitude ? logicAltitudeOffset : profile == null ? 1.4f : profile.aircraftAltitudeOffset;
            var hoverBob = hasLogicAltitude && logicDocked ? 0f : profile == null ? 0.08f : profile.aircraftHoverBobAmount;
            BankAngle = Mathf.Clamp(-motionController.VisualTurnRate * 0.08f, -bankMax, bankMax);
            HoverPhase = Mathf.Repeat(HoverPhase + deltaTime, 1f);
            IsHovering = motionController.CurrentMotionState == VisualMotionState.Hovering || motionController.NormalizedVisualSpeed < 0.05f;
            AltitudeOffset = altitude + Mathf.Sin(HoverPhase * Mathf.PI * 2f) * hoverBob;

            ApplyAltitudeToRoot(AltitudeOffset, BankAngle);
        }

        void ApplyAltitudeToRoot(float altitudeOffset, float bankAngle)
        {
            if (aircraftRoot != null && aircraftRoot != transform)
            {
                var local = aircraftRoot.localPosition;
                aircraftRoot.localPosition = new Vector3(local.x, baseRootY + altitudeOffset, local.z);
                aircraftRoot.localRotation = Quaternion.Euler(0f, 0f, bankAngle);
            }
        }
    }
}
