#if UNITY_EDITOR
using System;
using System.IO;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class AegisMapPreviewCaptureTool
    {
        public static string CapturePreview(GameObject root, AegisVisualMapDocument document, string fileName, int width, int height, float orthographicScale)
        {
            return CapturePreview(root, document, fileName, width, height, orthographicScale, Path.Combine(Path.GetTempPath(), "ProjectAegisRTS", "VisualCompilerPreviews"));
        }

        public static string CapturePreview(GameObject root, AegisVisualMapDocument document, string fileName, int width, int height, float orthographicScale, string outputDir)
        {
            if (root == null)
                throw new ArgumentNullException("root");
            if (document == null)
                throw new ArgumentNullException("document");
            if (string.IsNullOrEmpty(outputDir))
                outputDir = Path.Combine(Path.GetTempPath(), "ProjectAegisRTS", "VisualCompilerPreviews");

            RenderSettings.ambientLight = new Color(0.34f, 0.36f, 0.34f, 1f);
            var light = new GameObject("Aegis Visual Compiler Preview Sun");
            var lightComponent = light.AddComponent<Light>();
            lightComponent.type = LightType.Directional;
            lightComponent.intensity = 1.1f;
            lightComponent.shadows = LightShadows.Soft;
            light.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

            var cameraObject = new GameObject("Aegis Visual Compiler Preview Camera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.06f, 0.08f, 0.07f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = Mathf.Max(document.width, document.height) * orthographicScale;
            var pitch = 64f;
            var cameraHeight = 105f;
            var centerX = document.width * 0.5f;
            var centerZ = document.height * 0.5f;
            camera.transform.position = new Vector3(centerX, cameraHeight, centerZ - cameraHeight / Mathf.Tan(pitch * Mathf.Deg2Rad));
            camera.transform.rotation = Quaternion.Euler(pitch, 0f, 0f);

            var renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var previous = RenderTexture.active;
            camera.targetTexture = renderTexture;
            camera.Render();
            RenderTexture.active = renderTexture;

            var image = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
            image.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            image.Apply(false, false);
            EnsureNonBlank(image);

            Directory.CreateDirectory(outputDir);
            var outputPath = Path.Combine(outputDir, string.IsNullOrEmpty(fileName) ? "aegis_visual_compiler_preview.png" : fileName);
            File.WriteAllBytes(outputPath, image.EncodeToPNG());

            RenderTexture.active = previous;
            camera.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(image);
            UnityEngine.Object.DestroyImmediate(renderTexture);
            UnityEngine.Object.DestroyImmediate(cameraObject);
            UnityEngine.Object.DestroyImmediate(light);
            Debug.Log("Aegis visual compiler captured preview: " + outputPath);
            return outputPath;
        }

        static void EnsureNonBlank(Texture2D image)
        {
            var pixels = image.GetPixels32();
            if (pixels.Length == 0)
                throw new InvalidOperationException("Visual compiler preview produced an empty image.");

            var first = pixels[0];
            var varied = 0;
            var step = Math.Max(1, pixels.Length / 4096);
            for (var i = 0; i < pixels.Length; i += step)
            {
                var p = pixels[i];
                if (Math.Abs(p.r - first.r) + Math.Abs(p.g - first.g) + Math.Abs(p.b - first.b) > 12)
                    varied++;
                if (varied > 48)
                    return;
            }

            throw new InvalidOperationException("Visual compiler preview looked blank or single-color.");
        }
    }
}
#endif
