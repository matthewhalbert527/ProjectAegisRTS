using UnityEngine;

namespace ProjectAegisRTS.UnityClient.InputControls.XR
{
    public sealed class XrRightHandInputAdapter : MonoBehaviour, IRightHandInputSource
    {
        public Transform rightRayOrigin;
        public bool adapterEnabled;

        public bool IsAvailable { get { return adapterEnabled && rightRayOrigin != null; } }

        public bool TryGetRay(out Ray ray)
        {
            if (!IsAvailable)
            {
                ray = default(Ray);
                return false;
            }

            ray = new Ray(rightRayOrigin.position, rightRayOrigin.forward);
            return true;
        }

        public bool GetHudToggleDown() { return false; }
        public bool GetPrimaryCommandDown() { return false; }
        public bool GetPrimaryCommandHeld() { return false; }
        public bool GetPrimaryCommandUp() { return false; }
        public bool GetCancelDown() { return false; }
        public bool GetMoveModeDown() { return false; }
        public bool GetAttackModeDown() { return false; }
        public bool GetForceAttackModeDown() { return false; }
        public bool GetBoardManipulationHeld() { return false; }
        public bool GetBoardManipulationToggleDown() { return false; }
        public float GetRotateAxis() { return 0f; }
        public float GetZoomAxis() { return 0f; }
        public bool GetAlternateModifierHeld() { return false; }
    }
}
