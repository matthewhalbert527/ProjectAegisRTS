#if UNITY_EDITOR
using System;
using System.IO;
using ProjectAegisRTS.UnityClient.Art.Production;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class ProjectAegisHarvesterPreviewCaptureTool
    {
        const string PrefabPath = "Assets/Rts/Art/Prefabs/Actors/Production/Units/harvester/harvester.prefab";
        const string OutputRelativePath = "build/screenshots/units/harvester_unity_review.png";
        const string FramesRelativeFolder = "build/animations/harvester_motion_frames";
        const string ContactSheetRelativePath = "build/animations/harvester_motion_contact_sheet.png";
        const int Width = 1280;
        const int Height = 820;
        const int FrameCount = 48;
        const int FrameWidth = 960;
        const int FrameHeight = 640;

        [MenuItem("ProjectAegisRTS/Art/Capture Harvester Preview")]
        public static void CaptureHarvesterPreviewMenu()
        {
            CaptureHarvesterPreviewBatch();
        }

        public static void CaptureHarvesterPreviewBatch()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
                throw new InvalidOperationException("Missing harvester prefab: " + PrefabPath);

            CreateLighting();
            CreateGround();
            var camera = CreateCamera();
            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
                throw new InvalidOperationException("Could not instantiate harvester prefab.");

            instance.name = "harvester_unity_preview";
            instance.transform.position = Vector3.zero;
            instance.transform.rotation = Quaternion.Euler(0f, 35f, 0f);

            var rig = instance.GetComponent<ProjectAegisHarvesterVisualRig>();
            if (rig != null)
            {
                rig.SetCargoFill(0.72f);
                rig.ApplyTeamColor(new Color(0.12f, 0.70f, 0.88f, 1f));
                if (rig.machineGunRoot != null)
                    rig.machineGunRoot.localRotation = Quaternion.Euler(0f, 11f, 0f);
            }

            var outputPath = FullPath(OutputRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            RenderCameraToPng(camera, outputPath, Width, Height);
            AssetDatabase.Refresh();
            Debug.Log("Harvester preview captured: " + outputPath);
        }

        [MenuItem("ProjectAegisRTS/Art/Capture Harvester Motion Preview")]
        public static void CaptureHarvesterMotionMenu()
        {
            CaptureHarvesterMotionBatch();
        }

        public static void CaptureHarvesterMotionBatch()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
                throw new InvalidOperationException("Missing harvester prefab: " + PrefabPath);

            CreateLighting();
            CreateGround();
            var camera = CreateMotionCamera();
            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
                throw new InvalidOperationException("Could not instantiate harvester prefab.");

            instance.name = "harvester_motion_preview";
            var harvesterRig = instance.GetComponent<ProjectAegisHarvesterVisualRig>();
            var tankRig = instance.GetComponent<TankVisualRigController>();
            if (harvesterRig != null)
            {
                harvesterRig.SetCargoFill(0.72f);
                harvesterRig.ApplyTeamColor(new Color(0.12f, 0.70f, 0.88f, 1f));
            }

            if (tankRig != null)
                tankRig.estimateMotionFromTransform = false;

            var outputFolder = FullPath(FramesRelativeFolder);
            if (Directory.Exists(outputFolder))
                Directory.Delete(outputFolder, true);
            Directory.CreateDirectory(outputFolder);

            var framePaths = new string[FrameCount];
            const float dt = 1f / 12f;
            var previousPosition = new Vector3(-2.15f, 0f, -0.20f);
            instance.transform.position = previousPosition;
            instance.transform.rotation = Quaternion.Euler(0f, 58f, 0f);

            for (var frame = 0; frame < FrameCount; frame++)
            {
                var phase = frame / (float)(FrameCount - 1);
                var position = new Vector3(
                    Mathf.Lerp(-2.15f, 2.15f, phase),
                    0f,
                    Mathf.Sin(phase * Mathf.PI * 2f) * 0.28f);
                var velocity = (position - previousPosition) / dt;
                instance.transform.position = position;
                if (velocity.sqrMagnitude > 0.0001f)
                    instance.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(velocity, Vector3.up).normalized, Vector3.up);

                if (tankRig != null)
                    tankRig.ApplyVisualMotion(velocity, dt);

                if (harvesterRig != null)
                {
                    if (harvesterRig.cutterDrumRoot != null)
                        harvesterRig.cutterDrumRoot.localRotation = Quaternion.Euler(frame * 58f, 0f, 0f);
                    if (harvesterRig.machineGunRoot != null)
                    {
                        var sweep = Mathf.Sin(phase * Mathf.PI * 2f) * 14f;
                        harvesterRig.machineGunRoot.localRotation = Quaternion.Euler(0f, sweep, 0f);
                    }
                    harvesterRig.SetCargoFill(Mathf.Lerp(0.35f, 0.92f, Mathf.PingPong(phase * 1.2f, 1f)));
                }

                var path = Path.Combine(outputFolder, "frame_" + frame.ToString("000") + ".png").Replace('\\', '/');
                RenderCameraToPng(camera, path, FrameWidth, FrameHeight);
                framePaths[frame] = path;
                previousPosition = position;
            }

            CreateContactSheet(framePaths, FullPath(ContactSheetRelativePath));
            AssetDatabase.Refresh();
            Debug.Log("Harvester motion frames captured: " + outputFolder);
            Debug.Log("Harvester motion contact sheet captured: " + FullPath(ContactSheetRelativePath));
        }

        static void CreateGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "harvester preview ground";
            ground.transform.position = new Vector3(0f, -0.07f, 0f);
            ground.transform.localScale = new Vector3(11.5f, 0.05f, 9.0f);

            var renderer = ground.GetComponent<Renderer>();
            if (renderer == null)
                return;

            var material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            material.name = "harvester preview dark terrain";
            material.color = new Color(0.17f, 0.23f, 0.19f, 1f);
            renderer.sharedMaterial = material;
        }

        static void CreateLighting()
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.34f, 0.38f, 0.36f, 1f);

            var key = new GameObject("harvester preview key light");
            var keyLight = key.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.intensity = 1.30f;
            keyLight.color = new Color(1f, 0.94f, 0.82f, 1f);
            key.transform.rotation = Quaternion.Euler(48f, -28f, 0f);

            var fill = new GameObject("harvester preview fill light");
            var fillLight = fill.AddComponent<Light>();
            fillLight.type = LightType.Directional;
            fillLight.intensity = 0.35f;
            fillLight.color = new Color(0.62f, 0.74f, 0.95f, 1f);
            fill.transform.rotation = Quaternion.Euler(62f, 150f, 0f);
        }

        static Camera CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 6.5f, -8.2f);
            cameraObject.transform.LookAt(new Vector3(0f, 1.0f, 0f));

            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.25f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.055f, 0.070f, 0.066f, 1f);
            cameraObject.AddComponent<AudioListener>();
            return camera;
        }

        static Camera CreateMotionCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 6.2f, -7.7f);
            cameraObject.transform.LookAt(new Vector3(0f, 0.85f, 0f));

            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.15f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.055f, 0.070f, 0.066f, 1f);
            cameraObject.AddComponent<AudioListener>();
            return camera;
        }

        static string FullPath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "../..", relativePath)).Replace('\\', '/');
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

        static void CreateContactSheet(string[] framePaths, string outputPath)
        {
            const int columns = 6;
            const int rows = 2;
            const int tileWidth = 480;
            const int tileHeight = 320;
            var sheet = new Texture2D(tileWidth * columns, tileHeight * rows, TextureFormat.RGBA32, false);
            Fill(sheet, new Color32(8, 13, 12, 255));

            for (var i = 0; i < columns * rows; i++)
            {
                var frameIndex = Mathf.RoundToInt(i * (FrameCount - 1) / (float)(columns * rows - 1));
                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                texture.LoadImage(File.ReadAllBytes(framePaths[frameIndex]));
                var scaled = Resize(texture, tileWidth, tileHeight);
                var x = (i % columns) * tileWidth;
                var y = (rows - 1 - i / columns) * tileHeight;
                sheet.SetPixels32(x, y, tileWidth, tileHeight, scaled.GetPixels32());
                UnityEngine.Object.DestroyImmediate(texture);
                UnityEngine.Object.DestroyImmediate(scaled);
            }

            sheet.Apply();
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            File.WriteAllBytes(outputPath, sheet.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(sheet);
        }

        static void Fill(Texture2D texture, Color32 color)
        {
            var pixels = new Color32[texture.width * texture.height];
            for (var i = 0; i < pixels.Length; i++)
                pixels[i] = color;
            texture.SetPixels32(pixels);
        }

        static Texture2D Resize(Texture2D source, int width, int height)
        {
            var previousActive = RenderTexture.active;
            var renderTexture = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
            try
            {
                Graphics.Blit(source, renderTexture);
                RenderTexture.active = renderTexture;
                var result = new Texture2D(width, height, TextureFormat.RGBA32, false);
                result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                result.Apply();
                return result;
            }
            finally
            {
                RenderTexture.active = previousActive;
                RenderTexture.ReleaseTemporary(renderTexture);
            }
        }
    }
}
#endif
