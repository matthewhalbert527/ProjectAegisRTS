using ProjectAegisRTS.UnityClient.CoreBridge;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.CameraControls
{
    public sealed class RtsCameraController : MonoBehaviour
    {
        public float panSpeed = 12f;
        public float zoomSpeed = 28f;
        public float rotateSpeed = 75f;
        public float minHeight = 7f;
        public float maxHeight = 38f;
        public bool useOrthographicStage1View = true;
        public float orthographicSize = 28f;
        public bool preserveConfiguredTransform;

        BoardCoordinateMapper mapper;
        Camera controlledCamera;
        Vector3 lastMousePosition;

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
            HandleMiddleMousePan();
        }

        void HandleKeyboardPan()
        {
            var forward = transform.forward;
            forward.y = 0f;
            forward.Normalize();
            var right = transform.right;
            right.y = 0f;
            right.Normalize();

            var move = Vector3.zero;
            if (Input.GetKey(KeyCode.W))
                move += forward;
            if (Input.GetKey(KeyCode.S))
                move -= forward;
            if (Input.GetKey(KeyCode.D))
                move += right;
            if (Input.GetKey(KeyCode.A))
                move -= right;

            if (move.sqrMagnitude > 0.0001f)
                transform.position += move.normalized * panSpeed * Time.deltaTime;
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
                controlledCamera.orthographicSize = Mathf.Clamp(controlledCamera.orthographicSize - scroll * zoomSpeed, 8f, 42f);
                return;
            }

            transform.position += transform.forward * scroll * zoomSpeed;
            var position = transform.position;
            position.y = Mathf.Clamp(position.y, minHeight, maxHeight);
            transform.position = position;
        }

        void HandleMiddleMousePan()
        {
            if (Input.GetMouseButtonDown(2))
                lastMousePosition = Input.mousePosition;

            if (!Input.GetMouseButton(2))
                return;

            var delta = Input.mousePosition - lastMousePosition;
            lastMousePosition = Input.mousePosition;
            var pan = (-transform.right * delta.x - transform.up * delta.y) * (panSpeed * 0.018f * Time.deltaTime);
            pan.y = 0f;
            transform.position += pan;
        }
    }
}
