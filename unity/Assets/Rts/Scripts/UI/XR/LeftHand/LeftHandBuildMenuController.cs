using System.Collections.Generic;
using ProjectAegisRTS.Data;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.UI.Desktop;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.UI.XR.LeftHand
{
    public sealed class LeftHandBuildMenuController : MonoBehaviour
    {
        public RtsSimulationDriver driver;
        public LeftHandCommandRouter commandRouter;
        public LeftHandRadialMenuView radialView;
        public LeftHandBuildCategory activeCategory = LeftHandBuildCategory.Buildings;

        readonly List<LeftHandBuildItemViewModel> items = new List<LeftHandBuildItemViewModel>();
        string selectedActorTypeId = string.Empty;

        public bool IsOpen { get; private set; }
        public LeftHandBuildCategory ActiveCategory { get { return activeCategory; } }
        public string SelectedActorTypeId { get { return selectedActorTypeId; } }
        public IReadOnlyList<LeftHandBuildItemViewModel> Items { get { return items; } }

        void Start()
        {
            BuildIfNeeded();
        }

        void Update()
        {
            RefreshViewModels();
            if (radialView != null)
                radialView.Refresh(this);
        }

        public void Initialize(RtsSimulationDriver simulationDriver, LeftHandCommandRouter router, LeftHandRadialMenuView view)
        {
            driver = simulationDriver;
            commandRouter = router;
            radialView = view;
            BuildIfNeeded();
        }

        public void OpenMenu()
        {
            IsOpen = true;
            BuildIfNeeded();
        }

        public void CloseMenu()
        {
            IsOpen = false;
        }

        public void ToggleMenu()
        {
            IsOpen = !IsOpen;
            BuildIfNeeded();
        }

        public void SetCategory(LeftHandBuildCategory category)
        {
            activeCategory = category;
            selectedActorTypeId = string.Empty;
            RefreshViewModels();
        }

        public void SelectNextCategory()
        {
            SetCategory((LeftHandBuildCategory)(((int)activeCategory + 1) % 6));
        }

        public void SelectPreviousCategory()
        {
            SetCategory((LeftHandBuildCategory)(((int)activeCategory + 5) % 6));
        }

        public void SelectItemIndex(int oneBasedIndex)
        {
            RefreshViewModels();
            var activeItems = GetActiveCategoryItems();
            var index = oneBasedIndex - 1;
            if (index < 0 || index >= activeItems.Count)
                return;

            SelectItem(activeItems[index].ActorTypeId);
        }

        public RtsCommandResult QueueItemByIndex(int oneBasedIndex)
        {
            RefreshViewModels();
            var activeItems = GetActiveCategoryItems();
            var index = oneBasedIndex - 1;
            if (index < 0 || index >= activeItems.Count)
                return RtsCommandResult.Fail("NoBuildItem", "No build item exists at slot " + oneBasedIndex + ".");

            return QueueItem(activeItems[index].ActorTypeId);
        }

        public void SelectItem(string actorTypeId)
        {
            selectedActorTypeId = actorTypeId ?? string.Empty;
        }

        public RtsCommandResult QueueSelectedItem()
        {
            if (commandRouter == null || string.IsNullOrEmpty(selectedActorTypeId))
                return RtsCommandResult.Fail("NoBuildItem", "Select a build item first.");

            return commandRouter.QueueProduction(selectedActorTypeId);
        }

        public RtsCommandResult QueueItem(string actorTypeId)
        {
            SelectItem(actorTypeId);
            if (commandRouter == null)
                return RtsCommandResult.Fail("RouterMissing", "Left-hand command router is not available.");

            return commandRouter.QueueProduction(actorTypeId);
        }

        public List<LeftHandBuildItemViewModel> GetActiveCategoryItems()
        {
            RefreshViewModels();
            var result = new List<LeftHandBuildItemViewModel>();
            for (var i = 0; i < items.Count; i++)
                if (items[i].Category == activeCategory)
                    result.Add(items[i]);
            return result;
        }

        public void RefreshViewModels()
        {
            items.Clear();
            if (driver == null || driver.Rules == null)
                return;

            AddItems(DesktopProductionCatalog.ActiveMvpTypeIds, true);
            AddItems(DesktopProductionCatalog.FutureTypeIds, false);
        }

        void AddItems(string[] typeIds, bool activeMvp)
        {
            for (var i = 0; i < typeIds.Length; i++)
            {
                ActorDefinition definition;
                if (!driver.Rules.TryGetDefinition(typeIds[i], out definition))
                    continue;

                var production = FindProduction(typeIds[i]);
                var pending = production != null && production.State == "CompletedPendingPlacement";
                var available = activeMvp && HasProducer(definition);
                var viewModel = new LeftHandBuildItemViewModel
                {
                    ActorTypeId = definition.TypeId,
                    DisplayName = definition.DisplayName,
                    Category = MapCategory(definition),
                    Cost = definition.Production.Cost,
                    BuildTimeTicks = definition.Production.BuildTimeTicks,
                    IsAvailable = available || pending,
                    IsQueued = production != null,
                    IsProducing = production != null && production.State == "Producing",
                    IsPendingPlacement = pending,
                    Progress01 = production != null && production.BuildTimeTicks > 0 ? Mathf.Clamp01(production.ProgressTicks / (float)production.BuildTimeTicks) : 0f,
                    StatusText = BuildStatus(activeMvp, available, production, pending)
                };
                items.Add(viewModel);
            }
        }

        bool HasProducer(ActorDefinition definition)
        {
            if (definition.Production == null || string.IsNullOrEmpty(definition.Production.FactoryTypeId))
                return false;
            return driver.HasOwnedActorOfType(definition.Production.FactoryTypeId);
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

        static string BuildStatus(bool activeMvp, bool available, ProductionSnapshot production, bool pending)
        {
            if (pending)
                return "pending placement";
            if (production != null)
                return production.State;
            if (!activeMvp)
                return "future";
            return available ? "ready" : "producer missing";
        }

        static LeftHandBuildCategory MapCategory(ActorDefinition definition)
        {
            var desktop = DesktopProductionCatalog.GetCategory(definition);
            switch (desktop)
            {
                case DesktopProductionCategory.Defenses:
                    return LeftHandBuildCategory.Defenses;
                case DesktopProductionCategory.Infantry:
                    return LeftHandBuildCategory.Infantry;
                case DesktopProductionCategory.Vehicles:
                    return LeftHandBuildCategory.Vehicles;
                case DesktopProductionCategory.Aircraft:
                    return LeftHandBuildCategory.Aircraft;
                case DesktopProductionCategory.Support:
                    return LeftHandBuildCategory.Support;
                default:
                    return LeftHandBuildCategory.Buildings;
            }
        }

        void BuildIfNeeded()
        {
            if (radialView != null)
                radialView.Refresh(this);
        }
    }
}
