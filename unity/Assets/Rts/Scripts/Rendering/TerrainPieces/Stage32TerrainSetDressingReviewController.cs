using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.TerrainPieces
{
    public sealed class Stage32TerrainSetDressingReviewController : MonoBehaviour
    {
        public TerrainPieceLibrary pieceLibrary;
        public Stage32TerrainPieceMaterialLibrary materialLibrary;
        public TerrainSetDressingProfile setDressingProfile;
        public Transform pieceRoot;
        public Transform swatchRoot;
        public Transform gridRoot;
        public bool rebuildOnEnsure = true;
        public bool showLabels = true;
        public float spacing = 1.55f;

        readonly List<GameObject> spawned = new List<GameObject>();

        public int PieceCount { get; private set; }
        public int MaterialSwatchCount { get; private set; }
        public int FootprintReferenceCount { get; private set; }

        public void EnsureReviewScene()
        {
            if (rebuildOnEnsure)
                ClearSpawned();

            EnsureRoots();
            CreateFootprintReference();
            ArrangePieces();
            CreateMaterialSwatches();
        }

        void EnsureRoots()
        {
            if (pieceRoot == null)
                pieceRoot = CreateRoot("Stage32 Terrain Piece Library").transform;
            if (swatchRoot == null)
                swatchRoot = CreateRoot("Stage32 Material Swatches").transform;
            if (gridRoot == null)
                gridRoot = CreateRoot("Stage32 Footprint Reference").transform;
        }

        void ArrangePieces()
        {
            PieceCount = 0;
            if (pieceLibrary == null)
                return;

            var definitions = pieceLibrary.GetDefinitions();
            var categoryRows = new Dictionary<TerrainPieceCategory, int>();
            for (var i = 0; i < definitions.Count; i++)
            {
                var definition = definitions[i];
                if (definition == null || definition.prefab == null)
                    continue;

                int indexInCategory;
                categoryRows.TryGetValue(definition.category, out indexInCategory);
                categoryRows[definition.category] = indexInCategory + 1;

                var categoryIndex = (int)definition.category;
                var column = indexInCategory % 16;
                var row = indexInCategory / 16;
                var position = new Vector3(-12f + column * spacing, 0.10f, 5.6f - categoryIndex * 3.0f - row * 1.45f);

                var instance = Instantiate(definition.prefab, pieceRoot, false);
                instance.name = "Stage32 Review " + definition.pieceId;
                instance.transform.localPosition = position;
                instance.transform.localRotation = Quaternion.Euler(0f, definition.supportsRotation ? 25f : 0f, 0f);
                spawned.Add(instance);
                RemoveColliders(instance);
                PieceCount++;

                if (showLabels && column == 0)
                    CreateLabel(pieceRoot, CategoryLabel(definition.category), position + new Vector3(-1.15f, 0.42f, 0.05f), 0.04f, TextAnchor.MiddleRight);
            }
        }

        void CreateMaterialSwatches()
        {
            MaterialSwatchCount = 0;
            if (materialLibrary == null || materialLibrary.profiles == null)
                return;

            for (var i = 0; i < materialLibrary.profiles.Count; i++)
            {
                var profile = materialLibrary.profiles[i];
                if (profile == null || profile.material == null)
                    continue;

                var swatch = GameObject.CreatePrimitive(PrimitiveType.Cube);
                swatch.name = "Stage32 Material Swatch " + profile.profileId;
                swatch.transform.SetParent(swatchRoot, false);
                swatch.transform.localPosition = new Vector3(-11.6f + (i % 12) * 1.0f, 0.12f, -11.8f - (i / 12) * 0.95f);
                swatch.transform.localScale = new Vector3(0.78f, 0.11f, 0.78f);
                var renderer = swatch.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.sharedMaterial = profile.material;
                RemoveCollider(swatch);
                spawned.Add(swatch);
                MaterialSwatchCount++;
            }
        }

        void CreateFootprintReference()
        {
            FootprintReferenceCount = 0;
            for (var x = 0; x <= 12; x++)
                CreateLine("Fine Footprint X " + x, new Vector3(-5f + x * 0.5f, 0.04f, 9.2f), new Vector3(-5f + x * 0.5f, 0.04f, 12.2f));
            for (var z = 0; z <= 6; z++)
                CreateLine("Fine Footprint Z " + z, new Vector3(-5f, 0.045f, 9.2f + z * 0.5f), new Vector3(1f, 0.045f, 9.2f + z * 0.5f));

            CreateLabel(gridRoot, "Fine placement footprint reference", new Vector3(-2f, 0.30f, 8.72f), 0.055f, TextAnchor.MiddleCenter);
        }

        void CreateLine(string lineName, Vector3 start, Vector3 end)
        {
            var obj = new GameObject(lineName);
            obj.transform.SetParent(gridRoot, false);
            var line = obj.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            line.widthMultiplier = 0.010f;
            spawned.Add(obj);
            FootprintReferenceCount++;
        }

        GameObject CreateRoot(string rootName)
        {
            var root = new GameObject(rootName);
            root.transform.SetParent(transform, false);
            spawned.Add(root);
            return root;
        }

        void CreateLabel(Transform parent, string text, Vector3 localPosition, float size, TextAnchor anchor)
        {
            var obj = new GameObject("Stage32 Label " + text);
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = localPosition;
            var mesh = obj.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.fontSize = 32;
            mesh.characterSize = size;
            mesh.anchor = anchor;
            mesh.alignment = TextAlignment.Center;
            spawned.Add(obj);
        }

        void ClearSpawned()
        {
            for (var i = spawned.Count - 1; i >= 0; i--)
                DestroyUnityObject(spawned[i]);
            spawned.Clear();
            pieceRoot = null;
            swatchRoot = null;
            gridRoot = null;
            PieceCount = 0;
            MaterialSwatchCount = 0;
            FootprintReferenceCount = 0;
        }

        static string CategoryLabel(TerrainPieceCategory category)
        {
            if (category == TerrainPieceCategory.BaseConstruction)
                return "Base";
            return category.ToString();
        }

        static void RemoveColliders(GameObject target)
        {
            var colliders = target.GetComponentsInChildren<Collider>(true);
            for (var i = colliders.Length - 1; i >= 0; i--)
                DestroyUnityObject(colliders[i]);
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
