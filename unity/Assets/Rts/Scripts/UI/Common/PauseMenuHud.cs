using UnityEngine;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.UI.Common
{
    public sealed class PauseMenuHud : MonoBehaviour
    {
        RectTransform overlayRoot;
        RectTransform panelRoot;
        Text bodyText;
        Button backButton;

        PauseMenuController controller;

        public bool IsVisible
        {
            get { return overlayRoot != null && overlayRoot.gameObject.activeSelf; }
        }

        public void Initialize(PauseMenuController pauseController)
        {
            controller = pauseController;
            BuildIfNeeded();
            Hide();
        }

        public void ShowMain()
        {
            BuildIfNeeded();
            overlayRoot.gameObject.SetActive(true);
            bodyText.text = "Paused";
            backButton.gameObject.SetActive(false);
        }

        public void ShowSettings()
        {
            BuildIfNeeded();
            overlayRoot.gameObject.SetActive(true);
            bodyText.text = "Settings\nFullscreen and audio settings live on the boot Options screen for this prototype.";
            backButton.gameObject.SetActive(true);
        }

        public void ShowControls()
        {
            BuildIfNeeded();
            overlayRoot.gameObject.SetActive(true);
            bodyText.text =
                "Controls\n" +
                "Left-click selects. Right-click moves or attacks.\n" +
                "Use the right sidebar to build and command.\n" +
                "Esc opens this pause menu.\n" +
                "F8-F12 are developer debug panels when enabled.";
            backButton.gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (overlayRoot != null)
                overlayRoot.gameObject.SetActive(false);
        }

        public bool HasRequiredButtons()
        {
            BuildIfNeeded();
            return HasButton("Resume Button") &&
                HasButton("Restart Mission Button") &&
                HasButton("Settings Button") &&
                HasButton("Controls Button") &&
                HasButton("Quit To Menu Button") &&
                HasButton("Quit Game Button");
        }

        void BuildIfNeeded()
        {
            if (overlayRoot != null)
                return;

            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
                canvas = CreateCanvas();

            var existing = canvas.transform.Find("Pause Menu Overlay");
            var overlay = existing != null ? existing.gameObject : new GameObject("Pause Menu Overlay");
            if (existing == null)
                overlay.transform.SetParent(canvas.transform, false);
            overlayRoot = overlay.GetComponent<RectTransform>();
            if (overlayRoot == null)
                overlayRoot = overlay.AddComponent<RectTransform>();
            overlayRoot.anchorMin = Vector2.zero;
            overlayRoot.anchorMax = Vector2.one;
            overlayRoot.offsetMin = Vector2.zero;
            overlayRoot.offsetMax = Vector2.zero;
            RtsUiFactory.AddPanel(overlay, new Color(0f, 0f, 0f, 0.58f));

            var panel = overlay.transform.Find("Pause Menu Panel");
            var panelObject = panel != null ? panel.gameObject : new GameObject("Pause Menu Panel");
            if (panel == null)
                panelObject.transform.SetParent(overlay.transform, false);
            panelRoot = panelObject.GetComponent<RectTransform>();
            if (panelRoot == null)
                panelRoot = panelObject.AddComponent<RectTransform>();
            panelRoot.anchorMin = new Vector2(0.5f, 0.5f);
            panelRoot.anchorMax = new Vector2(0.5f, 0.5f);
            panelRoot.pivot = new Vector2(0.5f, 0.5f);
            panelRoot.sizeDelta = new Vector2(440f, 430f);
            panelRoot.anchoredPosition = Vector2.zero;
            RtsUiFactory.AddPanel(panelObject, new Color(0.05f, 0.06f, 0.075f, 0.98f));

            var layout = panelObject.GetComponent<VerticalLayoutGroup>();
            if (layout == null)
                layout = panelObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(22, 22, 18, 18);
            layout.spacing = 8f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var title = GetOrCreateText(panelObject.transform, "Pause Title", "Pause", 24, Color.white, TextAnchor.MiddleCenter);
            title.rectTransform.sizeDelta = new Vector2(396f, 34f);

            bodyText = GetOrCreateText(panelObject.transform, "Pause Body", "Paused", 14, new Color(0.84f, 0.90f, 0.95f, 1f), TextAnchor.UpperCenter);
            bodyText.rectTransform.sizeDelta = new Vector2(396f, 74f);

            AddButton("Resume Button", "Resume", () => controller.Resume());
            AddButton("Restart Mission Button", "Restart Mission", () => controller.RestartMission());
            AddButton("Settings Button", "Settings", () => controller.ShowSettings());
            AddButton("Controls Button", "Controls", () => controller.ShowControls());
            AddButton("Quit To Menu Button", "Quit to Menu", () => controller.QuitToMenu());
            AddButton("Quit Game Button", "Quit Game", () => controller.QuitGame());
            backButton = AddButton("Back Button", "Back", () => ShowMain());
            backButton.gameObject.SetActive(false);
        }

        Button AddButton(string objectName, string text, UnityEngine.Events.UnityAction action)
        {
            var child = panelRoot.transform.Find(objectName);
            var button = child != null ? child.GetComponent<Button>() : null;
            if (button == null)
                button = RtsUiFactory.CreateButton(panelRoot, objectName, text);
            button.GetComponent<RectTransform>().sizeDelta = new Vector2(396f, 38f);
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
            var label = button.GetComponentInChildren<Text>();
            if (label != null)
                label.text = text;
            return button;
        }

        bool HasButton(string objectName)
        {
            var child = panelRoot == null ? null : panelRoot.transform.Find(objectName);
            return child != null && child.GetComponent<Button>() != null;
        }

        static Text GetOrCreateText(Transform parent, string objectName, string text, int fontSize, Color color, TextAnchor anchor)
        {
            var child = parent.Find(objectName);
            var existing = child != null ? child.GetComponent<Text>() : null;
            if (existing != null)
                return existing;
            return RtsUiFactory.CreateText(parent, objectName, text, fontSize, color, anchor);
        }

        static Canvas CreateCanvas()
        {
            var canvasObject = new GameObject("Pause Menu Canvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<GraphicRaycaster>();
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }
    }
}
