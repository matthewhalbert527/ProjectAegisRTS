using System.Collections.Generic;
using ProjectAegisRTS.Data;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.UI.Desktop
{
    public sealed class SelectionPanelController : MonoBehaviour
    {
        RtsSimulationDriver driver;
        DesktopUiCommandRouter router;
        Text label;
        Button stopButton;
        Button moveButton;
        Button powerButton;

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
            label = GetOrCreateText(transform, "Selection Text", "No selection.", 12, Color.white, TextAnchor.UpperLeft);
            label.rectTransform.offsetMin = new Vector2(8f, 40f);
            label.rectTransform.offsetMax = new Vector2(-8f, -6f);

            var rowTransform = transform.Find("Selection Commands");
            var row = rowTransform != null ? rowTransform.gameObject : new GameObject("Selection Commands");
            if (rowTransform == null)
                row.transform.SetParent(transform, false);
            var rect = row.GetComponent<RectTransform>();
            if (rect == null)
                rect = row.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.offsetMin = new Vector2(8f, 6f);
            rect.offsetMax = new Vector2(-8f, 34f);
            var layout = row.GetComponent<HorizontalLayoutGroup>();
            if (layout == null && row.GetComponent<LayoutGroup>() == null)
                layout = row.AddComponent<HorizontalLayoutGroup>();
            if (layout != null)
                layout.spacing = 5f;

            stopButton = GetOrCreateButton(row.transform, "Stop", "Stop");
            moveButton = GetOrCreateButton(row.transform, "Move", "Move");
            powerButton = GetOrCreateButton(row.transform, "Power Toggle", "Power");
            stopButton.onClick.RemoveAllListeners();
            moveButton.onClick.RemoveAllListeners();
            powerButton.onClick.RemoveAllListeners();
            stopButton.onClick.AddListener(() => { if (router != null) router.StopSelected(); });
            moveButton.onClick.AddListener(() => { if (router != null) router.SetMoveMode(); });
            powerButton.onClick.AddListener(() => { if (router != null) router.TogglePowerSelected(); });
        }

        Text GetOrCreateText(Transform parent, string objectName, string text, int fontSize, Color color, TextAnchor anchor)
        {
            var child = parent.Find(objectName);
            var existing = child != null ? child.GetComponent<Text>() : null;
            return existing != null ? existing : RtsUiFactory.CreateText(parent, objectName, text, fontSize, color, anchor);
        }

        Button GetOrCreateButton(Transform parent, string objectName, string text)
        {
            var child = parent.Find(objectName);
            var existing = child != null ? child.GetComponent<Button>() : null;
            return existing != null ? existing : RtsUiFactory.CreateButton(parent, objectName, text);
        }

        void Refresh()
        {
            if (driver == null || label == null)
                return;

            var selected = driver.SelectedActorIds;
            stopButton.interactable = selected.Count > 0;
            moveButton.interactable = selected.Count > 0;
            powerButton.interactable = selected.Count == 1;

            if (selected.Count == 0)
            {
                label.text = "No selection.\nLeft-click a unit or building.";
                return;
            }

            if (selected.Count == 1)
            {
                ActorSnapshot actor;
                if (!driver.TryGetActorSnapshot(selected[0], out actor))
                {
                    label.text = "Selected actor is no longer available.";
                    return;
                }

                ActorDefinition definition;
                driver.TryGetDefinition(actor.TypeId, out definition);
                var category = definition == null ? "unknown" : DesktopProductionCatalog.GetCategory(definition).ToString();
                label.text =
                    "Selected: 1\n" +
                    "Actor ID: " + actor.ActorId + "\n" +
                    "Type: " + actor.TypeId + "\n" +
                    "Category: " + category + "\n" +
                    "Owner: " + actor.OwnerId + "\n" +
                    "Health: " + actor.Health + (definition == null ? string.Empty : "/" + definition.MaxHealth) + "\n" +
                    "Cell: " + actor.CellPosition + "\n" +
                    "Powered: " + actor.IsPowered + "  Low: " + actor.IsLowPower + "\n" +
                    "Producing: " + actor.IsProducing + "  Phase: " + actor.MovementPhase;
                return;
            }

            var counts = new Dictionary<string, int>();
            for (var i = 0; i < selected.Count; i++)
            {
                ActorSnapshot actor;
                if (!driver.TryGetActorSnapshot(selected[i], out actor))
                    continue;

                if (!counts.ContainsKey(actor.TypeId))
                    counts[actor.TypeId] = 0;
                counts[actor.TypeId]++;
            }

            var text = "Selected: " + selected.Count;
            foreach (var pair in counts)
                text += "\n" + pair.Key + ": " + pair.Value;
            label.text = text;
        }
    }
}
