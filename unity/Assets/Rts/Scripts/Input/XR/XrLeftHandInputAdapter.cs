using UnityEngine;

namespace ProjectAegisRTS.UnityClient.InputControls.XR
{
    public sealed class XrLeftHandInputAdapter : MonoBehaviour, ILeftHandInputSource
    {
        public Transform leftRayOrigin;
        public bool adapterEnabled;

        public bool IsAvailable { get { return adapterEnabled && leftRayOrigin != null; } }

        public bool TryGetRay(out Ray ray)
        {
            if (!IsAvailable)
            {
                ray = default(Ray);
                return false;
            }

            ray = new Ray(leftRayOrigin.position, leftRayOrigin.forward);
            return true;
        }

        public bool GetMenuToggleDown() { return false; }
        public bool GetPrimarySelectDown() { return false; }
        public bool GetPrimarySelectHeld() { return false; }
        public bool GetPrimarySelectUp() { return false; }
        public bool GetSecondaryModifierHeld() { return false; }
        public bool GetCancelDown() { return false; }
        public bool GetCategoryNextDown() { return false; }
        public bool GetCategoryPreviousDown() { return false; }
        public float GetCategoryAxis() { return 0f; }
        public float GetItemAxis() { return 0f; }
        public bool GetCycleCandidateNextDown() { return false; }
        public bool GetCycleCandidatePreviousDown() { return false; }
        public bool GetLassoModifierHeld() { return false; }
    }
}
