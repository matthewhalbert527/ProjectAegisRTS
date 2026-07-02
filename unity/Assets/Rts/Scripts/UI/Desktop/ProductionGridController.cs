using System.Collections.Generic;
using ProjectAegisRTS.Data;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Scenario;
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
            public Image Background;
        }

        readonly List<Card> cards = new List<Card>();
        RtsSimulationDriver driver;
        DesktopUiCommandRouter router;
        VerticalSliceMissionFlowController missionFlowController;
        VerticalSliceProgressTracker progressTracker;
        DesktopProductionCategory activeCategory = DesktopProductionCategory.Buildings;
        int columns = 2;
        string lastBuildKey = string.Empty;

        public void Initialize(RtsSimulationDriver simulationDriver, DesktopUiCommandRouter commandRouter, ProductionCategoryTabs tabs, int productionGridColumns, VerticalSliceProgressTracker tracker = null, VerticalSliceMissionFlowController missionFlow = null)
        {
            driver = simulationDriver;
            router = commandRouter;
            progressTracker = tracker;
            missionFlowController = missionFlow;
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
            var background = button.GetComponent<Image>();
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

            cards.Add(new Card { TypeId = typeId, Button = button, Label = label, Progress = progress, Background = background });
        }

        void RefreshCards()
        {
            if (driver == null || driver.Rules == null)
                return;

            if (cards.Count == 0)
                RebuildCards();
            if (progressTracker != null)
                progressTracker.Refresh();
            if (missionFlowController == null)
                missionFlowController = FindAnyObjectByType<VerticalSliceMissionFlowController>();
            if (missionFlowController != null)
                missionFlowController.Refresh();

            for (var i = 0; i < cards.Count; i++)
            {
                var card = cards[i];
                ActorDefinition definition;
                if (!driver.Rules.TryGetDefinition(card.TypeId, out definition))
                    continue;

                ProductionSnapshot production = FindProduction(card.TypeId);
                var pending = production != null && production.State == "CompletedPendingPlacement";
                var future = DesktopProductionCatalog.IsFuturePlaceholder(card.TypeId) && !DesktopProductionCatalog.IsActiveMvp(card.TypeId);
                var missingFactory = MissingFactory(definition);
                var missingPrerequisite = MissingPrerequisite(card.TypeId);
                card.Button.interactable = (string.IsNullOrEmpty(missingFactory) && string.IsNullOrEmpty(missingPrerequisite)) || pending;

                var status = future ? "Advanced" : "Ready";
                if (!string.IsNullOrEmpty(missingFactory))
                    status = "Requires " + DisplayType(missingFactory);
                else if (!string.IsNullOrEmpty(missingPrerequisite))
                    status = "Requires " + DisplayType(missingPrerequisite);
                if (production != null)
                    status = pending ? "Ready to place" : production.State + " " + production.ProgressTicks + "/" + production.BuildTimeTicks;

                card.Label.text =
                    definition.DisplayName + "\n" +
                    BuildGroup(card.TypeId) + "  $" + definition.Production.Cost + "\n" +
                    RecommendationLabel(card.TypeId) +
                    status;

                if (card.Background != null)
                    card.Background.color = CardColor(card.TypeId, future, missingFactory, missingPrerequisite, pending);

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

        string MissingFactory(ActorDefinition definition)
        {
            if (definition == null || definition.Production == null || string.IsNullOrEmpty(definition.Production.FactoryTypeId))
                return string.Empty;
            return driver != null && driver.HasOwnedActorOfType(definition.Production.FactoryTypeId) ? string.Empty : definition.Production.FactoryTypeId;
        }

        string MissingPrerequisite(string typeId)
        {
            return driver == null ? string.Empty : driver.GetMissingProductionPrerequisiteTypeId(typeId);
        }

        string RecommendationLabel(string typeId)
        {
            if (progressTracker == null)
                progressTracker = FindAnyObjectByType<VerticalSliceProgressTracker>();
            var recommended = missionFlowController != null ? missionFlowController.RecommendedTypeId : (progressTracker == null ? string.Empty : progressTracker.recommendedTypeId);
            if (recommended != typeId)
                return string.Empty;
            return "NEXT ";
        }

        Color CardColor(string typeId, bool future, string missingFactory, string missingPrerequisite, bool pending)
        {
            if (pending)
                return new Color(0.20f, 0.42f, 0.28f, 0.98f);
            var recommended = missionFlowController != null ? missionFlowController.RecommendedTypeId : (progressTracker == null ? string.Empty : progressTracker.recommendedTypeId);
            if (recommended == typeId)
                return new Color(0.28f, 0.43f, 0.20f, 1f);
            if (!string.IsNullOrEmpty(missingFactory) || !string.IsNullOrEmpty(missingPrerequisite))
                return new Color(0.12f, 0.13f, 0.15f, 0.78f);
            if (future)
                return new Color(0.20f, 0.23f, 0.30f, 0.95f);
            return new Color(0.18f, 0.22f, 0.27f, 0.95f);
        }

        static string BuildGroup(string typeId)
        {
            if (typeId == "power_plant" || typeId == "advanced_power_plant")
                return "Power";
            if (typeId == "refinery" || typeId == "harvester")
                return "Economy";
            if (typeId == "barracks" || typeId == "war_factory" || typeId == "dual_helipad")
                return "Production";
            if (typeId.Contains("tank") || typeId.Contains("infantry") || typeId.Contains("tower") || typeId.Contains("aircraft"))
                return "Combat";
            return "Support";
        }

        static string DisplayType(string typeId)
        {
            return string.IsNullOrEmpty(typeId) ? "producer" : typeId.Replace("_", " ");
        }
    }
}
