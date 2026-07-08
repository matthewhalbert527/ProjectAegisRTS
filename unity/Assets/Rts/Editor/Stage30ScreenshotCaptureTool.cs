using System;
using System.IO;
using ProjectAegisRTS.UnityClient.Rendering.Visuals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage30ScreenshotCaptureTool
    {
        public const string ScreenshotRelativePath = "build/screenshots/stage30_visual_readability_qa.png";

        [MenuItem("ProjectAegisRTS/Stage 30/Capture Visual Readability Screenshot")]
        public static void CaptureStage30ScreenshotMenu()
        {
            CaptureStage30Screenshot();
        }

        public static void CaptureStage30ScreenshotBatch()
        {
            try
            {
                CaptureStage30Screenshot();
                Debug.Log("Stage 30 screenshot capture completed. Path: " + Path.Combine(Stage8ActorCatalog.RepoRoot, ScreenshotRelativePath));
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

        public static string CaptureStage30Screenshot()
        {
            if (!File.Exists(Path.Combine(Stage8ActorCatalog.RepoRoot, "unity", Stage30SceneCreator.ScenePath)))
                Stage30SceneCreator.CreateOrUpdateStage30Scene();

            var scene = EditorSceneManager.OpenScene(Stage30SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 30 screenshot scene did not open.");

            var controller = UnityEngine.Object.FindFirstObjectByType<Stage29BattlefieldVisualReviewController>();
            if (controller != null)
                controller.EnsureReviewScene();

            var camera = Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>();
            if (camera == null)
                throw new InvalidOperationException("Stage 30 screenshot requires a camera.");

            var outputPath = Path.Combine(Stage8ActorCatalog.RepoRoot, ScreenshotRelativePath);
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
