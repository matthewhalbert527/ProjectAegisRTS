using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Art;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Art.Production
{
    public sealed class Stage20MvpVisualShowcaseController : MonoBehaviour
    {
        public ActorVisualDefinitionLibrary definitionLibrary;
        public ProductionVisualStandardLibrary standardLibrary;
        public Transform showcaseRoot;
        public bool showLabels = true;
        public bool showSocketLabels;
        public float spacing = 3.4f;
        public int columns = 5;

        readonly List<GameObject> spawned = new List<GameObject>();
        int selectedIndex;

        public int MvpProxyCount { get; private set; }
        public int MissingProxyCount { get; private set; }
        public int SocketValidatedCount { get; private set; }
        public int ViewCoverageValidatedCount { get; private set; }
        public ActorVisualDefinition SelectedDefinition { get; private set; }

        public void EnsureShowcase()
        {
            EnsureReferences();
            ClearSpawned();

            MvpProxyCount = 0;
            MissingProxyCount = 0;
            SocketValidatedCount = 0;
            ViewCoverageValidatedCount = 0;

            if (definitionLibrary == null)
                return;

            if (showcaseRoot == null)
            {
                var root = new GameObject("Stage20 MVP Production Proxy Grid");
                root.transform.SetParent(transform, false);
                showcaseRoot = root.transform;
            }

            var ids = Stage20MvpVisualActorSet.ActorTypeIds;
            for (var i = 0; i < ids.Length; i++)
            {
                var actorTypeId = ids[i];
                var definition = definitionLibrary.GetDefinition(actorTypeId);
                var prefab = definition == null ? null : definition.GetBestPrefab();
                if (definition == null || prefab == null)
                {
                    MissingProxyCount++;
                    continue;
                }

                var tag = prefab.GetComponentInChildren<ProductionVisualValidationTag>(true);
                if (tag == null || tag.visualTier != ProductionVisualTier.FirstPassProxy)
                {
                    MissingProxyCount++;
                    continue;
                }

                MvpProxyCount++;
                var descriptor = prefab.GetComponentInChildren<ActorPrefabDescriptor>(true);
                if (descriptor != null && descriptor.requiredSocketsPresent)
                    SocketValidatedCount++;
                if ((tag.ViewCoverage & ProductionVisualViewCoverage.AllAround) == ProductionVisualViewCoverage.AllAround)
                    ViewCoverageValidatedCount++;

                var instance = Instantiate(prefab);
                instance.name = "Stage20 Showcase " + actorTypeId;
                instance.transform.SetParent(showcaseRoot, false);
                var col = i % Mathf.Max(1, columns);
                var row = i / Mathf.Max(1, columns);
                instance.transform.localPosition = new Vector3(col * spacing, 0f, row * spacing);
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
                spawned.Add(instance);

                var instanceDescriptor = instance.GetComponentInChildren<ActorPrefabDescriptor>(true);
                CreateFootprintPlate(instance.transform, definition);
                if (showLabels)
                    CreateActorLabel(instance.transform, definition, tag, instanceDescriptor);
                ConfigureSocketLabels(instance, instanceDescriptor);
            }

            ClampSelectedIndex();
        }

        public ActorVisualDefinition CycleSelected(int direction)
        {
            selectedIndex += direction;
            ClampSelectedIndex();
            return SelectedDefinition;
        }

        public void ToggleLabels()
        {
            showLabels = !showLabels;
            EnsureShowcase();
        }

        public void ToggleSocketLabels()
        {
            showSocketLabels = !showSocketLabels;
            EnsureShowcase();
        }

        void EnsureReferences()
        {
            if (definitionLibrary == null)
                definitionLibrary = Object.FindFirstObjectByType<ActorVisualDefinitionLibrary>();
            if (standardLibrary == null)
                standardLibrary = Object.FindFirstObjectByType<ProductionVisualStandardLibrary>();
            if (definitionLibrary != null)
                definitionLibrary.EnsureInitialized();
            if (standardLibrary != null)
                standardLibrary.EnsureInitialized();
        }

        void CreateFootprintPlate(Transform parent, ActorVisualDefinition definition)
        {
            var plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plate.name = "Fine Grid Footprint " + definition.actorTypeId;
            plate.transform.SetParent(parent, false);
            plate.transform.localPosition = new Vector3(0f, -0.015f, 0f);
            plate.transform.localScale = new Vector3(Mathf.Max(0.95f, definition.footprintWidth), 0.02f, Mathf.Max(0.95f, definition.footprintHeight));
            var renderer = plate.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = RuntimeMaterial("Stage20 Footprint", new Color(0.08f, 0.10f, 0.11f, 0.55f));
            var collider = plate.GetComponent<Collider>();
            if (collider != null)
                DestroyUnityObject(collider);
        }

        void CreateActorLabel(Transform parent, ActorVisualDefinition definition, ProductionVisualValidationTag tag, ActorPrefabDescriptor descriptor)
        {
            var labelObject = new GameObject("Stage20 Label " + definition.actorTypeId);
            labelObject.transform.SetParent(parent, false);
            labelObject.transform.localPosition = new Vector3(0f, 1.55f, -0.75f);
            var text = labelObject.AddComponent<TextMesh>();
            text.text = definition.actorTypeId + "\n" +
                "Tier: " + tag.visualTier + "\n" +
                "Footprint: " + definition.footprintWidth + "x" + definition.footprintHeight + "\n" +
                "Sockets: " + (descriptor != null && descriptor.requiredSocketsPresent ? "ok" : "check") + "\n" +
                "Coverage: " + tag.ViewCoverage;
            text.fontSize = 28;
            text.characterSize = 0.045f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
        }

        void ConfigureSocketLabels(GameObject instance, ActorPrefabDescriptor descriptor)
        {
            if (descriptor == null)
                return;

            var sockets = descriptor.GetSockets();
            for (var i = 0; i < sockets.Length; i++)
            {
                if (sockets[i] == null || sockets[i].socketKind == ActorPrefabSocketKind.Root)
                    continue;
                var labelObject = new GameObject("Socket Label " + sockets[i].socketKind);
                labelObject.transform.SetParent(sockets[i].transform, false);
                labelObject.transform.localPosition = Vector3.up * 0.12f;
                var text = labelObject.AddComponent<TextMesh>();
                text.text = sockets[i].socketKind.ToString();
                text.fontSize = 20;
                text.characterSize = 0.028f;
                text.anchor = TextAnchor.MiddleCenter;
                text.alignment = TextAlignment.Center;
                labelObject.SetActive(showSocketLabels);
            }
        }

        void ClearSpawned()
        {
            for (var i = spawned.Count - 1; i >= 0; i--)
                if (spawned[i] != null)
                    DestroyUnityObject(spawned[i]);
            spawned.Clear();

            if (showcaseRoot != null)
            {
                for (var i = showcaseRoot.childCount - 1; i >= 0; i--)
                    DestroyUnityObject(showcaseRoot.GetChild(i).gameObject);
            }
        }

        void ClampSelectedIndex()
        {
            var ids = Stage20MvpVisualActorSet.ActorTypeIds;
            if (ids.Length == 0 || definitionLibrary == null)
            {
                selectedIndex = 0;
                SelectedDefinition = null;
                return;
            }

            if (selectedIndex < 0)
                selectedIndex = ids.Length - 1;
            if (selectedIndex >= ids.Length)
                selectedIndex = 0;
            SelectedDefinition = definitionLibrary.GetDefinition(ids[selectedIndex]);
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
