using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Performance
{
    public sealed class SceneComplexityReporter : MonoBehaviour
    {
        public int gameObjectCount;
        public int activeRendererCount;
        public int meshFilterCount;
        public int materialSlotCount;
        public int lightCount;
        public int cameraCount;
        public int canvasCount;
        public int behaviourCount;
        public string summary;

        void Start()
        {
            Refresh();
        }

        public void Refresh()
        {
            var transforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var renderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var meshFilters = Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var lights = Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            gameObjectCount = transforms.Length;
            activeRendererCount = 0;
            materialSlotCount = 0;
            for (var i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].enabled && renderers[i].gameObject.activeInHierarchy)
                    activeRendererCount++;
                if (renderers[i] != null && renderers[i].sharedMaterials != null)
                    materialSlotCount += renderers[i].sharedMaterials.Length;
            }

            meshFilterCount = meshFilters.Length;
            lightCount = lights.Length;
            cameraCount = cameras.Length;
            canvasCount = canvases.Length;
            behaviourCount = behaviours.Length;
            summary = "Objects " + gameObjectCount + ", renderers " + activeRendererCount + ", materials " + materialSlotCount;
        }

        public bool IsWithinBudget(PerformanceBudgetProfile profile)
        {
            if (profile == null)
                return true;
            return gameObjectCount <= profile.maxSceneGameObjects && activeRendererCount <= profile.maxActiveRenderers;
        }
    }
}
