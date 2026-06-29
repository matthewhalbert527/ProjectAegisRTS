using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.UI.XR.LeftHand
{
    public sealed class LeftHandRadialMenuView : MonoBehaviour
    {
        readonly List<Button> categoryButtons = new List<Button>();
        readonly List<Button> itemButtons = new List<Button>();
        readonly List<Text> itemLabels = new List<Text>();
        readonly List<Slider> itemProgress = new List<Slider>();

        public LeftHandBuildMenuController menuController;
        public Transform followTarget;
        public Vector3 followOffset = Vector3.zero;

        Text titleText;
        Text statusText;
        RectTransform itemRoot;
        RectTransform categoryRoot;
        Canvas parentCanvas;

        void Start()
        {
            BuildIfNeeded();
        }

        void Update()
        {
            if (parentCanvas != null && parentCanvas.renderMode == RenderMode.WorldSpace && followTarget != null)
            {
                parentCanvas.transform.position = followTarget.position + followOffset;
                parentCanvas.transform.rotation = followTarget.rotation;
            }

            if (menuController != null)
                Refresh(menuController);
        }

        public void Refresh(LeftHandBuildMenuController controller)
        {
            menuController = controller;
            BuildIfNeeded();
            gameObject.SetActive(controller == null || controller.IsOpen);
            if (controller == null)
                return;

            titleText.text = "LEFT HAND BUILD";
            statusText.text = "C toggle  F1-F6 category  1-8 queue  Esc cancel";
            RefreshCategories(controller.ActiveCategory);
            RefreshItems(controller.GetActiveCategoryItems(), controller.SelectedActorTypeId);
        }

        void BuildIfNeeded()
        {
            if (titleText != null)
                return;

            parentCanvas = GetComponentInParent<Canvas>();
            RtsUiFactory.Stretch(gameObject, Vector2.zero, Vector2.zero);
            RtsUiFactory.AddPanel(gameObject, new Color(0.03f, 0.04f, 0.05f, 0.88f));

            titleText = RtsUiFactory.CreateText(transform, "Title", "LEFT HAND BUILD", 18, Color.white, TextAnchor.MiddleLeft);
            titleText.rectTransform.anchorMin = new Vector2(0f, 1f);
            titleText.rectTransform.anchorMax = new Vector2(1f, 1f);
            titleText.rectTransform.offsetMin = new Vector2(12f, -40f);
            titleText.rectTransform.offsetMax = new Vector2(-12f, -8f);

            var categoryObject = new GameObject("Category Ring");
            categoryObject.transform.SetParent(transform, false);
            categoryRoot = categoryObject.AddComponent<RectTransform>();
            categoryRoot.anchorMin = new Vector2(0f, 1f);
            categoryRoot.anchorMax = new Vector2(1f, 1f);
            categoryRoot.offsetMin = new Vector2(10f, -94f);
            categoryRoot.offsetMax = new Vector2(-10f, -46f);
            var categoryLayout = categoryObject.AddComponent<GridLayoutGroup>();
            categoryLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            categoryLayout.constraintCount = 3;
            categoryLayout.cellSize = new Vector2(116f, 20f);
            categoryLayout.spacing = new Vector2(6f, 5f);

            var itemObject = new GameObject("Build Items");
            itemObject.transform.SetParent(transform, false);
            itemRoot = itemObject.AddComponent<RectTransform>();
            itemRoot.anchorMin = new Vector2(0f, 0f);
            itemRoot.anchorMax = new Vector2(1f, 1f);
            itemRoot.offsetMin = new Vector2(10f, 30f);
            itemRoot.offsetMax = new Vector2(-10f, -104f);
            var itemLayout = itemObject.AddComponent<GridLayoutGroup>();
            itemLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            itemLayout.constraintCount = 2;
            itemLayout.cellSize = new Vector2(176f, 68f);
            itemLayout.spacing = new Vector2(7f, 7f);

            statusText = RtsUiFactory.CreateText(transform, "Instructions", "", 11, new Color(0.80f, 0.88f, 0.95f, 1f), TextAnchor.MiddleLeft);
            statusText.rectTransform.anchorMin = new Vector2(0f, 0f);
            statusText.rectTransform.anchorMax = new Vector2(1f, 0f);
            statusText.rectTransform.offsetMin = new Vector2(12f, 6f);
            statusText.rectTransform.offsetMax = new Vector2(-12f, 28f);

            for (var i = 0; i < 6; i++)
                CreateCategoryButton((LeftHandBuildCategory)i);
        }

        void RefreshCategories(LeftHandBuildCategory active)
        {
            for (var i = 0; i < categoryButtons.Count; i++)
            {
                var image = categoryButtons[i].GetComponent<Image>();
                image.color = (int)active == i ? new Color(0.15f, 0.42f, 0.58f, 0.95f) : new Color(0.12f, 0.15f, 0.18f, 0.92f);
            }
        }

        void RefreshItems(IReadOnlyList<LeftHandBuildItemViewModel> items, string selectedActorTypeId)
        {
            while (itemButtons.Count < items.Count)
                CreateItemButton(itemButtons.Count);

            for (var i = 0; i < itemButtons.Count; i++)
            {
                var active = i < items.Count;
                itemButtons[i].gameObject.SetActive(active);
                if (!active)
                    continue;

                var item = items[i];
                var selected = item.ActorTypeId == selectedActorTypeId;
                itemButtons[i].interactable = item.IsAvailable;
                itemButtons[i].GetComponent<Image>().color = selected ? new Color(0.20f, 0.48f, 0.34f, 0.96f) : new Color(0.11f, 0.13f, 0.16f, 0.95f);
                itemLabels[i].text = (i + 1) + ". " + item.DisplayName + "\n" + item.ActorTypeId + "\n" + item.Cost + "c  " + item.StatusText;
                itemProgress[i].gameObject.SetActive(item.IsQueued || item.IsProducing || item.IsPendingPlacement);
                itemProgress[i].value = item.Progress01;
            }
        }

        void CreateCategoryButton(LeftHandBuildCategory category)
        {
            var button = RtsUiFactory.CreateButton(categoryRoot, "Category " + category, category.ToString());
            button.GetComponent<RectTransform>().sizeDelta = new Vector2(116f, 20f);
            var label = button.GetComponentInChildren<Text>();
            label.fontSize = 10;
            var captured = category;
            button.onClick.AddListener(() =>
            {
                if (menuController != null)
                    menuController.SetCategory(captured);
            });
            categoryButtons.Add(button);
        }

        void CreateItemButton(int index)
        {
            var button = RtsUiFactory.CreateButton(itemRoot, "Build Item " + index, string.Empty);
            button.GetComponent<RectTransform>().sizeDelta = new Vector2(176f, 68f);
            var label = button.GetComponentInChildren<Text>();
            label.alignment = TextAnchor.UpperLeft;
            label.fontSize = 10;
            label.rectTransform.offsetMin = new Vector2(6f, 18f);
            label.rectTransform.offsetMax = new Vector2(-6f, -4f);

            var progress = RtsUiFactory.CreateProgressBar(button.transform, "Progress");
            progress.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);
            progress.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 0f);
            progress.GetComponent<RectTransform>().offsetMin = new Vector2(6f, 5f);
            progress.GetComponent<RectTransform>().offsetMax = new Vector2(-6f, 15f);

            var captured = index;
            button.onClick.AddListener(() =>
            {
                if (menuController == null)
                    return;
                var activeItems = menuController.GetActiveCategoryItems();
                if (captured >= 0 && captured < activeItems.Count)
                    menuController.QueueItem(activeItems[captured].ActorTypeId);
            });

            itemButtons.Add(button);
            itemLabels.Add(label);
            itemProgress.Add(progress);
        }
    }
}
