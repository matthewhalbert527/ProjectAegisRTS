using ProjectAegisRTS.Core;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.UI.XR.LeftHand
{
    public sealed class LeftHandPlacementPanel : MonoBehaviour
    {
        public RtsSimulationDriver driver;
        public LeftHandCommandRouter commandRouter;
        public BoardRenderer boardRenderer;
        Text readoutText;

        void Start()
        {
            BuildIfNeeded();
        }

        void Update()
        {
            PlacementPreviewSnapshot preview;
            RefreshPreview(driver != null && driver.HasHoveredCell ? (Int2?)driver.HoveredCell : null, driver != null && driver.TryGetPlacementPreview(out preview) ? preview : null);
        }

        public void RefreshPreview(Int2? hoveredCell, PlacementPreviewSnapshot preview)
        {
            BuildIfNeeded();
            if (driver == null || !driver.HasPlacementMode)
            {
                readoutText.text = "Placement inactive.";
                return;
            }

            if (preview != null && boardRenderer != null)
                boardRenderer.SetPlacementPreview(preview.TypeId, preview.FootprintCells, preview.TopLeftCell, preview.CanPlace, preview.ErrorCode);

            var cellText = hoveredCell.HasValue ? hoveredCell.Value.ToString() : "none";
            var status = preview == null ? "hover board" : (preview.CanPlace ? "valid" : "invalid " + preview.ErrorCode);
            var footprint = preview == null || preview.FootprintCells == null ? 0 : preview.FootprintCells.Count;
            var fineSize = preview == null ? "n/a" : preview.PlacementFootprintCells.X + "x" + preview.PlacementFootprintCells.Y;
            readoutText.text = "Placing: " + driver.PendingPlacementTypeId + "\nFine cell: " + cellText + "\nStatus: " + status + "\nFine footprint: " + fineSize + " (" + footprint + " cells)\nLeft click/Enter confirm, Esc cancel";
        }

        public RtsCommandResult ConfirmPlacement()
        {
            return commandRouter != null ? commandRouter.ConfirmPlacementAtHoveredCell() : RtsCommandResult.Fail("RouterMissing", "Left-hand command router is not available.");
        }

        public RtsCommandResult CancelPlacement()
        {
            return commandRouter != null ? commandRouter.CancelPlacement() : RtsCommandResult.Fail("RouterMissing", "Left-hand command router is not available.");
        }

        void BuildIfNeeded()
        {
            if (readoutText != null)
                return;

            RtsUiFactory.Stretch(gameObject, Vector2.zero, Vector2.zero);
            RtsUiFactory.AddPanel(gameObject, new Color(0.04f, 0.05f, 0.07f, 0.84f));
            readoutText = RtsUiFactory.CreateText(transform, "Placement Readout", "Placement inactive.", 12, Color.white, TextAnchor.UpperLeft);
            readoutText.rectTransform.offsetMin = new Vector2(8f, 8f);
            readoutText.rectTransform.offsetMax = new Vector2(-8f, -8f);
        }
    }
}
