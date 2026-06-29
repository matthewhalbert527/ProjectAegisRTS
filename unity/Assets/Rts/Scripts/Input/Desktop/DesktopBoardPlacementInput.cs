using ProjectAegisRTS.UnityClient.Board;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.InputControls.Desktop
{
    public sealed class DesktopBoardPlacementInput : MonoBehaviour
    {
        public BoardPlacementController controller;
        public Camera sceneCamera;
        public Behaviour[] disableWhilePlacementActive;
        public float moveSpeed = 5f;
        public float yawSpeed = 75f;
        public float heightSpeed = 1.2f;
        public float scaleSpeed = 0.35f;

        void Update()
        {
            if (controller == null)
                return;

            if (Input.GetKeyDown(KeyCode.Tab))
                controller.TogglePlacementMode();

            var active = controller.IsPlacementModeActive;
            SetDisabledBehaviours(!active);
            if (!active)
                return;

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                controller.ConfirmPlacement();
            if (Input.GetKeyDown(KeyCode.Escape))
                controller.CancelPlacement();
            if (Input.GetKeyDown(KeyCode.R))
                controller.ResetPlacement();

            HandleMove();
            HandleYaw();
            HandleHeight();
            HandleScale();
        }

        void HandleMove()
        {
            var move = Vector3.zero;
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
                move.z += 1f;
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
                move.z -= 1f;
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
                move.x += 1f;
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
                move.x -= 1f;

            if (move.sqrMagnitude > 0.001f)
                controller.MoveHorizontal(move.normalized * moveSpeed * Time.deltaTime);
        }

        void HandleYaw()
        {
            var yaw = 0f;
            if (Input.GetKey(KeyCode.Q))
                yaw -= 1f;
            if (Input.GetKey(KeyCode.E))
                yaw += 1f;

            if (Mathf.Abs(yaw) > 0.001f)
                controller.AdjustYaw(yaw * yawSpeed * Time.deltaTime);
        }

        void HandleHeight()
        {
            var height = 0f;
            if (Input.GetKey(KeyCode.PageUp) || Input.GetKey(KeyCode.Z))
                height += 1f;
            if (Input.GetKey(KeyCode.PageDown) || Input.GetKey(KeyCode.X))
                height -= 1f;

            if (Mathf.Abs(height) > 0.001f)
                controller.AdjustHeight(height * heightSpeed * Time.deltaTime);
        }

        void HandleScale()
        {
            var scroll = Input.GetAxis("Mouse ScrollWheel");
            var modifier = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            if (modifier && Mathf.Abs(scroll) > 0.001f)
                controller.AdjustScale(scroll * scaleSpeed);
        }

        void SetDisabledBehaviours(bool enabledState)
        {
            if (disableWhilePlacementActive == null)
                return;

            for (var i = 0; i < disableWhilePlacementActive.Length; i++)
            {
                if (disableWhilePlacementActive[i] != null)
                    disableWhilePlacementActive[i].enabled = enabledState;
            }
        }
    }
}
