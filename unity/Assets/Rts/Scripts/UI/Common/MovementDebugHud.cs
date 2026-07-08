using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.Rendering.Motion;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.UI.Common
{
    public sealed class MovementDebugHud : MonoBehaviour
    {
        public ActorRenderSystem actorRenderSystem;
        public bool visible = true;
        public KeyCode toggleKey = KeyCode.F9;

        void Update()
        {
            if (Input.GetKeyDown(toggleKey))
                visible = !visible;
            if (actorRenderSystem == null)
                actorRenderSystem = Object.FindFirstObjectByType<ActorRenderSystem>();
        }

        void OnGUI()
        {
            if (!visible)
                return;

            if (actorRenderSystem == null)
                actorRenderSystem = Object.FindFirstObjectByType<ActorRenderSystem>();

            GUILayout.BeginArea(new Rect(12f, 260f, 380f, 310f), GUI.skin.box);
            GUILayout.Label("MOVEMENT DEBUG HUD (F9)");
            if (actorRenderSystem == null)
            {
                GUILayout.Label("ActorRenderSystem: missing");
                GUILayout.EndArea();
                return;
            }

            GUILayout.Label("Actor visuals: " + actorRenderSystem.ActorVisualCount);
            GUILayout.Label("Moving visuals: " + actorRenderSystem.MovingVisualCount);
            GUILayout.Label("Vehicle controllers: " + actorRenderSystem.VehicleMotionControllerCount);
            GUILayout.Label("Infantry controllers: " + actorRenderSystem.InfantryMotionControllerCount);
            GUILayout.Label("Aircraft controllers: " + actorRenderSystem.AircraftMotionControllerCount);

            ActorViewBehaviour view;
            if (actorRenderSystem.TryGetDebugActorView(out view) && view != null && view.ActorVisualMotion != null)
            {
                var motion = view.ActorVisualMotion;
                GUILayout.Space(6f);
                GUILayout.Label("Selected/first actor: " + view.ActorId + " " + view.ActorTypeId);
                GUILayout.Label("Profile: " + (motion.ActiveProfile == null ? "none" : motion.ActiveProfile.profileId));
                GUILayout.Label("State: " + motion.CurrentMotionState);
                GUILayout.Label("Visual speed: " + motion.CurrentVisualSpeed.ToString("0.00"));
                GUILayout.Label("Normalized speed: " + motion.NormalizedVisualSpeed.ToString("0.00"));
                GUILayout.Label("Visual facing: " + motion.CurrentVisualFacing.ToString("0.0"));
                GUILayout.Label("Target: " + motion.TargetPosition.ToString("F2"));
                GUILayout.Label("Visual: " + motion.VisualPosition.ToString("F2"));
                GUILayout.Label("Controller: " + view.MotionControllerSummary);
                if (view.VehicleMotion != null)
                    GUILayout.Label("Track/Wheel: " + view.VehicleMotion.TrackPhase.ToString("0.00") + " / " + view.VehicleMotion.WheelPhase.ToString("0.00"));
                if (view.InfantryMotion != null)
                    GUILayout.Label("Infantry: " + view.InfantryMotion.LocomotionState + " step " + view.InfantryMotion.StepPhase.ToString("0.00"));
                if (view.AircraftMotion != null)
                    GUILayout.Label("Aircraft bank: " + view.AircraftMotion.BankAngle.ToString("0.0"));
                if (view.TurretAim != null)
                    GUILayout.Label("Turret yaw/recoil: " + view.TurretAim.CurrentTurretYaw.ToString("0.0") + " / " + view.TurretAim.RecoilAmount.ToString("0.00"));
            }
            else
            {
                GUILayout.Label("No actor motion controller available yet.");
            }

            GUILayout.EndArea();
        }
    }
}
