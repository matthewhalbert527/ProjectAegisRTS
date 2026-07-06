using System;
using System.IO;
using ProjectAegisRTS.UnityClient.Rendering.Buildings;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class UnityAiBuildingAnimationCaptureTool
    {
        const string FramesRelativeFolder = "build/animations/unity_ai_building_slate_frames";
        const string DoorCloseupFramesRelativeFolder = "build/animations/unity_ai_building_door_closeup_frames";
        const string PowerSpinnerFramesRelativeFolder = "build/animations/unity_ai_building_power_spinner_frames";
        const string ContactSheetRelativePath = "build/animations/unity_ai_building_animation_contact_sheet.png";
        const int FrameCount = 36;
        const int FrameWidth = 960;
        const int FrameHeight = 640;

        [MenuItem("ProjectAegisRTS/Unity AI/Capture Building Animation Preview")]
        public static void CaptureBuildingAnimationPreviewMenu()
        {
            var folder = CaptureBuildingAnimationPreview();
            Debug.Log("Unity AI building animation preview captured: " + folder);
        }

        public static void CaptureBuildingAnimationPreviewBatch()
        {
            CaptureBuildingAnimationPreviewMenu();
        }

        [MenuItem("ProjectAegisRTS/Unity AI/Capture Building Door Animation Closeup")]
        public static void CaptureDoorAnimationCloseupMenu()
        {
            var folder = CaptureDoorAnimationCloseup();
            Debug.Log("Unity AI building door animation closeup captured: " + folder);
        }

        public static void CaptureDoorAnimationCloseupBatch()
        {
            CaptureDoorAnimationCloseupMenu();
        }

        [MenuItem("ProjectAegisRTS/Unity AI/Capture Building Power Spinner Closeup")]
        public static void CapturePowerSpinnerCloseupMenu()
        {
            var folder = CapturePowerSpinnerCloseup();
            Debug.Log("Unity AI building power spinner closeup captured: " + folder);
        }

        public static void CapturePowerSpinnerCloseupBatch()
        {
            CapturePowerSpinnerCloseupMenu();
        }

        static string CaptureBuildingAnimationPreview()
        {
            var scene = EditorSceneManager.OpenScene(UnityAiBuildingSlateGenerator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Could not open Unity AI building slate scene.");

            var camera = UnityEngine.Object.FindFirstObjectByType<Camera>();
            if (camera == null)
                throw new InvalidOperationException("Unity AI building slate scene has no camera.");

            var showcases = UnityEngine.Object.FindObjectsByType<BuildingArtShowcaseController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (showcases == null || showcases.Length == 0)
                throw new InvalidOperationException("Unity AI building slate scene has no showcase controllers to animate.");

            var states = new BuildingVisualStateController[showcases.Length];
            for (var i = 0; i < showcases.Length; i++)
            {
                var showcase = showcases[i];
                var visualState = showcase.GetComponentInChildren<BuildingVisualStateController>(true);
                if (visualState == null)
                    visualState = showcase.gameObject.AddComponent<BuildingVisualStateController>();

                visualState.standaloneTickInPlayMode = false;
                visualState.VisualDebugEnabled = false;
                visualState.Initialize(0, showcase.actorTypeId, showcase.profile);
                states[i] = visualState;
            }

            var outputFolder = FullPath(FramesRelativeFolder);
            if (Directory.Exists(outputFolder))
                Directory.Delete(outputFolder, true);
            Directory.CreateDirectory(outputFolder);

            var framePaths = new string[FrameCount];
            const float dt = 1f / 12f;
            for (var frame = 0; frame < FrameCount; frame++)
            {
                var phase = frame / (float)(FrameCount - 1);
                var producing = phase < 0.64f;
                for (var i = 0; i < states.Length; i++)
                {
                    if (states[i] == null)
                        continue;

                    states[i].SetDebugForcedState(producing ? BuildingAnimationVisualState.Producing : BuildingAnimationVisualState.PoweredIdle);
                    states[i].TickVisual(dt);
                }

                var path = Path.Combine(outputFolder, "frame_" + frame.ToString("000") + ".png").Replace('\\', '/');
                RenderCameraToPng(camera, path, FrameWidth, FrameHeight);
                framePaths[frame] = path;
            }

            CreateContactSheet(framePaths, FullPath(ContactSheetRelativePath));
            AssetDatabase.Refresh();
            return outputFolder;
        }

        static string CaptureDoorAnimationCloseup()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCloseupLighting();
            var camera = CreateCloseupCamera();
            CreateCloseupGround();

            var states = new[]
            {
                InstantiateCloseupBuilding("barracks", new Vector3(-3.8f, 0f, 0.2f), "Barracks personnel door"),
                InstantiateCloseupBuilding("field_hospital", new Vector3(-1.35f, 0f, 0.2f), "Field Hospital clinic door"),
                InstantiateCloseupBuilding("war_factory", new Vector3(1.25f, 0f, 0f), "War Factory clear vehicle bay"),
                InstantiateCloseupBuilding("repair_bay", new Vector3(3.85f, 0f, 0.2f), "Repair Bay arms")
            };

            var outputFolder = FullPath(DoorCloseupFramesRelativeFolder);
            if (Directory.Exists(outputFolder))
                Directory.Delete(outputFolder, true);
            Directory.CreateDirectory(outputFolder);

            const int frameCount = 56;
            const float dt = 1f / 12f;
            for (var frame = 0; frame < frameCount; frame++)
            {
                var producing = frame >= 8 && frame < 40;
                for (var i = 0; i < states.Length; i++)
                {
                    if (states[i] == null)
                        continue;

                    states[i].SetDebugForcedState(producing ? BuildingAnimationVisualState.Producing : BuildingAnimationVisualState.PoweredIdle);
                    states[i].TickVisual(dt);
                }

                var path = Path.Combine(outputFolder, "frame_" + frame.ToString("000") + ".png").Replace('\\', '/');
                RenderCameraToPng(camera, path, 1280, 720);
            }

            AssetDatabase.Refresh();
            return outputFolder;
        }

        static string CapturePowerSpinnerCloseup()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCloseupLighting();
            var camera = CreateCloseupCamera();
            camera.orthographicSize = 2.35f;
            CreateCloseupGround();

            var states = new[]
            {
                InstantiateCloseupBuilding("power_plant", new Vector3(-1.55f, 0f, 0.1f), "Power Plant spinning top blade"),
                InstantiateCloseupBuilding("advanced_power_plant", new Vector3(1.55f, 0f, 0.1f), "Advanced Plant dual spinners")
            };

            var outputFolder = FullPath(PowerSpinnerFramesRelativeFolder);
            if (Directory.Exists(outputFolder))
                Directory.Delete(outputFolder, true);
            Directory.CreateDirectory(outputFolder);

            const int frameCount = 56;
            const float dt = 1f / 12f;
            for (var frame = 0; frame < frameCount; frame++)
            {
                for (var i = 0; i < states.Length; i++)
                {
                    if (states[i] == null)
                        continue;

                    states[i].SetDebugForcedState(BuildingAnimationVisualState.PoweredIdle);
                    states[i].TickVisual(dt);
                }

                var path = Path.Combine(outputFolder, "frame_" + frame.ToString("000") + ".png").Replace('\\', '/');
                RenderCameraToPng(camera, path, 1280, 720);
            }

            AssetDatabase.Refresh();
            return outputFolder;
        }

        static BuildingVisualStateController InstantiateCloseupBuilding(string actorTypeId, Vector3 position, string label)
        {
            var prefabPath = "Assets/Rts/Art/UnityAIBuildingSlate/Prefabs/" + actorTypeId + "_unity_ai_building.prefab";
            var profilePath = "Assets/Rts/ScriptableObjects/BuildingProfiles/" + actorTypeId + "_building_visual.asset";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var profile = AssetDatabase.LoadAssetAtPath<BuildingVisualProfile>(profilePath);
            if (prefab == null)
                throw new InvalidOperationException("Missing building slate prefab: " + prefabPath);
            if (profile == null)
                throw new InvalidOperationException("Missing building visual profile: " + profilePath);

            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
                throw new InvalidOperationException("Could not instantiate building slate prefab: " + prefabPath);

            instance.name = label;
            instance.transform.position = position;
            instance.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            var visualState = instance.GetComponentInChildren<BuildingVisualStateController>(true);
            if (visualState == null)
                visualState = instance.AddComponent<BuildingVisualStateController>();
            visualState.standaloneTickInPlayMode = false;
            visualState.VisualDebugEnabled = false;
            visualState.Initialize(0, actorTypeId, profile);

            var textObject = new GameObject("label " + actorTypeId);
            textObject.transform.position = position + new Vector3(0f, 0.04f, -1.65f);
            textObject.transform.rotation = Quaternion.Euler(65f, 0f, 0f);
            var text = textObject.AddComponent<TextMesh>();
            text.text = label;
            text.fontSize = 28;
            text.characterSize = 0.048f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = new Color(0.80f, 0.86f, 0.82f, 1f);

            return visualState;
        }

        static void CreateCloseupGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "animation closeup ground";
            ground.transform.position = new Vector3(0f, -0.06f, 0f);
            ground.transform.localScale = new Vector3(10.8f, 0.05f, 4.5f);
            var renderer = ground.GetComponent<Renderer>();
            if (renderer != null)
            {
                var material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Unlit/Color"));
                material.name = "animation closeup heather ground";
                material.color = new Color(0.19f, 0.27f, 0.22f, 1f);
                renderer.sharedMaterial = material;
            }
        }

        static void CreateCloseupLighting()
        {
            var key = new GameObject("closeup warm key");
            var keyLight = key.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.intensity = 1.4f;
            keyLight.color = new Color(1f, 0.94f, 0.82f, 1f);
            key.transform.rotation = Quaternion.Euler(48f, -28f, 0f);

            var fill = new GameObject("closeup cool fill");
            var fillLight = fill.AddComponent<Light>();
            fillLight.type = LightType.Directional;
            fillLight.intensity = 0.45f;
            fillLight.color = new Color(0.58f, 0.72f, 0.92f, 1f);
            fill.transform.rotation = Quaternion.Euler(62f, 140f, 0f);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.34f, 0.38f, 0.36f, 1f);
        }

        static Camera CreateCloseupCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 5.8f, -6.0f);
            cameraObject.transform.LookAt(new Vector3(0f, 0.22f, 0.15f));
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 2.95f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.045f, 0.057f, 0.054f, 1f);
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
