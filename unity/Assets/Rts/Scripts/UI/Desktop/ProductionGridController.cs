using System.Collections.Generic;
using ProjectAegisRTS.Data;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.UI.Desktop
{
    public sealed class ProductionGridController : MonoBehaviour
    {
        sealed class Card
        {
            public string TypeId;
            public Button Button;
            public Text Label;
            public Slider Progress;
        }

        readonly List<Card> cards = new List<Card>();
        RtsSimulationDriver driver;
        DesktopUiCommandRouter router;
        DesktopProductionCategory activeCategory = DesktopProductionCategory.Buildings;
        int columns = 2;
        string lastBuildKey = string.Empty;

        public void Initialize(RtsSimulationDriver simulationDriver, DesktopUiCommandRouter commandRouter, ProductionCategoryTabs tabs, int productionGridColumns)
        {
            driver = simulationDriver;
            router = commandRouter;
            columns = Mathf.Max(1, productionGridColumns);
            BuildIfNeeded();
            RebuildCards();
        }

        public void SetCategory(DesktopProductionCategory category)
        {
            if (activeCategory == category && cards.Count > 0)
                return;

            activeCategory = category;
            RebuildCards();
        }

        void Update()
        {
            RefreshCards();
        }

        void BuildIfNeeded()
        {
            RtsUiFactory.Stretch(gameObject, Vector2.zero, Vector2.zero);
            if (GetComponent<GridLayoutGroup>() == null)
            {
                var layout = gameObject.AddComponent<GridLayoutGroup>();
                layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                layout.constraintCount = columns;
                layout.cellSize = new Vector2(164f, 84f);
                layout.spacing = new Vector2(6f, 6f);
            }
        }

        void RebuildCards()
        {
            if (driver == null || driver.Rules == null)
                return;

            var key = activeCategory + ":" + driver.Rules.ActorDefinitions.Count;
            if (key == lastBuildKey && cards.Count > 0)
                return;

            lastBuildKey = key;
            for (var i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);
            cards.Clear();

            var typeIds = DesktopProductionCatalog.GetOrderedTypeIds(activeCategory, driver.Rules.ActorDefinitions);
            for (var i = 0; i < typeIds.Count; i++)
                CreateCard(typeIds[i]);
        }

        void CreateCard(string typeId)
        {
            ActorDefinition definition;
            if (!driver.Rules.TryGetDefinition(typeId, out definition))
                return;

            var button = RtsUiFactory.CreateButton(transform, "Production " + typeId, definition.DisplayName);
            var rect = button.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(164f, 84f);

            var label = button.GetComponentInChildren<Text>();
            label.alignment = TextAnchor.UpperLeft;
            label.fontSize = 12;
            label.rectTransform.offsetMin = new Vector2(8f, 24f);
            label.rectTransform.offsetMax = new Vector2(-8f, -4f);

            var progress = RtsUiFactory.CreateProgressBar(button.transform, "Progress");
            progress.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);
            progress.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 0f);
            progress.GetComponent<RectTransform>().offsetMin = new Vector2(8f, 8f);
            progress.GetComponent<RectTransform>().offsetMax = new Vector2(-8f, 20f);

            var captured = typeId;
            button.onClick.AddListener(() =>
            {
                if (router != null)
                    router.QueueProduction(captured);
            });

            cards.Add(new Card { TypeId = typeId, Button = button, Label = label, Progress = progress });
        }

        void RefreshCards()
        {
            if (driver == null || driver.Rules == null)
                return;

            if (cards.Count == 0)
                RebuildCards();

            for (var i = 0; i < cards.Count; i++)
            {
                var card = cards[i];
                ActorDefinition definition;
                if (!driver.Rules.TryGetDefinition(card.TypeId, out definition))
                    continue;

                ProductionSnapshot production = FindProduction(card.TypeId);
                var pending = production != null && production.State == "CompletedPendingPlacement";
                var future = DesktopProductionCatalog.IsFuturePlaceholder(card.TypeId) && !DesktopProductionCatalog.IsActiveMvp(card.TypeId);
                card.Button.interactable = !future || pending;

                var status = future ? "future" : "ready";
                if (production != null)
                    status = production.State;

                card.Label.text =
                    definition.DisplayName + "\n" +
                    card.TypeId + "\n" +
                    "Cost " + definition.Production.Cost + "  Ticks " + definition.Production.BuildTimeTicks + "\n" +
                    status;

                if (production != null && production.BuildTimeTicks > 0)
                {
                    card.Progress.gameObject.SetActive(true);
                    card.Progress.value = Mathf.Clamp01(production.ProgressTicks / (float)production.BuildTimeTicks);
                }
                else
                {
                    card.Progress.gameObject.SetActive(false);
                }
            }
        }

        ProductionSnapshot FindProduction(string typeId)
        {
            var player = driver.GetLocalPlayerSnapshot();
            if (player == null)
                return null;

            for (var i = 0; i < player.Production.Count; i++)
                if (player.Production[i].TypeId == typeId)
                    return player.Production[i];

            return null;
        }
    }
}
