using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Art;
using ProjectAegisRTS.UnityClient.Art.Production;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Visuals
{
    public sealed class Stage29BattlefieldVisualReviewController : MonoBehaviour
    {
        public BattlefieldMaterialLibrary materialLibrary;
        public TerrainMaterialProfileLibrary terrainMaterialProfileLibrary;
        public ActorVisualDefinitionLibrary actorVisualDefinitionLibrary;
        public ProductionVisualStandardLibrary productionVisualStandardLibrary;
        public Transform terrainRoot;
        public Transform proxyRoot;
        public Transform swatchRoot;
        public bool rebuildOnEnsure = true;
        public bool showLabels = true;
        public float tileSize = 0.64f;

        readonly List<GameObject> spawned = new List<GameObject>();

        public int TerrainTileCount { get; private set; }
        public int ActorProxyCount { get; private set; }
        public int MaterialSwatchCount { get; private set; }
        public int FineGridLineCount { get; private set; }

        public void EnsureReviewScene()
        {
            EnsureReferences();
            if (rebuildOnEnsure)
                ClearSpawned();

            EnsureRoots();
            CreateTerrainBoard();
            CreateMaterialSwatches();
            CreateMvpProxyRow();
        }

        void EnsureReferences()
        {
            if (materialLibrary == null)
                materialLibrary = FindFirstObjectByType<BattlefieldMaterialLibrary>();
            if (materialLibrary == null)
                materialLibrary = gameObject.AddComponent<BattlefieldMaterialLibrary>();
            materialLibrary.EnsureRuntimeDefaults();

            if (terrainMaterialProfileLibrary == null)
                terrainMaterialProfileLibrary = FindFirstObjectByType<TerrainMaterialProfileLibrary>();
            if (terrainMaterialProfileLibrary == null)
                terrainMaterialProfileLibrary = gameObject.AddComponent<TerrainMaterialProfileLibrary>();
            terrainMaterialProfileLibrary.materialLibrary = materialLibrary;
            terrainMaterialProfileLibrary.RebuildLookup();

            if (actorVisualDefinitionLibrary == null)
                actorVisualDefinitionLibrary = FindFirstObjectByType<ActorVisualDefinitionLibrary>();
            if (actorVisualDefinitionLibrary != null)
                actorVisualDefinitionLibrary.EnsureInitialized();

            if (productionVisualStandardLibrary == null)
                productionVisualStandardLibrary = FindFirstObjectByType<ProductionVisualStandardLibrary>();
            if (productionVisualStandardLibrary != null)
                productionVisualStandardLibrary.EnsureInitialized();
        }

        void EnsureRoots()
        {
            if (terrainRoot == null)
                terrainRoot = CreateRoot("Stage29 Terrain Review").transform;
            if (proxyRoot == null)
                proxyRoot = CreateRoot("Stage29 MVP Proxy Review").transform;
            if (swatchRoot == null)
                swatchRoot = CreateRoot("Stage29 Material Swatches").transform;
        }

        GameObject CreateRoot(string rootName)
        {
            var root = new GameObject(rootName);
            root.transform.SetParent(transform, false);
            spawned.Add(root);
            return root;
        }

        void CreateTerrainBoard()
        {
            TerrainTileCount = 0;
            FineGridLineCount = 0;
            const int width = 24;
            const int height = 16;
            var origin = new Vector3(-width * 0.5f * tileSize, 0f, -height * 0.5f * tileSize);

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var kind = TerrainKindFor(x, y, width, height);
                    var tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    tile.name = "Stage29 Terrain " + kind + " " + x + " " + y;
                    tile.transform.SetParent(terrainRoot, false);
                    tile.transform.localPosition = origin + new Vector3((x + 0.5f) * tileSize, 0f, (y + 0.5f) * tileSize);
                    tile.transform.localScale = new Vector3(tileSize * 0.98f, 0.035f, tileSize * 0.98f);
                    SetRendererMaterial(tile, materialLibrary.MaterialForTerrainKind(kind));
                    RemoveCollider(tile);
                    spawned.Add(tile);
                    TerrainTileCount++;

                    if (kind == "ResourceField")
                        CreateResourceShard(tile.transform, x, y);
                    else if (kind == "RockBlocked")
                        CreateRockMarker(tile.transform, x, y);
                    else if (kind == "ConcretePad" && (x + y) % 3 == 0)
                        CreatePadLine(tile.transform);
                }
            }

            CreateBoardFineGrid(width, height, origin);
            CreateLabel(terrainRoot, "Stage 29 terrain / placement board", new Vector3(0f, 0.12f, -height * 0.5f * tileSize - 0.62f), 0.07f);
        }

        string TerrainKindFor(int x, int y, int width, int height)
        {
            if (x >= 18 && y >= 1 && y <= 5)
                return "Water";
            if (x >= 18 && y >= 10)
                return "FogExplored";
            if (x >= 14 && x <= 17 && y >= 10 && y <= 13)
                return "ResourceField";
            if (x >= 2 && x <= 6 && y >= 10 && y <= 13)
                return "RockBlocked";
            if (y == 7 || (x >= 8 && x <= 12 && y >= 5 && y <= 6))
                return "RoadPath";
            if (x >= 4 && x <= 13 && y >= 2 && y <= 6)
                return "CompactedBase";
            if ((x >= 5 && x <= 8 && y >= 3 && y <= 5) || (x >= 10 && x <= 13 && y >= 3 && y <= 5))
                return "ConcretePad";
            return "GrassDirt";
        }

        void CreateBoardFineGrid(int width, int height, Vector3 origin)
        {
            var gridRoot = new GameObject("Stage29 Fine Placement Grid Lines");
            gridRoot.transform.SetParent(terrainRoot, false);
            spawned.Add(gridRoot);

            for (var x = 0; x <= width * 2; x++)
            {
                var localX = origin.x + x * tileSize * 0.5f;
                CreateLine(gridRoot.transform, "Fine Grid X " + x, new Vector3(localX, 0.052f, origin.z), new Vector3(localX, 0.052f, origin.z + height * tileSize));
            }
            for (var z = 0; z <= height * 2; z++)
            {
                var localZ = origin.z + z * tileSize * 0.5f;
                CreateLine(gridRoot.transform, "Fine Grid Z " + z, new Vector3(origin.x, 0.054f, localZ), new Vector3(origin.x + width * tileSize, 0.054f, localZ));
            }
        }

        void CreateLine(Transform parent, string lineName, Vector3 start, Vector3 end)
        {
            var obj = new GameObject(lineName);
            obj.transform.SetParent(parent, false);
            var line = obj.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            line.widthMultiplier = 0.008f;
            line.sharedMaterial = materialLibrary.fineGridGuide;
            spawned.Add(obj);
            FineGridLineCount++;
        }

        void CreateMaterialSwatches()
        {
            MaterialSwatchCount = 0;
            var profiles = terrainMaterialProfileLibrary.GetProfiles();
            for (var i = 0; i < profiles.Count; i++)
            {
                var profile = profiles[i];
                if (profile == null)
                    continue;
                var swatch = GameObject.CreatePrimitive(PrimitiveType.Cube);
                swatch.name = "Stage29 Swatch " + profile.terrainKind;
                swatch.transform.SetParent(swatchRoot, false);
                swatch.transform.localPosition = new Vector3(-7.8f + i * 1.05f, 0.12f, 6.15f);
                swatch.transform.localScale = new Vector3(0.86f, 0.16f, 0.86f);
                SetRendererMaterial(swatch, profile.material);
                RemoveCollider(swatch);
                spawned.Add(swatch);
                MaterialSwatchCount++;
                if (showLabels)
                    CreateLabel(swatch.transform, profile.displayName, new Vector3(0f, 0.42f, 0f), 0.035f);
            }
        }

        void CreateMvpProxyRow()
        {
            ActorProxyCount = 0;
            if (actorVisualDefinitionLibrary == null)
                return;

            for (var i = 0; i < Stage20MvpVisualActorSet.ActorTypeIds.Length; i++)
            {
                var actorTypeId = Stage20MvpVisualActorSet.ActorTypeIds[i];
                var definition = actorVisualDefinitionLibrary.GetDefinition(actorTypeId);
                var prefab = definition == null ? null : definition.GetBestPrefab();
                if (prefab == null)
                    continue;

                var instance = Instantiate(prefab);
                instance.name = "Stage29 Review " + actorTypeId;
                instance.transform.SetParent(proxyRoot, false);
                var col = i % 5;
                var row = i / 5;
                instance.transform.localPosition = new Vector3(-5.8f + col * 2.75f, 0.12f, -6.15f - row * 2.65f);
                instance.transform.localRotation = Quaternion.identity;
                spawned.Add(instance);
                ActorProxyCount++;

                CreateProxyFootprint(instance.transform, definition);
                if (showLabels)
                    CreateLabel(instance.transform, actorTypeId.Replace('_', ' '), new Vector3(0f, 1.75f, -0.75f), 0.045f);
            }
        }

        void CreateProxyFootprint(Transform parent, ActorVisualDefinition definition)
        {
            var width = Mathf.Max(1, definition == null ? 1 : definition.footprintWidth);
            var height = Mathf.Max(1, definition == null ? 1 : definition.footprintHeight);
            var footprint = GameObject.CreatePrimitive(PrimitiveType.Cube);
            footprint.name = "Stage29 Fine Grid Grounding Footprint";
            footprint.transform.SetParent(parent, false);
            footprint.transform.localPosition = new Vector3(0f, -0.028f, 0f);
            footprint.transform.localScale = new Vector3(width * 1.02f, 0.026f, height * 1.02f);
            SetRendererMaterial(footprint, materialLibrary.foundationEdge);
            RemoveCollider(footprint);
            spawned.Add(footprint);
        }

        void CreateResourceShard(Transform parent, int x, int y)
        {
            if ((x + y) % 2 != 0)
                return;
            var shard = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shard.name = "Stage29 Resource Shard";
            shard.transform.SetParent(parent, false);
            shard.transform.localPosition = new Vector3(0f, 0.38f, 0f);
            shard.transform.localScale = new Vector3(0.14f, 0.36f, 0.14f);
            SetRendererMaterial(shard, materialLibrary.warmLight);
            RemoveCollider(shard);
            spawned.Add(shard);
        }

        void CreateRockMarker(Transform parent, int x, int y)
        {
            if ((x + y) % 2 != 0)
                return;
            var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rock.name = "Stage29 Blocked Rock Marker";
            rock.transform.SetParent(parent, false);
            rock.transform.localPosition = new Vector3(0f, 0.18f, 0f);
            rock.transform.localScale = new Vector3(0.34f, 0.18f, 0.28f);
            SetRendererMaterial(rock, materialLibrary.rockBlocked);
            RemoveCollider(rock);
            spawned.Add(rock);
        }

        void CreatePadLine(Transform parent)
        {
            var stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stripe.name = "Stage29 Concrete Pad Expansion Joint";
            stripe.transform.SetParent(parent, false);
            stripe.transform.localPosition = new Vector3(0f, 0.61f, 0f);
            stripe.transform.localScale = new Vector3(0.92f, 0.035f, 0.045f);
            SetRendererMaterial(stripe, materialLibrary.foundationEdge);
            RemoveCollider(stripe);
            spawned.Add(stripe);
        }

        void CreateLabel(Transform parent, string text, Vector3 localPosition, float size)
        {
            var obj = new GameObject("Stage29 Label " + text);
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = localPosition;
            var mesh = obj.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.fontSize = 30;
            mesh.characterSize = size;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            spawned.Add(obj);
        }

        static void SetRendererMaterial(GameObject target, Material material)
        {
            var renderer = target.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = material;
        }

        static void RemoveCollider(GameObject target)
        {
            var collider = target.GetComponent<Collider>();
            if (collider != null)
                DestroyUnityObject(collider);
        }

        void ClearSpawned()
        {
            for (var i = spawned.Count - 1; i >= 0; i--)
                DestroyUnityObject(spawned[i]);
            spawned.Clear();

            TerrainTileCount = 0;
            ActorProxyCount = 0;
            MaterialSwatchCount = 0;
            FineGridLineCount = 0;
            terrainRoot = null;
            proxyRoot = null;
            swatchRoot = null;
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
