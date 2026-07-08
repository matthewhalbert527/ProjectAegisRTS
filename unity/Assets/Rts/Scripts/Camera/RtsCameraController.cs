using ProjectAegisRTS.UnityClient.CoreBridge;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectAegisRTS.UnityClient.CameraControls
{
    public sealed class RtsCameraController : MonoBehaviour
    {
        public float panSpeed = 32f;
        public float zoomSpeed = 28f;
        public float rotateSpeed = 75f;
        public float minHeight = 7f;
        public float maxHeight = 38f;
        public float minOrthographicSize = 8f;
        public float maxOrthographicSize = 60f;
        public bool useOrthographicStage1View = true;
        public float orthographicSize = 28f;
        public bool preserveConfiguredTransform;
        public bool enableWasdPan = true;
        public bool enableArrowPan = true;
        public bool enableEdgePan = true;
        public bool enableMouseDragPan = true;
        public float edgePanMarginPixels = 24f;
        public float fastPanMultiplier = 1.75f;
        public float boardClampPadding = 3f;

        BoardCoordinateMapper mapper;
        Camera controlledCamera;
        Vector3 dragGroundPoint;
        bool draggingCamera;

        public void Configure(BoardCoordinateMapper coordinateMapper)
        {
            mapper = coordinateMapper;
            if (mapper == null)
                return;

            var center = mapper.BoardCenterWorld;
            controlledCamera = GetComponent<Camera>();
            if (controlledCamera != null)
            {
                controlledCamera.nearClipPlane = 0.1f;
                controlledCamera.farClipPlane = 1000f;
                controlledCamera.orthographic = useOrthographicStage1View;
                if (controlledCamera.orthographic)
                    controlledCamera.orthographicSize = orthographicSize;
            }

            if (!preserveConfiguredTransform)
            {
                transform.position = new Vector3(center.x, maxHeight, center.z - 42f);
                transform.rotation = Quaternion.Euler(60f, 0f, 0f);
            }
        }

        void Update()
        {
            if (mapper == null)
                return;

            HandleKeyboardPan();
            HandleRotation();
            HandleZoom();
            HandleEdgePan();
            HandleMouseDragPan();
        }

        void HandleKeyboardPan()
        {
            Vector3 forward;
            Vector3 right;
            GetPlanarCameraAxes(out forward, out right);

            var move = Vector3.zero;
            if (enableWasdPan)
            {
                if (Input.GetKey(KeyCode.W))
                    move += forward;
                if (Input.GetKey(KeyCode.S))
                    move -= forward;
                if (Input.GetKey(KeyCode.D))
                    move += right;
                if (Input.GetKey(KeyCode.A))
                    move -= right;
            }

            if (enableArrowPan)
            {
                if (Input.GetKey(KeyCode.UpArrow))
                    move += forward;
                if (Input.GetKey(KeyCode.DownArrow))
                    move -= forward;
                if (Input.GetKey(KeyCode.RightArrow))
                    move += right;
                if (Input.GetKey(KeyCode.LeftArrow))
                    move -= right;
            }

            if (move.sqrMagnitude > 0.0001f)
                ApplyPan(move.normalized * CurrentPanSpeed() * Time.deltaTime);
        }

        void GetPlanarCameraAxes(out Vector3 forward, out Vector3 right)
        {
            forward = transform.forward;
            forward.y = 0f;
            forward.Normalize();
            right = transform.right;
            right.y = 0f;
            right.Normalize();

            if (forward.sqrMagnitude < 0.0001f)
                forward = Vector3.forward;
            if (right.sqrMagnitude < 0.0001f)
                right = Vector3.right;
        }

        void HandleRotation()
        {
            var direction = 0f;
            if (Input.GetKey(KeyCode.Q))
                direction -= 1f;
            if (Input.GetKey(KeyCode.E))
                direction += 1f;

            if (Mathf.Abs(direction) > 0.01f)
                transform.RotateAround(mapper.BoardCenterWorld, Vector3.up, direction * rotateSpeed * Time.deltaTime);
        }

        void HandleZoom()
        {
            var scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) < 0.001f)
                return;

            if (controlledCamera != null && controlledCamera.orthographic)
            {
                controlledCamera.orthographicSize = Mathf.Clamp(controlledCamera.orthographicSize - scroll * zoomSpeed, minOrthographicSize, maxOrthographicSize);
                orthographicSize = controlledCamera.orthographicSize;
                ClampCameraToBoard();
                return;
            }

            transform.position += transform.forward * scroll * zoomSpeed;
            var position = transform.position;
            position.y = Mathf.Clamp(position.y, minHeight, maxHeight);
            transform.position = position;
            ClampCameraToBoard();
        }

        void HandleEdgePan()
        {
            if (!enableEdgePan || IsPointerOverUi())
                return;

            var mouse = Input.mousePosition;
            if (mouse.x < 0f || mouse.y < 0f || mouse.x > Screen.width || mouse.y > Screen.height)
                return;

            Vector3 forward;
            Vector3 right;
            GetPlanarCameraAxes(out forward, out right);

            var move = Vector3.zero;
            if (mouse.x <= edgePanMarginPixels)
                move -= right;
            else if (mouse.x >= Screen.width - edgePanMarginPixels)
                move += right;
            if (mouse.y <= edgePanMarginPixels)
                move -= forward;
            else if (mouse.y >= Screen.height - edgePanMarginPixels)
                move += forward;

            if (move.sqrMagnitude > 0.0001f)
                ApplyPan(move.normalized * CurrentPanSpeed() * Time.deltaTime);
        }

        void HandleMouseDragPan()
        {
            if (!enableMouseDragPan)
                return;

            var dragButtonDown = Input.GetMouseButtonDown(2) ||
                (IsCameraPanModifierHeld() && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)));
            if (dragButtonDown && !IsPointerOverUi())
            {
                draggingCamera = TryScreenPointToGround(Input.mousePosition, out dragGroundPoint);
                return;
            }

            if (!IsDragPanHeld())
            {
                draggingCamera = false;
                return;
            }

            if (!draggingCamera)
                return;

            Vector3 currentGroundPoint;
            if (!TryScreenPointToGround(Input.mousePosition, out currentGroundPoint))
                return;

            ApplyPan(dragGroundPoint - currentGroundPoint);
        }

        void ApplyPan(Vector3 worldDelta)
        {
            if (worldDelta.sqrMagnitude <= 0.000001f)
                return;

            transform.position += worldDelta;
            ClampCameraToBoard();
        }

        float CurrentPanSpeed()
        {
            var speed = panSpeed;
            if (controlledCamera != null && controlledCamera.orthographic)
                speed *= Mathf.Clamp(controlledCamera.orthographicSize / 28f, 0.45f, 2.4f);
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                speed *= fastPanMultiplier;
            return speed;
        }

        void ClampCameraToBoard()
        {
            if (mapper == null)
                return;

            Vector3 centerGroundPoint;
            if (!TryViewportPointToGround(new Vector3(0.5f, 0.5f, 0f), out centerGroundPoint))
                return;

            var root = mapper.BoardRoot != null ? mapper.BoardRoot : mapper.transform;
            var local = root.InverseTransformPoint(centerGroundPoint);
            var size = mapper.BoardSizeWorld;
            var clamped = new Vector3(
                Mathf.Clamp(local.x, -boardClampPadding, size.x + boardClampPadding),
                local.y,
                Mathf.Clamp(local.z, -boardClampPadding, size.z + boardClampPadding));

            var localDelta = clamped - local;
            if (localDelta.sqrMagnitude <= 0.000001f)
                return;

            transform.position += root.TransformVector(localDelta);
        }

        bool TryScreenPointToGround(Vector3 screenPoint, out Vector3 groundPoint)
        {
            if (controlledCamera == null)
                controlledCamera = GetComponent<Camera>();
            if (controlledCamera == null)
            {
                groundPoint = Vector3.zero;
                return false;
            }

            return TryRayToGround(controlledCamera.ScreenPointToRay(screenPoint), out groundPoint);
        }

        bool TryViewportPointToGround(Vector3 viewportPoint, out Vector3 groundPoint)
        {
            if (controlledCamera == null)
                controlledCamera = GetComponent<Camera>();
            if (controlledCamera == null)
            {
                groundPoint = Vector3.zero;
                return false;
            }

            return TryRayToGround(controlledCamera.ViewportPointToRay(viewportPoint), out groundPoint);
        }

        bool TryRayToGround(Ray ray, out Vector3 groundPoint)
        {
            var root = mapper != null && mapper.BoardRoot != null ? mapper.BoardRoot : null;
            var plane = new Plane(root == null ? Vector3.up : root.up, root == null ? Vector3.zero : root.position);
            float distance;
            if (!plane.Raycast(ray, out distance))
            {
                groundPoint = Vector3.zero;
                return false;
            }

            groundPoint = ray.GetPoint(distance);
            return true;
        }

        static bool IsDragPanHeld()
        {
            return Input.GetMouseButton(2) ||
                (IsCameraPanModifierHeld() && (Input.GetMouseButton(0) || Input.GetMouseButton(1)));
        }

        public static bool IsCameraPanModifierHeld()
        {
            return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        }

        static bool IsPointerOverUi()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
    }
}
