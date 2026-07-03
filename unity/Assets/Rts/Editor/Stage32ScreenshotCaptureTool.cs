using System;
using System.IO;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.Rendering.TerrainPieces;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage32ScreenshotCaptureTool
    {
        public const string ReviewScreenshotRelativePath = "build/screenshots/stage32_terrain_set_dressing_review.png";
        public const string PlayerFacingScreenshotRelativePath = "build/screenshots/stage32_player_facing_terrain_view.png";

        [MenuItem("ProjectAegisRTS/Stage 32/Capture Terrain Set Dressing Screenshots")]
        public static void CaptureStage32ScreenshotsMenu()
        {
            CaptureStage32Screenshots();
        }

        public static void CaptureStage32ScreenshotsBatch()
        {
            try
            {
                CaptureStage32Screenshots();
                Debug.Log("Stage 32 screenshot capture completed.");
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

        public static void CaptureStage32Screenshots()
        {
            CaptureReviewScreenshot();
            CapturePlayerFacingScreenshot();
        }

        public static string CaptureReviewScreenshot()
        {
            if (!File.Exists(Path.Combine(Stage8ActorCatalog.RepoRoot, "unity", Stage32SceneCreator.ScenePath)))
                Stage32SceneCreator.CreateOrUpdateStage32Scene();

            var scene = EditorSceneManager.OpenScene(Stage32SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 32 screenshot review scene did not open.");

            var controller = UnityEngine.Object.FindFirstObjectByType<Stage32TerrainSetDressingReviewController>();
            if (controller != null)
                controller.EnsureReviewScene();

            var camera = Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>();
            if (camera == null)
                throw new InvalidOperationException("Stage 32 review screenshot requires a camera.");

            var outputPath = Path.Combine(Stage8ActorCatalog.RepoRoot, ReviewScreenshotRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            RenderCameraToPng(camera, outputPath, 1920, 1080);
            return outputPath;
        }

        public static string CapturePlayerFacingScreenshot()
        {
            Stage16SceneCreator.CreateOrUpdateStage16Scene();
            var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 32 screenshot player-facing scene did not open.");

            var bootstrapper = UnityEngine.Object.FindFirstObjectByType<RtsGameBootstrapper>();
            if (bootstrapper != null)
                bootstrapper.InitializeScene();
            var layer = UnityEngine.Object.FindFirstObjectByType<TerrainSetDressingRuntimeLayer>();
            if (layer != null)
                layer.EnsureInitialized();

            var camera = Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>();
            if (camera == null)
                throw new InvalidOperationException("Stage 32 player-facing screenshot requires a camera.");

            var outputPath = Path.Combine(Stage8ActorCatalog.RepoRoot, PlayerFacingScreenshotRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            RenderCameraToPng(camera, outputPath, 1920, 1080);
            return outputPath;
        }

        static void RenderCameraToPng(Camera camera, string outputPath, int width, int height)
        {
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            var renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            try
            {
                camera.targetTexture = renderTexture;
                RenderTexture.active = renderTexture;
                camera.Render();
                var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture.Apply();
                File.WriteAllBytes(outputPath, texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                renderTexture.Release();
                UnityEngine.Object.DestroyImmediate(renderTexture);
            }
        }
    }
}
