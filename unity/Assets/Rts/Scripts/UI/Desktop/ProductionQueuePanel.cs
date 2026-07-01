using System.Collections.Generic;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.UI.Desktop
{
    public sealed class ProductionQueuePanel : MonoBehaviour
    {
        sealed class Row
        {
            public int QueueItemId;
            public Text Label;
            public Slider Progress;
            public Button CancelButton;
        }

        readonly List<Row> rows = new List<Row>();
        RtsSimulationDriver driver;
        DesktopUiCommandRouter router;
        string lastKey = string.Empty;

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
            RtsUiFactory.Stretch(gameObject, Vector2.zero, Vector2.zero);
            if (GetComponent<VerticalLayoutGroup>() == null)
            {
                var layout = gameObject.AddComponent<VerticalLayoutGroup>();
                layout.spacing = 4f;
                layout.childForceExpandHeight = false;
                layout.childControlHeight = false;
            }
        }

        void Refresh()
        {
            if (driver == null)
                return;

            var player = driver.GetLocalPlayerSnapshot();
            var key = BuildKey(player);
            if (key != lastKey)
            {
                lastKey = key;
                RebuildRows(player);
            }

            if (player == null)
                return;

            for (var i = 0; i < rows.Count; i++)
            {
                var item = Find(player, rows[i].QueueItemId);
                if (item == null)
                    continue;

                rows[i].Label.text = DisplayType(item.TypeId) + "  " + DisplayState(item) + "  " + item.ProgressTicks + "/" + item.BuildTimeTicks;
                rows[i].Progress.value = item.BuildTimeTicks <= 0 ? 0f : Mathf.Clamp01(item.ProgressTicks / (float)item.BuildTimeTicks);
            }
        }

        void RebuildRows(PlayerSnapshot player)
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);
            rows.Clear();

            RtsUiFactory.CreateText(transform, "Queue Title", "Production Queue", 14, Color.white, TextAnchor.MiddleLeft).rectTransform.sizeDelta = new Vector2(340f, 22f);

            if (player == null || player.Production.Count == 0)
            {
                RtsUiFactory.CreateText(transform, "Queue Empty", "No active production.", 12, new Color(0.72f, 0.78f, 0.83f, 1f), TextAnchor.MiddleLeft).rectTransform.sizeDelta = new Vector2(340f, 24f);
                return;
            }

            for (var i = 0; i < player.Production.Count; i++)
            {
                var item = player.Production[i];
                var rowObject = new GameObject("Queue " + item.QueueItemId);
                rowObject.transform.SetParent(transform, false);
                var layout = rowObject.AddComponent<HorizontalLayoutGroup>();
                layout.spacing = 4f;
                layout.childForceExpandWidth = false;
                rowObject.AddComponent<RectTransform>().sizeDelta = new Vector2(340f, 30f);

                var label = RtsUiFactory.CreateText(rowObject.transform, "Label", DisplayType(item.TypeId), 11, Color.white, TextAnchor.MiddleLeft);
                label.rectTransform.sizeDelta = new Vector2(168f, 28f);
                var progress = RtsUiFactory.CreateProgressBar(rowObject.transform, "Progress");
                progress.GetComponent<RectTransform>().sizeDelta = new Vector2(92f, 14f);
                var cancel = RtsUiFactory.CreateButton(rowObject.transform, "Cancel", "X");
                cancel.GetComponent<RectTransform>().sizeDelta = new Vector2(32f, 24f);
                var captured = item.QueueItemId;
                cancel.onClick.AddListener(() =>
                {
                    if (router != null)
                        router.CancelProduction(captured);
                });

                rows.Add(new Row { QueueItemId = item.QueueItemId, Label = label, Progress = progress, CancelButton = cancel });
            }
        }

        string BuildKey(PlayerSnapshot player)
        {
            if (player == null)
                return "none";

            var key = player.Production.Count.ToString();
            for (var i = 0; i < player.Production.Count; i++)
                key += "|" + player.Production[i].QueueItemId + ":" + player.Production[i].TypeId + ":" + player.Production[i].State;
            return key;
        }

        ProductionSnapshot Find(PlayerSnapshot player, int queueItemId)
        {
            for (var i = 0; i < player.Production.Count; i++)
                if (player.Production[i].QueueItemId == queueItemId)
                    return player.Production[i];
            return null;
        }

        static string DisplayState(ProductionSnapshot item)
        {
            if (item == null)
                return string.Empty;
            if (item.State == "CompletedPendingPlacement")
                return "Ready: place on grid";
            return item.State;
        }

        static string DisplayType(string typeId)
        {
            return string.IsNullOrEmpty(typeId) ? "item" : typeId.Replace("_", " ");
        }
    }
}
