using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.TerrainPieces
{
    public sealed class TerrainSetDressingRenderer : MonoBehaviour
    {
        public TerrainPieceRuntimeResolver resolver;
        public TerrainSetDressingProfile profile;
        public Transform boardRoot;
        public Transform visualRoot;
        public bool renderOnStart = true;
        public bool rebuildOnRender = true;
        public int hardInstanceLimit = 64;

        readonly List<GameObject> spawned = new List<GameObject>();

        public int RenderedPieceCount { get; private set; }
        public int ResourceDecorationCount { get; private set; }
        public int BaseDecorationCount { get; private set; }
        public int ObstacleDecorationCount { get; private set; }
        public bool LastRenderWasVisualOnly { get; private set; }

        void Start()
        {
            if (renderOnStart)
                RenderProfile();
        }

        public void RenderProfile()
        {
            EnsureReferences();
            if (resolver == null || profile == null || profile.placements == null)
                return;

            if (rebuildOnRender)
                ClearSpawned();

            var limit = Mathf.Min(hardInstanceLimit, Mathf.Max(1, profile.maxRenderedPieces));
            RenderedPieceCount = 0;
            ResourceDecorationCount = 0;
            BaseDecorationCount = 0;
            ObstacleDecorationCount = 0;
            LastRenderWasVisualOnly = profile.visualOnlyNeverGameplayAuthority;

            for (var i = 0; i < profile.placements.Count && RenderedPieceCount < limit; i++)
            {
                var placement = profile.placements[i];
                if (placement == null || string.IsNullOrEmpty(placement.pieceId))
                    continue;

                var definition = resolver.ResolvePiece(placement.pieceId);
                if (definition == null)
                    continue;

                var instance = resolver.InstantiatePiece(placement.pieceId, visualRoot);
                if (instance == null)
                    continue;

                instance.transform.localPosition = placement.localPosition;
                instance.transform.localRotation = Quaternion.Euler(0f, definition.supportsRotation ? placement.rotationY : 0f, 0f);
                var scale = Mathf.Clamp(placement.uniformScale, 0.55f, 1.55f);
                instance.transform.localScale = Vector3.one * scale;
                if (definition.supportsTint)
                    resolver.ApplyTint(instance, placement.tint);

                spawned.Add(instance);
                RenderedPieceCount++;
                if (definition.category == TerrainPieceCategory.Resource)
                    ResourceDecorationCount++;
                else if (definition.category == TerrainPieceCategory.BaseConstruction)
                    BaseDecorationCount++;
                else if (definition.category == TerrainPieceCategory.Obstacle)
                    ObstacleDecorationCount++;
            }
        }

        public void ClearSpawned()
        {
            for (var i = spawned.Count - 1; i >= 0; i--)
                DestroyUnityObject(spawned[i]);
            spawned.Clear();
            RenderedPieceCount = 0;
            ResourceDecorationCount = 0;
            BaseDecorationCount = 0;
            ObstacleDecorationCount = 0;
        }

        void EnsureReferences()
        {
            if (resolver == null)
                resolver = GetComponent<TerrainPieceRuntimeResolver>();
            if (resolver == null)
                resolver = gameObject.AddComponent<TerrainPieceRuntimeResolver>();

            if (boardRoot == null)
            {
                var board = GameObject.Find("BoardRoot");
                boardRoot = board != null ? board.transform : transform;
            }

            if (visualRoot == null)
            {
                var existing = boardRoot.Find("Stage32 Terrain Set Dressing");
                if (existing != null)
                    visualRoot = existing;
                else
                {
                    var root = new GameObject("Stage32 Terrain Set Dressing");
                    root.transform.SetParent(boardRoot, false);
                    visualRoot = root.transform;
                }
            }
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
