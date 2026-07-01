using ProjectAegisRTS.Data;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.UI.Desktop
{
    public sealed class PlacementModePanel : MonoBehaviour
    {
        RtsSimulationDriver driver;
        DesktopUiCommandRouter router;
        Text label;
        Button cancelButton;

        public void Initialize(RtsSimulationDriver simulationDriver, DesktopUiCommandRouter commandRouter)
        {
            driver = simulationDriver;
            router = commandRouter;
            BuildIfNeeded();
        }

        void Update()
        {
            Refresh();
        }

        void BuildIfNeeded()
        {
            if (label != null)
                return;

            RtsUiFactory.Stretch(gameObject, Vector2.zero, Vector2.zero);
            RtsUiFactory.AddPanel(gameObject, new Color(0.10f, 0.12f, 0.14f, 0.85f));
            label = GetOrCreateText("Placement Text", "Placement inactive.", 12, Color.white, TextAnchor.UpperLeft);
            label.rectTransform.offsetMin = new Vector2(8f, 28f);
            label.rectTransform.offsetMax = new Vector2(-8f, -6f);
            cancelButton = GetOrCreateButton("Cancel Placement", "Cancel Placement");
            cancelButton.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);
            cancelButton.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 0f);
            cancelButton.GetComponent<RectTransform>().offsetMin = new Vector2(8f, 4f);
            cancelButton.GetComponent<RectTransform>().offsetMax = new Vector2(-8f, 28f);
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(() =>
            {
                if (router != null)
                    router.CancelPlacement();
            });
        }

        Text GetOrCreateText(string objectName, string text, int fontSize, Color color, TextAnchor anchor)
        {
            var child = transform.Find(objectName);
            var existing = child != null ? child.GetComponent<Text>() : null;
            return existing != null ? existing : RtsUiFactory.CreateText(transform, objectName, text, fontSize, color, anchor);
        }

        Button GetOrCreateButton(string objectName, string text)
        {
            var child = transform.Find(objectName);
            var existing = child != null ? child.GetComponent<Button>() : null;
            return existing != null ? existing : RtsUiFactory.CreateButton(transform, objectName, text);
        }

        void Refresh()
        {
            if (driver == null || label == null)
                return;

            if (!driver.HasPlacementMode)
            {
                label.text = "Placement inactive.\nCompleted buildings will appear here.";
                cancelButton.interactable = false;
                return;
            }

            cancelButton.interactable = true;
            PlacementPreviewSnapshot preview;
            var previewText = "Hover a board cell.";
            if (driver.TryGetPlacementPreview(out preview))
                previewText = preview.CanPlace ? "Valid at " + preview.TopLeftCell : "Invalid: " + preview.ErrorCode;

            ActorDefinition definition;
            var footprint = "n/a";
            if (driver.TryGetDefinition(driver.PendingPlacementTypeId, out definition) && definition is BuildingDefinition)
            {
                var building = (BuildingDefinition)definition;
                footprint = building.FootprintCells.X + "x" + building.FootprintCells.Y;
            }

            label.text = "Placing: " + driver.PendingPlacementTypeId + "\nFootprint: " + footprint + "\n" + previewText + "\nLeft-click board to place.";
        }
    }
}
