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
    public static class ProjectAegisWheeledVehicleMotionCaptureTool
    {
        const int FrameCount = 48;
        const int FrameWidth = 960;
        const int FrameHeight = 640;

        readonly struct CaptureSpec
        {
            public CaptureSpec(string unitId, string prefabPath, float cameraSize, float pathWidth)
            {
                UnitId = unitId;
                PrefabPath = prefabPath;
                CameraSize = cameraSize;
                PathWidth = pathWidth;
            }

            public string UnitId { get; }
            public string PrefabPath { get; }
            public float CameraSize { get; }
            public float PathWidth { get; }
            public string FramesFolder => "build/animations/" + UnitId + "_motion_frames";
            public string ContactSheetPath => "build/animations/" + UnitId + "_motion_contact_sheet.png";
        }

        static readonly CaptureSpec[] Specs =
        {
            new CaptureSpec(
                "scout_rover",
                "Assets/Rts/Art/Prefabs/Actors/Production/Units/scout_rover/scout_rover.prefab",
                4.25f,
                2.20f),
            new CaptureSpec(
                "apc",
                "Assets/Rts/Art/Prefabs/Actors/Production/Units/apc/apc.prefab",
                4.85f,
                2.45f),
        };

        [MenuItem("ProjectAegisRTS/Art/Capture Wheeled Vehicle Motion Previews")]
        public static void CaptureWheeledVehicleMotionMenu()
        {
            CaptureWheeledVehicleMotionBatch();
        }

        public static void CaptureWheeledVehicleMotionBatch()
        {
            foreach (var spec in Specs)
            {
                var folder = Capture(spec);
                Debug.Log(spec.UnitId + " wheeled motion frames captured: " + folder);
            }

            if (Application.isBatchMode)
                EditorApplication.Exit(0);
        }

        static string Capture(CaptureSpec spec)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(spec.PrefabPath);
            if (prefab == null)
                throw new InvalidOperationException("Missing wheeled vehicle prefab: " + spec.PrefabPath);

            CreateLighting();
            CreateGround();
            var camera = CreateCamera(spec.CameraSize);

            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
                throw new InvalidOperationException("Could not instantiate " + spec.UnitId + " prefab.");

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
            var baseRotation = Quaternion.Euler(0f, 180f, 0f);
            var baseForward = baseRotation * Vector3.forward;
            var baseRight = baseRotation * Vector3.right;
            var previousPosition = MotionPoint(-1f / (FrameCount - 1), baseForward, baseRight, spec.PathWidth);
            instance.transform.position = previousPosition;
            instance.transform.rotation = baseRotation;

            for (var frame = 0; frame < FrameCount; frame++)
            {
                var phase = frame / (float)(FrameCount - 1);
                var position = MotionPoint(phase, baseForward, baseRight, spec.PathWidth);

                var velocity = (position - previousPosition) / dt;
                instance.transform.position = position;
                if (velocity.sqrMagnitude > 0.0001f)
                    instance.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(velocity, Vector3.up).normalized, Vector3.up);

                var aimYaw = Mathf.Lerp(-55f, 55f, Mathf.PingPong(phase * 1.7f, 1f));
                rig.SetAimDirection(baseRotation * Quaternion.Euler(0f, aimYaw, 0f) * Vector3.forward);
                if (frame == 10 || frame == 29)
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

        static Vector3 MotionPoint(float phase, Vector3 forward, Vector3 right, float pathWidth)
        {
            var along = Mathf.Lerp(-pathWidth, pathWidth, phase);
            var lateral = Mathf.Sin(phase * Mathf.PI * 2f) * 0.08f;
            return forward * along + right * lateral;
        }

        static void CreateGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "wheeled vehicle preview ground";
            ground.transform.position = new Vector3(0f, -0.06f, 0f);
            ground.transform.localScale = new Vector3(10.4f, 0.05f, 5.2f);

            var renderer = ground.GetComponent<Renderer>();
            if (renderer == null)
                return;

            var material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            material.name = "wheeled vehicle preview dark terrain";
            material.color = new Color(0.16f, 0.22f, 0.18f, 1f);
            renderer.sharedMaterial = material;
        }

        static void CreateLighting()
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.34f, 0.38f, 0.36f, 1f);

            var key = new GameObject("wheeled preview key light");
            var keyLight = key.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.intensity = 1.25f;
            keyLight.color = new Color(1f, 0.94f, 0.82f, 1f);
            key.transform.rotation = Quaternion.Euler(48f, -30f, 0f);

            var fill = new GameObject("wheeled preview fill light");
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
            cameraObject.transform.position = new Vector3(2.6f, 4.9f, -7.4f);
            cameraObject.transform.LookAt(new Vector3(0f, 0.72f, -0.10f));

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
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
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
            var previous = RenderTexture.active;
            var renderTexture = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(source, renderTexture);
            RenderTexture.active = renderTexture;
            var result = new Texture2D(width, height, TextureFormat.RGBA32, false);
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            result.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTexture);
            return result;
        }
    }
}
#endif
