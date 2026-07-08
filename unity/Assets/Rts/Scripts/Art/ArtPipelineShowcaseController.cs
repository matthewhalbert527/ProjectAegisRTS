using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Art
{
    public sealed class ArtPipelineShowcaseController : MonoBehaviour
    {
        public ActorVisualDefinitionLibrary definitionLibrary;
        public ConceptArtReferenceLibrary conceptLibrary;
        public Transform showcaseRoot;
        public bool showConceptCards = true;
        public bool showSocketLabels;
        public float spacing = 3.2f;
        public int columns = 9;

        readonly List<GameObject> spawned = new List<GameObject>();
        int selectedIndex;

        public int DefinitionCount { get; private set; }
        public int ConceptReferenceCount { get; private set; }
        public int BlockoutPrefabCount { get; private set; }
        public int MissingIconCount { get; private set; }
        public int MissingPrefabCount { get; private set; }
        public int IpReviewCount { get; private set; }
        public ActorVisualDefinition SelectedDefinition { get; private set; }

        public void EnsureShowcase()
        {
            EnsureReferences();
            ClearSpawned();

            var definitions = definitionLibrary == null ? null : definitionLibrary.GetAllDefinitions();
            if (definitions == null)
                return;

            DefinitionCount = 0;
            ConceptReferenceCount = conceptLibrary == null ? 0 : conceptLibrary.ReferenceCount;
            BlockoutPrefabCount = 0;
            MissingIconCount = 0;
            MissingPrefabCount = 0;
            IpReviewCount = 0;

            if (showcaseRoot == null)
            {
                var root = new GameObject("Stage8 Generated Blockout Grid");
                root.transform.SetParent(transform, false);
                showcaseRoot = root.transform;
            }

            for (var i = 0; i < definitions.Count; i++)
            {
                var definition = definitions[i];
                if (definition == null)
                    continue;

                DefinitionCount++;
                if (definition.icon == null)
                    MissingIconCount++;
                if (definition.ipReviewRequired)
                    IpReviewCount++;
                if (definition.generatedBlockoutPrefab != null)
                    BlockoutPrefabCount++;

                var prefab = definition.GetBestPrefab();
                if (prefab == null)
                {
                    MissingPrefabCount++;
                    continue;
                }

                var instance = Instantiate(prefab);
                instance.name = "Stage8 Showcase " + definition.actorTypeId;
                instance.transform.SetParent(showcaseRoot, false);
                var col = i % Mathf.Max(1, columns);
                var row = i / Mathf.Max(1, columns);
                instance.transform.localPosition = new Vector3(2f + col * spacing, 0f, 18f + row * spacing);
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one * Mathf.Max(0.1f, definition.visualScale);
                spawned.Add(instance);

                ConceptArtReference concept = null;
                if (conceptLibrary != null)
                    conceptLibrary.TryGetReference(definition.actorTypeId, out concept);
                var card = instance.GetComponent<ConceptArtCardView>();
                if (card == null)
                    card = instance.AddComponent<ConceptArtCardView>();
                card.Configure(concept, definition, showConceptCards);

                ConfigureSocketLabels(instance, definition);
            }

            ClampSelectedIndex();
        }

        public void ToggleConceptCards()
        {
            showConceptCards = !showConceptCards;
            for (var i = 0; i < spawned.Count; i++)
            {
                var card = spawned[i] == null ? null : spawned[i].GetComponent<ConceptArtCardView>();
                if (card != null)
                    card.SetVisible(showConceptCards);
            }
        }

        public void ToggleSocketLabels()
        {
            showSocketLabels = !showSocketLabels;
            EnsureShowcase();
        }

        public ActorVisualDefinition CycleSelected(int direction)
        {
            EnsureReferences();
            var definitions = definitionLibrary == null ? null : definitionLibrary.GetAllDefinitions();
            if (definitions == null || definitions.Count == 0)
                return null;
            selectedIndex += direction;
            ClampSelectedIndex();
            SelectedDefinition = definitions[selectedIndex];
            return SelectedDefinition;
        }

        public GameObject SpawnSelectedPreview(Vector3 localPosition)
        {
            var definition = SelectedDefinition;
            if (definition == null)
                definition = CycleSelected(0);
            if (definition == null)
                return null;

            var prefab = definition.GetBestPrefab();
            if (prefab == null)
                return null;

            var instance = Instantiate(prefab);
            instance.name = "Stage8 Selected Preview " + definition.actorTypeId;
            instance.transform.SetParent(showcaseRoot == null ? transform : showcaseRoot, false);
            instance.transform.localPosition = localPosition;
            spawned.Add(instance);
            return instance;
        }

        void EnsureReferences()
        {
            if (definitionLibrary == null)
                definitionLibrary = Object.FindFirstObjectByType<ActorVisualDefinitionLibrary>();
            if (conceptLibrary == null)
                conceptLibrary = Object.FindFirstObjectByType<ConceptArtReferenceLibrary>();
            if (definitionLibrary != null)
                definitionLibrary.EnsureInitialized();
            if (conceptLibrary != null)
                conceptLibrary.EnsureInitialized();
        }

        void ConfigureSocketLabels(GameObject instance, ActorVisualDefinition definition)
        {
            var descriptor = instance.GetComponentInChildren<ActorPrefabDescriptor>(true);
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
                text.fontSize = 24;
                text.characterSize = 0.035f;
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
            var definitions = definitionLibrary == null ? null : definitionLibrary.GetAllDefinitions();
            if (definitions == null || definitions.Count == 0)
            {
                selectedIndex = 0;
                SelectedDefinition = null;
                return;
            }

            if (selectedIndex < 0)
                selectedIndex = definitions.Count - 1;
            if (selectedIndex >= definitions.Count)
                selectedIndex = 0;
            SelectedDefinition = definitions[selectedIndex];
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
