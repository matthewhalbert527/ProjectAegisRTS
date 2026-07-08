using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Feedback;
using ProjectAegisRTS.UnityClient.Rendering.Combat;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Performance
{
    public sealed class RuntimePerformanceStats : MonoBehaviour
    {
        public RtsSimulationDriver driver;
        public ProjectileRenderSystem projectileRenderSystem;
        public VfxFeedbackController vfxFeedbackController;
        public ObjectPoolService objectPoolService;

        public int FrameCount { get; private set; }
        public float SmoothedFps { get; private set; }
        public float AverageFrameMs { get; private set; }
        public float WorstFrameMs { get; private set; }
        public int ActorSnapshotCount { get; private set; }
        public int ProjectileVisualCount { get; private set; }
        public int FeedbackMarkerCount { get; private set; }
        public int PoolInactiveCount { get; private set; }
        public int PoolCreatedCount { get; private set; }
        public int PoolReusedCount { get; private set; }
        public int PoolReleasedCount { get; private set; }

        float accumulatedMs;

        void Update()
        {
            RecordFrame(Time.unscaledDeltaTime);
        }

        public void Initialize(RtsSimulationDriver simulationDriver, ProjectileRenderSystem projectileSystem, VfxFeedbackController feedbackController, ObjectPoolService poolService)
        {
            driver = simulationDriver;
            projectileRenderSystem = projectileSystem;
            vfxFeedbackController = feedbackController;
            objectPoolService = poolService;
            RefreshSnapshotCounts();
        }

        public void RecordFrame(float deltaTimeSeconds)
        {
            if (deltaTimeSeconds <= 0f)
                return;

            var frameMs = deltaTimeSeconds * 1000f;
            FrameCount++;
            accumulatedMs += frameMs;
            AverageFrameMs = accumulatedMs / FrameCount;
            if (frameMs > WorstFrameMs)
                WorstFrameMs = frameMs;

            var fps = 1f / deltaTimeSeconds;
            SmoothedFps = SmoothedFps <= 0f ? fps : Mathf.Lerp(SmoothedFps, fps, 0.08f);
            RefreshSnapshotCounts();
        }

        public void RefreshSnapshotCounts()
        {
            ActorSnapshotCount = driver != null && driver.LatestSnapshot != null ? driver.LatestSnapshot.Actors.Count : 0;
            ProjectileVisualCount = projectileRenderSystem != null ? projectileRenderSystem.ProjectileVisualCount : 0;
            FeedbackMarkerCount = vfxFeedbackController != null ? vfxFeedbackController.ActiveMarkerCount : 0;
            PoolInactiveCount = objectPoolService != null ? objectPoolService.InactiveCount : 0;
            PoolCreatedCount = objectPoolService != null ? objectPoolService.CreatedCount : 0;
            PoolReusedCount = objectPoolService != null ? objectPoolService.ReusedCount : 0;
            PoolReleasedCount = objectPoolService != null ? objectPoolService.ReleasedCount : 0;
        }

        public bool IsWithinBudget(PerformanceBudgetProfile profile)
        {
            if (profile == null)
                return true;

            return ActorSnapshotCount <= profile.maxActorViews &&
                ProjectileVisualCount <= profile.maxProjectileViews &&
                FeedbackMarkerCount <= profile.maxFeedbackMarkers &&
                PoolInactiveCount <= profile.maxPoolInactiveObjects;
        }
    }
}
