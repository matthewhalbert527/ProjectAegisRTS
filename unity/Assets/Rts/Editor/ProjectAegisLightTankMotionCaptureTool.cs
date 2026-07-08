using System;
using System.IO;
using ProjectAegisRTS.UnityClient.Art.Production;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class ProjectAegisLightTankMotionCaptureTool
    {
        const string PrefabPath = "Assets/Rts/Art/Prefabs/Actors/Production/Units/light_tank/light_tank.prefab";
        const string FramesRelativeFolder = "build/animations/light_tank_motion_frames";
        const string ContactSheetRelativePath = "build/animations/light_tank_motion_contact_sheet.png";
        const int FrameCount = 48;
        const int FrameWidth = 960;
        const int FrameHeight = 640;

        readonly struct TankCaptureSpec
        {
            public TankCaptureSpec(string unitId, string prefabPath, float cameraSize)
            {
                UnitId = unitId;
                PrefabPath = prefabPath;
                CameraSize = cameraSize;
            }

            public string UnitId { get; }
            public string PrefabPath { get; }
            public float CameraSize { get; }
            public string FramesFolder => "build/animations/" + UnitId + "_motion_frames";
            public string ContactSheetPath => "build/animations/" + UnitId + "_motion_contact_sheet.png";
        }

        static readonly TankCaptureSpec[] AllTankSpecs =
        {
            new TankCaptureSpec("light_tank", PrefabPath, 4.05f),
            new TankCaptureSpec("medium_tank", "Assets/Rts/Art/Prefabs/Actors/Production/Units/medium_tank/medium_tank.prefab", 4.45f),
            new TankCaptureSpec("heavy_tank", "Assets/Rts/Art/Prefabs/Actors/Production/Units/heavy_tank/heavy_tank.prefab", 5.60f),
        };

        [MenuItem("ProjectAegisRTS/Art/Capture Light Tank Motion Preview")]
        public static void CaptureLightTankMotionMenu()
        {
            var folder = CaptureLightTankMotion();
            Debug.Log("Light tank motion capture completed: " + folder);
        }

        public static void CaptureLightTankMotionBatch()
        {
            CaptureLightTankMotionMenu();
        }

        [MenuItem("ProjectAegisRTS/Art/Capture All Tank Motion Previews")]
        public static void CaptureAllTankMotionMenu()
        {
            foreach (var spec in AllTankSpecs)
            {
                var folder = CaptureTankMotion(spec);
                Debug.Log(spec.UnitId + " motion capture completed: " + folder);
            }
        }

        public static void CaptureAllTankMotionBatch()
        {
            CaptureAllTankMotionMenu();
        }

        static string CaptureLightTankMotion()
        {
            return CaptureTankMotion(AllTankSpecs[0]);
        }

        static string CaptureTankMotion(TankCaptureSpec spec)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(spec.PrefabPath);
            if (prefab == null)
                throw new InvalidOperationException("Missing tank prefab: " + spec.PrefabPath);

            CreateLighting();
            CreateGround();
            var camera = CreateCamera(spec.CameraSize);

            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
                throw new InvalidOperationException("Could not instantiate tank prefab.");

            instance.name = spec.UnitId + "_motion_preview";
            var rig = instance.GetComponent<TankVisualRigController>();
            if (rig == null)
                throw new InvalidOperationException(spec.UnitId + " prefab is missing TankVisualRigController.");

            rig.driveTurretFromDesiredAim = true;
            rig.estimateMotionFromTransform = false;

            var outputFolder = FullPath(spec.FramesFolder);
            if (Directory.Exists(outputFolder))
                Directory.Delete(outputFolder, true);
            Directory.CreateDirectory(outputFolder);

            var framePaths = new string[FrameCount];
            const float dt = 1f / 12f;
            var previousPosition = new Vector3(-2.10f, 0f, -0.35f);
            instance.transform.position = previousPosition;
            instance.transform.rotation = Quaternion.Euler(0f, 60f, 0f);

            for (var frame = 0; frame < FrameCount; frame++)
            {
                var phase = frame / (float)(FrameCount - 1);
                var position = new Vector3(
                    Mathf.Lerp(-2.10f, 2.10f, phase),
                    0f,
                    Mathf.Sin(phase * Mathf.PI * 2f) * 0.32f);

                var velocity = (position - previousPosition) / dt;
                instance.transform.position = position;
                if (velocity.sqrMagnitude > 0.0001f)
                    instance.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(velocity, Vector3.up).normalized, Vector3.up);

                var aimYaw = Mathf.Sin(phase * Mathf.PI * 2f) * 60f;
                rig.SetAimDirection(Quaternion.Euler(0f, aimYaw, 0f) * Vector3.forward);
                if (frame == 12 || frame == 31)
                    rig.TriggerRecoil(1f);

                rig.ApplyVisualMotion(velocity, dt);

                var path = Path.Combine(outputFolder, "frame_" + frame.ToString("000") + ".png").Replace('\\', '/');
                RenderCameraToPng(camera, path, FrameWidth, FrameHeight);
                framePaths[frame] = path;
                previousPosition = position;
            }

            CreateContactSheet(framePaths, FullPath(spec.ContactSheetPath));
            AssetDatabase.Refresh();
            return outputFolder;
        }

        static void CreateGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "motion preview ground";
            ground.transform.position = new Vector3(0f, -0.06f, 0f);
            ground.transform.localScale = new Vector3(9.2f, 0.05f, 4.6f);

            var renderer = ground.GetComponent<Renderer>();
            if (renderer == null)
                return;

            var material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            material.name = "motion preview dark grass pad";
            material.color = new Color(0.18f, 0.24f, 0.20f, 1f);
            renderer.sharedMaterial = material;
        }

        static void CreateLighting()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.34f, 0.38f, 0.36f, 1f);

            var key = new GameObject("motion preview key light");
            var keyLight = key.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.intensity = 1.25f;
            keyLight.color = new Color(1f, 0.94f, 0.82f, 1f);
            key.transform.rotation = Quaternion.Euler(48f, -30f, 0f);

            var fill = new GameObject("motion preview fill light");
            var fillLight = fill.AddComponent<Light>();
            fillLight.type = LightType.Directional;
            fillLight.intensity = 0.35f;
            fillLight.color = new Color(0.62f, 0.74f, 0.95f, 1f);
            fill.transform.rotation = Quaternion.Euler(62f, 150f, 0f);
        }

        static Camera CreateCamera(float orthographicSize)
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 5.5f, -6.2f);
            cameraObject.transform.LookAt(new Vector3(0f, 0.4f, 0f));

            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = orthographicSize;
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
