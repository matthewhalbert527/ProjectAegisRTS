using ProjectAegisRTS.UnityClient.InputControls.XR;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.InputControls.Desktop
{
    public sealed class DesktopRightHandInputSource : MonoBehaviour, IRightHandInputSource
    {
        public Camera sceneCamera;
        public bool IsAvailable { get { return enabled && (sceneCamera != null || Camera.main != null); } }

        public bool TryGetRay(out Ray ray)
        {
            var cameraToUse = sceneCamera != null ? sceneCamera : Camera.main;
            if (cameraToUse == null)
            {
                ray = default(Ray);
                return false;
            }

            ray = cameraToUse.ScreenPointToRay(Input.mousePosition);
            return true;
        }

        public bool GetHudToggleDown()
        {
            return Input.GetKeyDown(KeyCode.V);
        }

        public bool GetPrimaryCommandDown()
        {
            return Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
        }

        public bool GetPrimaryCommandHeld()
        {
            return Input.GetMouseButton(1);
        }

        public bool GetPrimaryCommandUp()
        {
            return Input.GetMouseButtonUp(1);
        }

        public bool GetCancelDown()
        {
            return Input.GetKeyDown(KeyCode.Escape);
        }

        public bool GetMoveModeDown()
        {
            return Input.GetKeyDown(KeyCode.M);
        }

        public bool GetAttackModeDown()
        {
            return Input.GetKeyDown(KeyCode.A);
        }

        public bool GetForceAttackModeDown()
        {
            return Input.GetKeyDown(KeyCode.F);
        }

        public bool GetBoardManipulationHeld()
        {
            return Input.GetKey(KeyCode.Space) || Input.GetMouseButton(2);
        }

        public bool GetBoardManipulationToggleDown()
        {
            return Input.GetMouseButtonDown(2);
        }

        public float GetRotateAxis()
        {
            if (Input.GetKey(KeyCode.E))
                return 1f;
            if (Input.GetKey(KeyCode.Q))
                return -1f;
            return 0f;
        }

        public float GetZoomAxis()
        {
            var scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f)
                return Mathf.Sign(scroll);
            return 0f;
        }

        public bool GetAlternateModifierHeld()
        {
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }
    }
}
