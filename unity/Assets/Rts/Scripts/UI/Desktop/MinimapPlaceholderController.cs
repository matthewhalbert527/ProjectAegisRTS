using System.Collections.Generic;
using ProjectAegisRTS.Data;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.UI.Desktop
{
    public sealed class MinimapPlaceholderController : MonoBehaviour
    {
        readonly List<Image> dots = new List<Image>();
        RtsSimulationDriver driver;
        RectTransform dotRoot;
        Text label;

        public void Initialize(RtsSimulationDriver simulationDriver)
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
            if (dotRoot != null)
                return;

            RtsUiFactory.Stretch(gameObject, Vector2.zero, Vector2.zero);
            RtsUiFactory.AddPanel(gameObject, new Color(0.02f, 0.03f, 0.04f, 0.92f));
            label = RtsUiFactory.CreateText(transform, "Label", "Minimap", 12, Color.white, TextAnchor.UpperLeft);
            label.rectTransform.offsetMin = new Vector2(6f, -22f);
            label.rectTransform.offsetMax = new Vector2(-6f, -4f);

            var root = new GameObject("Actor Dots");
            root.transform.SetParent(transform, false);
            dotRoot = root.AddComponent<RectTransform>();
            dotRoot.anchorMin = Vector2.zero;
            dotRoot.anchorMax = Vector2.one;
            dotRoot.offsetMin = new Vector2(6f, 6f);
            dotRoot.offsetMax = new Vector2(-6f, -24f);
        }

        void Refresh()
        {
            if (driver == null || driver.LatestSnapshot == null || dotRoot == null)
                return;

            var actors = driver.LatestSnapshot.Actors;
            while (dots.Count < actors.Count)
            {
                var dot = new GameObject("Dot");
                dot.transform.SetParent(dotRoot, false);
                var image = dot.AddComponent<Image>();
                image.color = new Color(0.32f, 0.86f, 0.44f, 1f);
                image.rectTransform.sizeDelta = new Vector2(5f, 5f);
                dots.Add(image);
            }

            for (var i = 0; i < dots.Count; i++)
            {
                if (i >= actors.Count)
                {
                    dots[i].gameObject.SetActive(false);
                    continue;
                }

                var actor = actors[i];
                dots[i].gameObject.SetActive(true);
                ActorDefinition definition;
                var isBuilding = driver.TryGetDefinition(actor.TypeId, out definition) && definition is BuildingDefinition;
                dots[i].color = isBuilding ? new Color(0.95f, 0.74f, 0.28f, 1f) : new Color(0.36f, 0.88f, 0.50f, 1f);
                dots[i].rectTransform.sizeDelta = isBuilding ? new Vector2(7f, 7f) : new Vector2(4f, 4f);
                dots[i].rectTransform.anchorMin = Vector2.zero;
                dots[i].rectTransform.anchorMax = Vector2.zero;
                dots[i].rectTransform.anchoredPosition = new Vector2(actor.CellPosition.X / 32f * dotRoot.rect.width, actor.CellPosition.Y / 32f * dotRoot.rect.height);
            }
        }
    }
}
