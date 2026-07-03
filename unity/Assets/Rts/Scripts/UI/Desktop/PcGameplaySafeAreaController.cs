using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.UI.Desktop
{
    public sealed class PcGameplaySafeAreaController : MonoBehaviour
    {
        public DesktopRtsHudRoot desktopHud;
        public CncStyleSidebarLayout sidebarLayout;
        public PlayerFacingUiModeController uiModeController;
        public Canvas hudCanvas;
        public Vector2 canvasReferenceResolution = new Vector2(1920f, 1080f);
        [Range(0f, 1f)]
        public float canvasMatchWidthOrHeight = 0.5f;
        public float rightSidebarMarginPx = 24f;
        public float leftObjectiveMarginPx = 16f;
        public float topReservedPx = 12f;
        public float bottomReservedPx = 8f;
        public float fallbackSidebarWidth = 380f;
        public float minimumGameplayWidthPx = 420f;
        public bool logOnChange;

        int lastScreenWidth = -1;
        int lastScreenHeight = -1;
        PcGameplaySafeAreaSnapshot lastSnapshot;

        public float LeftReservedPx { get { return lastSnapshot.LeftReservedPx; } }
        public float RightReservedPx { get { return lastSnapshot.RightReservedPx; } }
        public float TopReservedPx { get { return lastSnapshot.TopReservedPx; } }
        public float BottomReservedPx { get { return lastSnapshot.BottomReservedPx; } }
        public float SidebarWidthPx { get { return lastSnapshot.SidebarWidthPx; } }
        public float ObjectiveWidthPx { get { return lastSnapshot.ObjectiveWidthPx; } }
        public Rect GameplayViewportRect { get { return lastSnapshot.GameplayViewportRect; } }
        public Rect NormalizedCameraRect { get { return lastSnapshot.NormalizedCameraRect; } }
        public bool UsesPcSafeArea { get { return lastSnapshot.UsesPcSafeArea; } }

        void Awake()
        {
            Refresh();
        }

        void Update()
        {
            if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
                Refresh();
        }

        public PcGameplaySafeAreaSnapshot Refresh()
        {
            ResolveReferences();
            lastSnapshot = CalculateForScreen(Screen.width, Screen.height);
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;

            if (logOnChange)
                Debug.Log(Describe(lastSnapshot));

            return lastSnapshot;
        }

        public PcGameplaySafeAreaSnapshot CalculateForScreen(int screenWidth, int screenHeight)
        {
            ResolveReferences();

            var safeWidth = Mathf.Max(1, screenWidth);
            var safeHeight = Mathf.Max(1, screenHeight);
            var usesPcSafeArea = IsPcDesktopSafeAreaActive();
            if (!usesPcSafeArea)
            {
                var full = new Rect(0f, 0f, safeWidth, safeHeight);
                return new PcGameplaySafeAreaSnapshot(
                    false,
                    safeWidth,
                    safeHeight,
                    0f,
                    0f,
                    0f,
                    0f,
                    0f,
                    0f,
                    full,
                    new Rect(0f, 0f, 1f, 1f));
            }

            var canvasScale = CalculateCanvasScale(safeWidth, safeHeight);
            var sidebarWidthPx = HasRightSidebar() ? GetSidebarWidthUnits() * canvasScale : 0f;
            var objectiveWidthPx = HasLeftObjectiveColumn() ? GetObjectiveColumnWidthForScreen(safeWidth, safeHeight) : 0f;
            var rightReserved = sidebarWidthPx > 0f ? sidebarWidthPx + rightSidebarMarginPx : 0f;
            var leftReserved = objectiveWidthPx > 0f ? objectiveWidthPx + leftObjectiveMarginPx : 0f;
            var topReserved = Mathf.Max(0f, topReservedPx);
            var bottomReserved = Mathf.Max(0f, bottomReservedPx);

            var viewportWidth = safeWidth - leftReserved - rightReserved;
            if (viewportWidth < minimumGameplayWidthPx)
            {
                var deficit = minimumGameplayWidthPx - viewportWidth;
                var leftReduction = Mathf.Min(leftReserved * 0.35f, deficit * 0.5f);
                leftReserved -= leftReduction;
                deficit -= leftReduction;
                var rightReduction = Mathf.Min(rightReserved * 0.25f, deficit);
                rightReserved -= rightReduction;
                viewportWidth = Mathf.Max(minimumGameplayWidthPx, safeWidth - leftReserved - rightReserved);
            }

            var viewportHeight = Mathf.Max(1f, safeHeight - topReserved - bottomReserved);
            var viewport = new Rect(leftReserved, bottomReserved, Mathf.Max(1f, viewportWidth), viewportHeight);
            var normalized = new Rect(
                viewport.x / safeWidth,
                viewport.y / safeHeight,
                viewport.width / safeWidth,
                viewport.height / safeHeight);

            return new PcGameplaySafeAreaSnapshot(
                true,
                safeWidth,
                safeHeight,
                leftReserved,
                rightReserved,
                topReserved,
                bottomReserved,
                sidebarWidthPx,
                objectiveWidthPx,
                viewport,
                normalized);
        }

        public string DescribeCurrent()
        {
            return Describe(lastSnapshot.ScreenWidth > 0 ? lastSnapshot : CalculateForScreen(Screen.width, Screen.height));
        }

        public static string Describe(PcGameplaySafeAreaSnapshot snapshot)
        {
            return "[Stage28.1 Layout] screen=" + snapshot.ScreenWidth + "x" + snapshot.ScreenHeight +
                " pcSafeArea=" + snapshot.UsesPcSafeArea +
                " leftReserved=" + snapshot.LeftReservedPx.ToString("0.0") +
                " rightReserved=" + snapshot.RightReservedPx.ToString("0.0") +
                " topReserved=" + snapshot.TopReservedPx.ToString("0.0") +
                " bottomReserved=" + snapshot.BottomReservedPx.ToString("0.0") +
                " sidebarWidth=" + snapshot.SidebarWidthPx.ToString("0.0") +
                " objectiveWidth=" + snapshot.ObjectiveWidthPx.ToString("0.0") +
                " viewport=" + RectToString(snapshot.GameplayViewportRect) +
                " cameraRect=" + RectToString(snapshot.NormalizedCameraRect);
        }

        public bool DoesScreenRectFitGameplaySafeArea(Rect screenRect, float tolerancePx)
        {
            var snapshot = lastSnapshot.ScreenWidth > 0 ? lastSnapshot : CalculateForScreen(Screen.width, Screen.height);
            return screenRect.xMin >= snapshot.GameplayViewportRect.xMin - tolerancePx &&
                screenRect.xMax <= snapshot.GameplayViewportRect.xMax + tolerancePx &&
                screenRect.yMin >= snapshot.GameplayViewportRect.yMin - tolerancePx &&
                screenRect.yMax <= snapshot.GameplayViewportRect.yMax + tolerancePx;
        }

        void ResolveReferences()
        {
            if (desktopHud == null)
                desktopHud = FindAnyObjectByType<DesktopRtsHudRoot>();
            if (sidebarLayout == null && desktopHud != null)
                sidebarLayout = desktopHud.cncSidebarLayout;
            if (sidebarLayout == null)
                sidebarLayout = FindAnyObjectByType<CncStyleSidebarLayout>();
            if (uiModeController == null)
                uiModeController = FindAnyObjectByType<PlayerFacingUiModeController>();
            if (hudCanvas == null && desktopHud != null)
                hudCanvas = desktopHud.canvas != null ? desktopHud.canvas : desktopHud.GetComponentInParent<Canvas>();

            if (hudCanvas != null)
            {
                var scaler = hudCanvas.GetComponent<UnityEngine.UI.CanvasScaler>();
                if (scaler != null && scaler.uiScaleMode == UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize)
                {
                    canvasReferenceResolution = scaler.referenceResolution;
                    canvasMatchWidthOrHeight = scaler.matchWidthOrHeight;
                }
            }
        }

        bool IsPcDesktopSafeAreaActive()
        {
            if (uiModeController == null)
                return true;

            return uiModeController.IsPcDesktopMode() || uiModeController.IsDebugHybridMode();
        }

        bool HasRightSidebar()
        {
            if (desktopHud == null || !desktopHud.gameObject.activeInHierarchy)
                return false;
            if (sidebarLayout == null)
                return desktopHud.sidebarController != null && desktopHud.sidebarController.gameObject.activeInHierarchy;
            return sidebarLayout.rightSidebarRoot != null && sidebarLayout.rightSidebarRoot.gameObject.activeInHierarchy;
        }

        bool HasLeftObjectiveColumn()
        {
            return HasSceneComponent<PlayerObjectiveHud>() ||
                HasSceneComponent<PlayerPromptHud>() ||
                HasSceneComponent<MatchObjectiveHud>() ||
                HasSceneComponent<VerticalSliceChecklistHud>();
        }

        float GetSidebarWidthUnits()
        {
            if (sidebarLayout != null && sidebarLayout.width > 0f)
                return sidebarLayout.width;
            if (desktopHud != null && desktopHud.sidebarWidth > 0f)
                return desktopHud.sidebarWidth;
            return fallbackSidebarWidth;
        }

        float CalculateCanvasScale(int screenWidth, int screenHeight)
        {
            var referenceWidth = Mathf.Max(1f, canvasReferenceResolution.x);
            var referenceHeight = Mathf.Max(1f, canvasReferenceResolution.y);
            var widthScale = Mathf.Max(0.0001f, screenWidth / referenceWidth);
            var heightScale = Mathf.Max(0.0001f, screenHeight / referenceHeight);
            var logWidth = Mathf.Log(widthScale, 2f);
            var logHeight = Mathf.Log(heightScale, 2f);
            var logWeightedAverage = Mathf.Lerp(logWidth, logHeight, Mathf.Clamp01(canvasMatchWidthOrHeight));
            return Mathf.Pow(2f, logWeightedAverage);
        }

        static float GetObjectiveColumnWidthForScreen(int screenWidth, int screenHeight)
        {
            var scale = HudScaleForScreen(screenWidth, screenHeight);
            var rightEdge = Mathf.Max(
                Mathf.Max(PlayerHudLayout.ObjectiveArea.xMax, PlayerHudLayout.PromptArea.xMax),
                Mathf.Max(PlayerHudLayout.MatchArea.xMax, PlayerHudLayout.ChecklistArea.xMax));
            return rightEdge * scale;
        }

        static float HudScaleForScreen(int screenWidth, int screenHeight)
        {
            var widthScale = screenWidth / 1920f;
            var heightScale = screenHeight / 1080f;
            return Mathf.Clamp(Mathf.Min(widthScale, heightScale), 1f, 1.6f);
        }

        static bool HasSceneComponent<T>() where T : Component
        {
            var components = Resources.FindObjectsOfTypeAll<T>();
            for (var i = 0; i < components.Length; i++)
            {
                var component = components[i];
                if (component != null && component.gameObject != null && component.gameObject.scene.IsValid() && component.gameObject.activeInHierarchy)
                    return true;
            }

            return false;
        }

        static string RectToString(Rect rect)
        {
            return "(" + rect.x.ToString("0.###") + "," + rect.y.ToString("0.###") + "," + rect.width.ToString("0.###") + "," + rect.height.ToString("0.###") + ")";
        }
    }

    public struct PcGameplaySafeAreaSnapshot
    {
        public readonly bool UsesPcSafeArea;
        public readonly int ScreenWidth;
        public readonly int ScreenHeight;
        public readonly float LeftReservedPx;
        public readonly float RightReservedPx;
        public readonly float TopReservedPx;
        public readonly float BottomReservedPx;
        public readonly float SidebarWidthPx;
        public readonly float ObjectiveWidthPx;
        public readonly Rect GameplayViewportRect;
        public readonly Rect NormalizedCameraRect;

        public PcGameplaySafeAreaSnapshot(
            bool usesPcSafeArea,
            int screenWidth,
            int screenHeight,
            float leftReservedPx,
            float rightReservedPx,
            float topReservedPx,
            float bottomReservedPx,
            float sidebarWidthPx,
            float objectiveWidthPx,
            Rect gameplayViewportRect,
            Rect normalizedCameraRect)
        {
            UsesPcSafeArea = usesPcSafeArea;
            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;
            LeftReservedPx = leftReservedPx;
            RightReservedPx = rightReservedPx;
            TopReservedPx = topReservedPx;
            BottomReservedPx = bottomReservedPx;
            SidebarWidthPx = sidebarWidthPx;
            ObjectiveWidthPx = objectiveWidthPx;
            GameplayViewportRect = gameplayViewportRect;
            NormalizedCameraRect = normalizedCameraRect;
        }
    }
}
