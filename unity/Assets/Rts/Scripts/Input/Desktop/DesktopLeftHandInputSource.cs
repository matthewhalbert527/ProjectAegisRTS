using ProjectAegisRTS.UnityClient.InputControls.XR;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.InputControls.Desktop
{
    public sealed class DesktopLeftHandInputSource : MonoBehaviour, ILeftHandInputSource
    {
        public Camera sceneCamera;
        public bool requireLeftShiftToAim;
        public bool IsAvailable { get { return enabled && (sceneCamera != null || Camera.main != null); } }
        public int LastNumberKeyDown { get; private set; }

        void Update()
        {
            LastNumberKeyDown = 0;
            for (var i = 1; i <= 8; i++)
            {
                var key = (KeyCode)((int)KeyCode.Alpha0 + i);
                if (Input.GetKeyDown(key))
                {
                    LastNumberKeyDown = i;
                    break;
                }
            }
        }

        public bool TryGetRay(out Ray ray)
        {
            var cameraToUse = sceneCamera != null ? sceneCamera : Camera.main;
            if (cameraToUse == null || (requireLeftShiftToAim && !Input.GetKey(KeyCode.LeftShift)))
            {
                ray = default(Ray);
                return false;
            }

            ray = cameraToUse.ScreenPointToRay(Input.mousePosition);
            return true;
        }

        public bool GetMenuToggleDown()
        {
            return Input.GetKeyDown(KeyCode.C);
        }

        public bool GetPrimarySelectDown()
        {
            return Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
        }

        public bool GetPrimarySelectHeld()
        {
            return Input.GetMouseButton(0);
        }

        public bool GetPrimarySelectUp()
        {
            return Input.GetMouseButtonUp(0);
        }

        public bool GetSecondaryModifierHeld()
        {
            return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        }

        public bool GetCancelDown()
        {
            return Input.GetKeyDown(KeyCode.Escape);
        }

        public bool GetCategoryNextDown()
        {
            return Input.GetKeyDown(KeyCode.RightBracket);
        }

        public bool GetCategoryPreviousDown()
        {
            return Input.GetKeyDown(KeyCode.LeftBracket);
        }

        public float GetCategoryAxis()
        {
            if (Input.GetKeyDown(KeyCode.RightBracket))
                return 1f;
            if (Input.GetKeyDown(KeyCode.LeftBracket))
                return -1f;
            return 0f;
        }

        public int GetCategoryHotkeyDown()
        {
            if (Input.GetKeyDown(KeyCode.F1))
                return 1;
            if (Input.GetKeyDown(KeyCode.F2))
                return 2;
            if (Input.GetKeyDown(KeyCode.F3))
                return 3;
            if (Input.GetKeyDown(KeyCode.F4))
                return 4;
            if (Input.GetKeyDown(KeyCode.F5))
                return 5;
            if (Input.GetKeyDown(KeyCode.F6))
                return 6;
            return 0;
        }

        public bool GetStatusHudToggleDown()
        {
            return Input.GetKeyDown(KeyCode.BackQuote);
        }

        public float GetItemAxis()
        {
            var scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f)
                return Mathf.Sign(scroll);
            return 0f;
        }

        public bool GetCycleCandidateNextDown()
        {
            return Input.GetKeyDown(KeyCode.Tab) && !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift);
        }

        public bool GetCycleCandidatePreviousDown()
        {
            return Input.GetKeyDown(KeyCode.Tab) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
        }

        public bool GetLassoModifierHeld()
        {
            return Input.GetKey(KeyCode.L);
        }
    }
}
