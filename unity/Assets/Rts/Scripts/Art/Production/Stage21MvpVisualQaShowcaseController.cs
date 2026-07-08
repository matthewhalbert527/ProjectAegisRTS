using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Art;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Art.Production
{
    public sealed class Stage21MvpVisualQaShowcaseController : MonoBehaviour
    {
        public ActorVisualDefinitionLibrary definitionLibrary;
        public ProductionVisualStandardLibrary standardLibrary;
        public ArtistModelImportManifest artistModelImportManifest;
        public MvpVisualQaRunner qaRunner;
        public Transform showcaseRoot;
        public bool showLabels = true;
        public bool showSocketMarkers = true;
        public float spacing = 3.6f;
        public int columns = 5;

        readonly List<GameObject> spawned = new List<GameObject>();
        int selectedIndex;

        public IReadOnlyList<MvpVisualQaReport> Reports
        {
            get { return qaRunner == null ? new List<MvpVisualQaReport>() : qaRunner.latestReports; }
        }

        public int DisplayedActorCount { get; private set; }
        public int PassCount { get { return qaRunner == null ? 0 : qaRunner.PassCount; } }
        public int WarningCount { get { return qaRunner == null ? 0 : qaRunner.WarningCount; } }
        public int FailCount { get { return qaRunner == null ? 0 : qaRunner.FailCount; } }
        public MvpVisualQaReport SelectedReport { get; private set; }

        public void EnsureShowcase()
        {
            EnsureReferences();
            ClearSpawned();
            DisplayedActorCount = 0;

            if (qaRunner == null || definitionLibrary == null)
                return;

            qaRunner.RunAll();
            if (showcaseRoot == null)
            {
                var root = new GameObject("Stage21 MVP Visual QA Grid");
                root.transform.SetParent(transform, false);
                showcaseRoot = root.transform;
            }

            var ids = Stage20MvpVisualActorSet.ActorTypeIds;
            for (var i = 0; i < ids.Length; i++)
            {
                var actorTypeId = ids[i];
                var definition = definitionLibrary.GetDefinition(actorTypeId);
                var prefab = definition == null ? null : definition.GetBestPrefab();
                if (prefab == null)
                    continue;

                var instance = Instantiate(prefab);
                instance.name = "Stage21 QA " + actorTypeId;
                instance.transform.SetParent(showcaseRoot, false);
                var col = i % Mathf.Max(1, columns);
                var row = i / Mathf.Max(1, columns);
                instance.transform.localPosition = new Vector3(col * spacing, 0f, row * spacing);
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
                spawned.Add(instance);
                DisplayedActorCount++;

                var report = ReportFor(actorTypeId);
                CreateFootprintGrid(instance.transform, definition);
                if (showLabels)
                    CreateQaLabel(instance.transform, definition, report);
                if (showSocketMarkers)
                    CreateSocketMarkers(instance);
            }

            ClampSelectedIndex();
        }

        public void CycleSelected(int direction)
        {
            selectedIndex += direction;
            ClampSelectedIndex();
        }

        public void ToggleLabels()
        {
            showLabels = !showLabels;
            EnsureShowcase();
        }

        public void ToggleSocketMarkers()
        {
            showSocketMarkers = !showSocketMarkers;
            EnsureShowcase();
        }

        void EnsureReferences()
        {
            if (definitionLibrary == null)
                definitionLibrary = Object.FindFirstObjectByType<ActorVisualDefinitionLibrary>();
            if (standardLibrary == null)
                standardLibrary = Object.FindFirstObjectByType<ProductionVisualStandardLibrary>();
            if (qaRunner == null)
                qaRunner = GetComponent<MvpVisualQaRunner>();
            if (qaRunner == null)
                qaRunner = gameObject.AddComponent<MvpVisualQaRunner>();

            qaRunner.definitionLibrary = definitionLibrary;
            qaRunner.standardLibrary = standardLibrary;
            qaRunner.artistModelImportManifest = artistModelImportManifest;
            if (definitionLibrary != null)
                definitionLibrary.EnsureInitialized();
            if (standardLibrary != null)
                standardLibrary.EnsureInitialized();
        }

        MvpVisualQaReport ReportFor(string actorTypeId)
        {
            var reports = qaRunner == null ? null : qaRunner.latestReports;
            if (reports == null)
                return null;
            for (var i = 0; i < reports.Count; i++)
                if (reports[i] != null && reports[i].actorTypeId == actorTypeId)
                    return reports[i];
            return null;
        }

        void CreateFootprintGrid(Transform parent, ActorVisualDefinition definition)
        {
            if (definition == null)
                return;

            var footprint = GameObject.CreatePrimitive(PrimitiveType.Cube);
            footprint.name = "Stage21 Fine Grid Footprint " + definition.actorTypeId;
            footprint.transform.SetParent(parent, false);
            footprint.transform.localPosition = new Vector3(0f, -0.018f, 0f);
            footprint.transform.localScale = new Vector3(Mathf.Max(0.95f, definition.footprintWidth), 0.018f, Mathf.Max(0.95f, definition.footprintHeight));
            var renderer = footprint.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = RuntimeMaterial("Stage21 QA Footprint", new Color(0.07f, 0.09f, 0.10f, 0.65f));
            RemoveCollider(footprint);

            for (var x = 0; x <= definition.footprintWidth * 2; x++)
                CreateLine(parent, "Fine Grid X " + x, new Vector3(-definition.footprintWidth * 0.5f + x * 0.5f, 0.012f, -definition.footprintHeight * 0.5f), new Vector3(-definition.footprintWidth * 0.5f + x * 0.5f, 0.012f, definition.footprintHeight * 0.5f));
            for (var z = 0; z <= definition.footprintHeight * 2; z++)
                CreateLine(parent, "Fine Grid Z " + z, new Vector3(-definition.footprintWidth * 0.5f, 0.014f, -definition.footprintHeight * 0.5f + z * 0.5f), new Vector3(definition.footprintWidth * 0.5f, 0.014f, -definition.footprintHeight * 0.5f + z * 0.5f));
        }

        void CreateLine(Transform parent, string name, Vector3 start, Vector3 end)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var line = obj.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            line.widthMultiplier = 0.012f;
            line.sharedMaterial = RuntimeMaterial("Stage21 QA Fine Grid Line", new Color(0.25f, 0.45f, 0.48f, 0.55f));
        }

        void CreateQaLabel(Transform parent, ActorVisualDefinition definition, MvpVisualQaReport report)
        {
            var labelObject = new GameObject("Stage21 QA Label " + definition.actorTypeId);
            labelObject.transform.SetParent(parent, false);
            labelObject.transform.localPosition = new Vector3(0f, 1.8f, -0.85f);
            var text = labelObject.AddComponent<TextMesh>();
            text.text = definition.actorTypeId + "\n" +
                "QA: " + (report == null ? "missing" : report.overallStatus.ToString()) + "\n" +
                "Meshes/Materials: " + (report == null ? "?" : report.meshObjectCount + "/" + report.materialCount) + "\n" +
                "Import: " + (report == null ? "?" : report.artistImportStatus);
            text.fontSize = 28;
            text.characterSize = 0.044f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
        }

        void CreateSocketMarkers(GameObject instance)
        {
            var descriptor = instance.GetComponentInChildren<ActorPrefabDescriptor>(true);
            if (descriptor == null)
                return;

            var sockets = descriptor.GetSockets();
            for (var i = 0; i < sockets.Length; i++)
            {
                var socket = sockets[i];
                if (socket == null || socket.socketKind == ActorPrefabSocketKind.Root)
                    continue;

                var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.name = "Stage21 Socket Marker " + socket.socketKind;
                marker.transform.SetParent(socket.transform, false);
                marker.transform.localPosition = Vector3.zero;
                marker.transform.localScale = Vector3.one * 0.08f;
                var renderer = marker.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.sharedMaterial = RuntimeMaterial("Stage21 QA Socket Marker", new Color(0.95f, 0.80f, 0.25f, 1f));
                RemoveCollider(marker);
            }
        }

        void ClampSelectedIndex()
        {
            var reports = qaRunner == null ? null : qaRunner.latestReports;
            if (reports == null || reports.Count == 0)
            {
                selectedIndex = 0;
                SelectedReport = null;
                return;
            }

            if (selectedIndex < 0)
                selectedIndex = reports.Count - 1;
            if (selectedIndex >= reports.Count)
                selectedIndex = 0;
            SelectedReport = reports[selectedIndex];
        }

        void ClearSpawned()
        {
            for (var i = spawned.Count - 1; i >= 0; i--)
                DestroyUnityObject(spawned[i]);
            spawned.Clear();

            if (showcaseRoot != null)
                for (var i = showcaseRoot.childCount - 1; i >= 0; i--)
                    DestroyUnityObject(showcaseRoot.GetChild(i).gameObject);
        }

        static Material RuntimeMaterial(string name, Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");
            var material = new Material(shader);
            material.name = name;
            material.color = color;
            return material;
        }

        static void RemoveCollider(GameObject target)
        {
            var collider = target.GetComponent<Collider>();
            if (collider != null)
                DestroyUnityObject(collider);
        }

        static void DestroyUnityObject(Object target)
        {
            if (target == null)
                return;
            if (Application.isPlaying)
                Destroy(target);
            else
                DestroyImmediate(target);
        }
    }
}
