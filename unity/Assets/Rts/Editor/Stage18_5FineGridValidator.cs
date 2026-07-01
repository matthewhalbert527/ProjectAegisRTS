using System;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Data;
using ProjectAegisRTS.Demo;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage18_5FineGridValidator
    {
        public static void ValidateStage18_5FineGridBatch()
        {
            try
            {
                ValidateStage18_5FineGrid();
                if (Application.isBatchMode)
                    EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                if (Application.isBatchMode)
                    EditorApplication.Exit(1);
                throw;
            }
        }

        public static void ValidateStage18_5FineGrid()
        {
            Stage18SceneValidator.ValidateStage18Scene();
            ValidateCoreFineGridContract();
            ValidateUnitySceneFineGridContract();
            Debug.Log("Stage 18.5 fine placement grid validation passed.");
        }

        static void ValidateCoreFineGridContract()
        {
            var rules = DemoRules.CreateDefaultRules();
            var powerPlant = (BuildingDefinition)rules.GetDefinition("power_plant");
            var warFactory = (BuildingDefinition)rules.GetDefinition("war_factory");
            if (!powerPlant.FootprintCells.Equals(new Int2(2, 2)) || !powerPlant.PlacementFootprintCells.Equals(new Int2(4, 4)))
                throw new InvalidOperationException("Stage 18.5 expected power plant 2x2 coarse / 4x4 fine footprint.");
            if (!warFactory.FootprintCells.Equals(new Int2(3, 2)) || !warFactory.PlacementFootprintCells.Equals(new Int2(6, 4)))
                throw new InvalidOperationException("Stage 18.5 expected war factory 3x2 coarse / 6x4 fine footprint.");

            var world = DemoWorldFactory.CreateMvpWorld();
            var snapshot = world.CreateSnapshot();
            if (snapshot.Map.PlacementGridScale != PlacementGridMetrics.PlacementGridScale || snapshot.Map.PlacementWidth != snapshot.Map.Width * 2 || snapshot.Map.PlacementHeight != snapshot.Map.Height * 2)
                throw new InvalidOperationException("Stage 18.5 map snapshot does not expose doubled placement dimensions.");

            var halfOffsetPreview = world.PreviewPlacement(1, "power_plant", new Int2(15, 8));
            if (!halfOffsetPreview.CanPlace || halfOffsetPreview.PlacementGridScale != 2 || halfOffsetPreview.PlacementFootprintCells.X != 4 || halfOffsetPreview.FootprintCells.Count != 16)
                throw new InvalidOperationException("Stage 18.5 half-cell power plant preview failed.");

            var overlapPreview = world.PreviewPlacement(1, "power_plant", new Int2(11, 8));
            if (overlapPreview.CanPlace || overlapPreview.ErrorCode != "OccupiedCell")
                throw new InvalidOperationException("Stage 18.5 expected fine-grid overlap rejection.");
        }

        static void ValidateUnitySceneFineGridContract()
        {
            var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 18.5 Stage16 scene did not open.");

            var bootstrapper = Require<RtsGameBootstrapper>("RtsGameBootstrapper");
            var mapper = Require<BoardCoordinateMapper>("BoardCoordinateMapper");
            Require<BoardRenderer>("BoardRenderer");

            mapper.Configure(bootstrapper.boardWidth, bootstrapper.boardHeight, bootstrapper.boardCellSizeMeters, bootstrapper.boardRoot);
            if (mapper.PlacementGridScale != 2)
                throw new InvalidOperationException("Stage 18.5 mapper placement grid scale must be 2.");
            if (mapper.PlacementBoardWidth != bootstrapper.boardWidth * 2 || mapper.PlacementBoardHeight != bootstrapper.boardHeight * 2)
                throw new InvalidOperationException("Stage 18.5 mapper placement board dimensions are not doubled.");
            if (Mathf.Abs(mapper.PlacementCellSizeMeters - bootstrapper.boardCellSizeMeters * 0.5f) > 0.001f)
                throw new InvalidOperationException("Stage 18.5 mapper fine cells must be half the coarse cell size.");
        }

        static T Require<T>(string label) where T : Component
        {
            var active = UnityEngine.Object.FindFirstObjectByType<T>();
            if (active != null)
                return active;

            var all = Resources.FindObjectsOfTypeAll<T>();
            for (var i = 0; i < all.Length; i++)
                if (all[i] != null && all[i].gameObject != null && all[i].gameObject.scene.IsValid())
                    return all[i];

            throw new InvalidOperationException("Missing component: " + label);
        }
    }
}
