using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Buildings
{
    public sealed class BuildingArtShowcaseController : MonoBehaviour
    {
        public string actorTypeId = "building";
        public BuildingVisualProfile profile;
        public bool cycleProductionState;
        public float productionCycleSeconds = 5f;

        BuildingVisualStateController visualState;

        void OnEnable()
        {
            visualState = GetComponentInChildren<BuildingVisualStateController>(true);
            if (visualState == null)
                visualState = gameObject.AddComponent<BuildingVisualStateController>();

            visualState.standaloneTickInPlayMode = true;
            visualState.VisualDebugEnabled = false;
            visualState.Initialize(0, actorTypeId, profile);
        }

        void Update()
        {
            if (visualState == null)
                return;

            if (cycleProductionState && productionCycleSeconds > 0.1f)
            {
                var phase = Mathf.Repeat(Time.time / productionCycleSeconds, 1f);
                visualState.SetDebugForcedState(phase < 0.58f ? BuildingAnimationVisualState.Producing : BuildingAnimationVisualState.PoweredIdle);
            }
            else
            {
                visualState.SetDebugForcedState(BuildingAnimationVisualState.PoweredIdle);
            }
        }
    }
}
