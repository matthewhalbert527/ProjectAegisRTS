using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.UI.XR.LeftHand;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Selection
{
    public sealed class LeftHandLassoSelectionController : MonoBehaviour
    {
        readonly List<Int2> previewCells = new List<Int2>();
        public RtsSimulationDriver driver;
        public BoardCoordinateMapper mapper;
        public BoardRenderer boardRenderer;
        public LeftHandCommandRouter commandRouter;

        public bool IsActive { get; private set; }
        public Int2 StartCell { get; private set; }
        public Int2 EndCell { get; private set; }

        public void Initialize(RtsSimulationDriver simulationDriver, BoardCoordinateMapper coordinateMapper, BoardRenderer renderer, LeftHandCommandRouter router)
        {
            driver = simulationDriver;
            mapper = coordinateMapper;
            boardRenderer = renderer;
            commandRouter = router;
        }

        public void StartLasso(Int2 cell)
        {
            StartCell = cell;
            EndCell = cell;
            IsActive = true;
            RefreshPreview();
        }

        public void UpdateLasso(Int2 cell)
        {
            if (!IsActive)
                return;

            EndCell = cell;
            RefreshPreview();
        }

        public RtsCommandResult CompleteLasso(bool additive)
        {
            if (!IsActive)
                return RtsCommandResult.Ok("Lasso selection was not active.");

            IsActive = false;
            if (boardRenderer != null)
                boardRenderer.ClearPlacementPreview();

            var candidates = SelectionResolver.FindCandidatesInRect(driver.LatestSnapshot, driver, mapper, StartCell, EndCell);
            if (candidates.Count == 0)
                return commandRouter != null ? commandRouter.ClearSelection() : RtsCommandResult.Ok("Lasso found no actors.");

            var ids = new List<int>();
            if (additive && driver != null)
                for (var i = 0; i < driver.SelectedActorIds.Count; i++)
                    ids.Add(driver.SelectedActorIds[i]);

            for (var i = 0; i < candidates.Count; i++)
                if (!ids.Contains(candidates[i].ActorId))
                    ids.Add(candidates[i].ActorId);

            return driver.SetSelectedActorIds(ids);
        }

        public void CancelLasso()
        {
            IsActive = false;
            if (boardRenderer != null)
                boardRenderer.ClearPlacementPreview();
        }

        void RefreshPreview()
        {
            if (boardRenderer == null)
                return;

            previewCells.Clear();
            var minX = Mathf.Min(StartCell.X, EndCell.X);
            var maxX = Mathf.Max(StartCell.X, EndCell.X);
            var minY = Mathf.Min(StartCell.Y, EndCell.Y);
            var maxY = Mathf.Max(StartCell.Y, EndCell.Y);
            for (var y = minY; y <= maxY; y++)
                for (var x = minX; x <= maxX; x++)
                    previewCells.Add(new Int2(x, y));

            boardRenderer.SetPlacementPreview(previewCells, true);
        }
    }
}
