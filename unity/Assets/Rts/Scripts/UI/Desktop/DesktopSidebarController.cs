using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.UI.Desktop
{
    public sealed class DesktopSidebarController : MonoBehaviour
    {
        RtsSimulationDriver driver;
        Text titleText;
        Text readoutText;

        public void Initialize(
            RtsSimulationDriver simulationDriver,
            DesktopUiCommandRouter router,
            ProductionCategoryTabs tabs,
            ProductionGridController grid,
            ProductionQueuePanel queue,
            PlacementModePanel placement,
            SelectionPanelController selection,
            MinimapPlaceholderController minimap)
        {
            driver = simulationDriver;
            BuildIfNeeded();
        }

        void Update()
        {
            Refresh();
        }

        void BuildIfNeeded()
        {
            if (titleText != null)
                return;

            RtsUiFactory.Stretch(gameObject, Vector2.zero, Vector2.zero);
            RtsUiFactory.AddPanel(gameObject, new Color(0.05f, 0.06f, 0.08f, 0.92f));

            titleText = GetOrCreateText("Title", "ProjectAegisRTS", 18, Color.white, TextAnchor.MiddleLeft);
            titleText.rectTransform.anchorMin = new Vector2(0f, 1f);
            titleText.rectTransform.anchorMax = new Vector2(1f, 1f);
            titleText.rectTransform.offsetMin = new Vector2(14f, -42f);
            titleText.rectTransform.offsetMax = new Vector2(-14f, -8f);

            readoutText = GetOrCreateText("Readout", "Waiting for simulation...", 13, new Color(0.85f, 0.90f, 0.95f, 1f), TextAnchor.UpperLeft);
            readoutText.rectTransform.anchorMin = new Vector2(0f, 1f);
            readoutText.rectTransform.anchorMax = new Vector2(1f, 1f);
            readoutText.rectTransform.offsetMin = new Vector2(14f, -102f);
            readoutText.rectTransform.offsetMax = new Vector2(-14f, -46f);
        }

        Text GetOrCreateText(string objectName, string text, int fontSize, Color color, TextAnchor anchor)
        {
            var child = transform.Find(objectName);
            var existing = child != null ? child.GetComponent<Text>() : null;
            return existing != null ? existing : RtsUiFactory.CreateText(transform, objectName, text, fontSize, color, anchor);
        }

        void Refresh()
        {
            if (driver == null || driver.LatestSnapshot == null || readoutText == null)
                return;

            var snapshot = driver.LatestSnapshot;
            var player = driver.GetLocalPlayerSnapshot();
            if (player == null)
            {
                readoutText.text = "No local player.";
                return;
            }

            readoutText.text =
                "Credits: " + player.Credits + "\n" +
                "Power: " + player.Power.Generated + " / " + player.Power.Consumed + "  " + player.Power.State + "\n" +
                "Actors: " + snapshot.Actors.Count + "    Tick: " + snapshot.Tick + "\n" +
                "Mode: " + driver.CommandMode;
        }
    }
}
