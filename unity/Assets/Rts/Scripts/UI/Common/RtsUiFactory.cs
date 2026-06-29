using UnityEngine;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.UI.Common
{
    public static class RtsUiFactory
    {
        static Font cachedFont;

        public static Font DefaultFont
        {
            get
            {
                if (cachedFont == null)
                    cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

                return cachedFont;
            }
        }

        public static RectTransform Stretch(GameObject target, Vector2 offsetMin, Vector2 offsetMax)
        {
            var rect = target.GetComponent<RectTransform>();
            if (rect == null)
                rect = target.AddComponent<RectTransform>();

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            return rect;
        }

        public static Image AddPanel(GameObject target, Color color)
        {
            var image = target.GetComponent<Image>();
            if (image == null)
                image = target.AddComponent<Image>();

            image.color = color;
            return image;
        }

        public static Text AddText(GameObject target, string text, int fontSize, Color color, TextAnchor anchor)
        {
            var label = target.GetComponent<Text>();
            if (label == null)
                label = target.AddComponent<Text>();

            label.font = DefaultFont;
            label.text = text;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = anchor;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            return label;
        }

        public static Text CreateText(Transform parent, string name, string text, int fontSize, Color color, TextAnchor anchor)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            Stretch(obj, Vector2.zero, Vector2.zero);
            return AddText(obj, text, fontSize, color, anchor);
        }

        public static Button CreateButton(Transform parent, string name, string text)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(120f, 32f);
            var image = obj.AddComponent<Image>();
            image.color = new Color(0.18f, 0.22f, 0.27f, 0.95f);
            var button = obj.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = image.color;
            colors.highlightedColor = new Color(0.28f, 0.36f, 0.43f, 1f);
            colors.pressedColor = new Color(0.10f, 0.13f, 0.17f, 1f);
            colors.disabledColor = new Color(0.12f, 0.12f, 0.12f, 0.55f);
            button.colors = colors;

            var label = CreateText(obj.transform, "Label", text, 13, Color.white, TextAnchor.MiddleCenter);
            label.raycastTarget = false;
            return button;
        }

        public static Slider CreateProgressBar(Transform parent, string name)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(120f, 12f);

            var background = new GameObject("Background");
            background.transform.SetParent(obj.transform, false);
            Stretch(background, Vector2.zero, Vector2.zero);
            AddPanel(background, new Color(0.08f, 0.09f, 0.10f, 0.9f));

            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(obj.transform, false);
            Stretch(fillArea, Vector2.zero, Vector2.zero);

            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            Stretch(fill, Vector2.zero, Vector2.zero);
            AddPanel(fill, new Color(0.26f, 0.72f, 0.40f, 0.95f));

            var slider = obj.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0f;
            slider.transition = Selectable.Transition.None;
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.targetGraphic = background.GetComponent<Image>();
            slider.interactable = false;
            return slider;
        }
    }
}
