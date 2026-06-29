using ProjectAegisRTS.UnityClient.Board;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.InputControls.XR
{
    public sealed class XrBoardPlacementInputAdapter : MonoBehaviour
    {
        public BoardPlacementController controller;
        public bool xrPackagesAvailable;

        public bool TryGetPlacementRay(out Ray ray)
        {
            ray = new Ray(transform.position, transform.forward);
            return false;
        }

        public bool TryGetConfirmPressed()
        {
            return false;
        }

        public bool TryGetCancelPressed()
        {
            return false;
        }

        public bool TryGetHeightAdjust(out float delta)
        {
            delta = 0f;
            return false;
        }

        public bool TryGetYawAdjust(out float delta)
        {
            delta = 0f;
            return false;
        }

        public bool TryGetScaleAdjust(out float delta)
        {
            delta = 0f;
            return false;
        }
    }
}
