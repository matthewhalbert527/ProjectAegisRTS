using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.UI.Common;
using ProjectAegisRTS.UnityClient.UI.Desktop;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.CameraControls
{
    public sealed class PlayerFacingCameraFramer : MonoBehaviour
    {
        public Camera targetCamera;
        public BoardCoordinateMapper mapper;
        public PcGameplaySafeAreaController safeAreaController;
        public PlayerFacingUiModeController uiModeController;
        public bool applyOnStart = true;
        public bool applyOnScreenChange = true;
        public bool logOnApply = true;
        public float boardPadding = 1.08f;
        public float minOrthographicSize = 16f;
        public float maxOrthographicSize = 24f;
        public float preferredCameraHeight = 30f;

        int lastScreenWidth = -1;
        int lastScreenHeight = -1;
        Rect lastCameraRect = new Rect(-1f, -1f, -1f, -1f);
        float lastOrthographicSize = -1f;
        Vector3 lastCameraPosition = new Vector3(float.NaN, float.NaN, float.NaN);
        PcGameplaySafeAreaSnapshot lastSafeArea;

        public PcGameplaySafeAreaSnapshot LastSafeArea { get { return lastSafeArea; } }

        void Awake()
        {
            ResolveReferences();
        }

        void Start()
        {
            if (applyOnStart)
                ApplyFraming();
        }

        void LateUpdate()
        {
            if (!applyOnScreenChange)
                return;
            if (Screen.width == lastScreenWidth && Screen.height == lastScreenHeight)
                return;

            ApplyFraming();
        }

        public PcGameplaySafeAreaSnapshot ApplyFraming()
        {
            return ApplyFramingForScreen(Screen.width, Screen.height);
        }

        public PcGameplaySafeAreaSnapshot ApplyFramingForScreen(int screenWidth, int screenHeight)
        {
            ResolveReferences();

            if (targetCamera == null)
                return default(PcGameplaySafeAreaSnapshot);

            var snapshot = safeAreaController != null
                ? safeAreaController.CalculateForScreen(screenWidth, screenHeight)
                : new PcGameplaySafeAreaSnapshot(false, screenWidth, screenHeight, 0f, 0f, 0f, 0f, 0f, 0f, new Rect(0f, 0f, Mathf.Max(1, screenWidth), Mathf.Max(1, screenHeight)), new Rect(0f, 0f, 1f, 1f));

            var usePcSafeArea = snapshot.UsesPcSafeArea && IsPcDesktopSafeAreaActive();
            targetCamera.rect = usePcSafeArea ? snapshot.NormalizedCameraRect : new Rect(0f, 0f, 1f, 1f);

            targetCamera.orthographic = true;
            targetCamera.nearClipPlane = 0.1f;
            targetCamera.farClipPlane = 1000f;
            if (mapper != null)
                FrameBoard(snapshot);

            lastScreenWidth = screenWidth;
            lastScreenHeight = screenHeight;
            lastSafeArea = snapshot;

            if (logOnApply && HasFramingChanged())
            {
                Debug.Log(PcGameplaySafeAreaController.Describe(snapshot) +
                    " orthographicSize=" + targetCamera.orthographicSize.ToString("0.00") +
                    " cameraPosition=" + targetCamera.transform.position.ToString("F2"));
                RecordLastFraming();
            }

            return snapshot;
        }

        public Rect GetBoardScreenBounds()
        {
            ResolveReferences();
            if (targetCamera == null || mapper == null)
                return new Rect();

            var corners = GetBoardCorners();
            var min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            var max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            for (var i = 0; i < corners.Length; i++)
            {
                var screen = targetCamera.WorldToScreenPoint(corners[i]);
                min = Vector2.Min(min, new Vector2(screen.x, screen.y));
                max = Vector2.Max(max, new Vector2(screen.x, screen.y));
            }

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        public bool IsBoardInsideSafeArea(float tolerancePx)
        {
            ResolveReferences();
            if (safeAreaController == null)
                return true;
            return safeAreaController.DoesScreenRectFitGameplaySafeArea(GetBoardScreenBounds(), tolerancePx);
        }

        void FrameBoard(PcGameplaySafeAreaSnapshot snapshot)
        {
            var viewport = snapshot.GameplayViewportRect;
            var aspect = viewport.height <= 0f ? targetCamera.aspect : Mathf.Max(0.1f, viewport.width / viewport.height);
            CenterCameraOnBoard();

            var corners = GetBoardCorners();
            var minX = float.PositiveInfinity;
            var maxX = float.NegativeInfinity;
            var minY = float.PositiveInfinity;
            var maxY = float.NegativeInfinity;
            for (var i = 0; i < corners.Length; i++)
            {
                var local = targetCamera.transform.InverseTransformPoint(corners[i]);
                minX = Mathf.Min(minX, local.x);
                maxX = Mathf.Max(maxX, local.x);
                minY = Mathf.Min(minY, local.y);
                maxY = Mathf.Max(maxY, local.y);
            }

            var requiredForHeight = (maxY - minY) * 0.5f;
            var requiredForWidth = (maxX - minX) * 0.5f / aspect;
            var required = Mathf.Max(requiredForHeight, requiredForWidth) * boardPadding;
            targetCamera.orthographicSize = Mathf.Clamp(required, minOrthographicSize, maxOrthographicSize);

            var cameraController = targetCamera.GetComponent<RtsCameraController>();
            if (cameraController != null)
            {
                cameraController.preserveConfiguredTransform = true;
                cameraController.orthographicSize = targetCamera.orthographicSize;
                cameraController.maxHeight = targetCamera.transform.position.y;
            }
        }

        void CenterCameraOnBoard()
        {
            var center = mapper.BoardCenterWorld;
            var rotation = targetCamera.transform.rotation;
            var forward = rotation * Vector3.forward;
            var height = preferredCameraHeight > 0f ? preferredCameraHeight : targetCamera.transform.position.y;
            if (height <= center.y + 1f)
                height = center.y + 30f;

            var distanceToGround = Mathf.Abs(forward.y) > 0.001f ? (height - center.y) / -forward.y : 0f;
            var position = center - forward * distanceToGround;
            position.y = height;
            targetCamera.transform.position = position;
        }

        Vector3[] GetBoardCorners()
        {
            var root = mapper.BoardRoot != null ? mapper.BoardRoot : mapper.transform;
            var size = mapper.BoardSizeWorld;
            return new[]
            {
                root.TransformPoint(new Vector3(0f, 0f, 0f)),
                root.TransformPoint(new Vector3(size.x, 0f, 0f)),
                root.TransformPoint(new Vector3(0f, 0f, size.z)),
                root.TransformPoint(new Vector3(size.x, 0f, size.z))
            };
        }

        void ResolveReferences()
        {
            if (targetCamera == null)
                targetCamera = GetComponent<Camera>();
            if (targetCamera == null)
                targetCamera = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
            if (mapper == null)
                mapper = FindAnyObjectByType<BoardCoordinateMapper>();
            if (safeAreaController == null)
                safeAreaController = FindAnyObjectByType<PcGameplaySafeAreaController>();
            if (safeAreaController == null)
            {
                var host = FindAnyObjectByType<DesktopRtsHudRoot>();
                if (host != null)
                    safeAreaController = host.gameObject.GetComponent<PcGameplaySafeAreaController>() ?? host.gameObject.AddComponent<PcGameplaySafeAreaController>();
            }
            if (uiModeController == null)
                uiModeController = FindAnyObjectByType<PlayerFacingUiModeController>();
        }

        bool IsPcDesktopSafeAreaActive()
        {
            if (uiModeController == null)
                return true;
            return uiModeController.IsPcDesktopMode() || uiModeController.IsDebugHybridMode();
        }

        bool HasFramingChanged()
        {
            return !Approximately(lastCameraRect, targetCamera.rect) ||
                Mathf.Abs(lastOrthographicSize - targetCamera.orthographicSize) > 0.01f ||
                Vector3.Distance(lastCameraPosition, targetCamera.transform.position) > 0.01f;
        }

        void RecordLastFraming()
        {
            lastCameraRect = targetCamera.rect;
            lastOrthographicSize = targetCamera.orthographicSize;
            lastCameraPosition = targetCamera.transform.position;
        }

        static bool Approximately(Rect left, Rect right)
        {
            return Mathf.Abs(left.x - right.x) <= 0.001f &&
                Mathf.Abs(left.y - right.y) <= 0.001f &&
                Mathf.Abs(left.width - right.width) <= 0.001f &&
                Mathf.Abs(left.height - right.height) <= 0.001f;
        }
    }
}
