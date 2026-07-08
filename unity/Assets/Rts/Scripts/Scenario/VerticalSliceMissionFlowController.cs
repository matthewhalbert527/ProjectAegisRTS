using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.CoreBridge;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Scenario
{
    public sealed class VerticalSliceMissionFlowController : MonoBehaviour
    {
        readonly List<string> completedBeatLabels = new List<string>();

        public RtsSimulationDriver driver;
        public VerticalSliceProgressTracker progressTracker;

        public int CurrentBeatIndex { get; private set; }
        public string CurrentBeatId { get; private set; }
        public string CurrentInstructionText { get; private set; }
        public string NextRecommendedAction { get; private set; }
        public string RecommendedTypeId { get; private set; }
        public IReadOnlyList<string> CompletedBeatLabels { get { return completedBeatLabels; } }

        public void Initialize(RtsSimulationDriver simulationDriver, VerticalSliceProgressTracker tracker)
        {
            driver = simulationDriver;
            progressTracker = tracker;
            Refresh();
        }

        void Awake()
        {
            if (driver == null)
                driver = FindAnyObjectByType<RtsSimulationDriver>();
            if (progressTracker == null)
                progressTracker = FindAnyObjectByType<VerticalSliceProgressTracker>();
            Refresh();
        }

        void Update()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (progressTracker == null)
                progressTracker = FindAnyObjectByType<VerticalSliceProgressTracker>();
            if (progressTracker == null)
            {
                CurrentBeatIndex = 0;
                CurrentBeatId = BeatId(0);
                CurrentInstructionText = "Start the vertical slice to begin.";
                NextRecommendedAction = "Open the vertical slice.";
                RecommendedTypeId = string.Empty;
                completedBeatLabels.Clear();
                return;
            }

            progressTracker.Refresh();
            CurrentBeatIndex = progressTracker.GetCurrentStepIndex();
            CurrentBeatId = BeatId(CurrentBeatIndex);
            CurrentInstructionText = progressTracker.currentChecklistPrompt;
            RecommendedTypeId = progressTracker.recommendedTypeId;
            NextRecommendedAction = BuildRecommendedAction(CurrentBeatIndex, RecommendedTypeId);

            completedBeatLabels.Clear();
            for (var i = 0; i < VerticalSliceProgressTracker.MissionStepCount; i++)
                if (progressTracker.IsStepComplete(i))
                    completedBeatLabels.Add(progressTracker.GetStepLabel(i));
        }

        public string GetBeatLabel(int beatIndex)
        {
            if (progressTracker == null)
                return string.Empty;
            return progressTracker.GetStepLabel(beatIndex);
        }

        public bool IsBeatComplete(int beatIndex)
        {
            return progressTracker != null && progressTracker.IsStepComplete(beatIndex);
        }

        static string BeatId(int beatIndex)
        {
            switch (beatIndex)
            {
                case 0: return "welcome_camera_overview";
                case 1: return "select_fabrication_hub";
                case 2: return "build_power_plant";
                case 3: return "place_power_plant";
                case 4: return "build_refinery";
                case 5: return "place_refinery";
                case 6: return "observe_harvester_loop";
                case 7: return "build_barracks";
                case 8: return "train_infantry";
                case 9: return "build_war_factory";
                case 10: return "produce_light_tank";
                case 11: return "scout_enemy_base";
                case 12: return "attack_enemy";
                case 13: return "destroy_enemy_base";
                case 14: return "victory";
                default: return "unknown";
            }
        }

        static string BuildRecommendedAction(int beatIndex, string recommendedTypeId)
        {
            if (!string.IsNullOrEmpty(recommendedTypeId))
                return "Recommended: " + DisplayType(recommendedTypeId);

            switch (beatIndex)
            {
                case 1: return "Select the Fabrication Hub.";
                case 3: return "Place on green fine-grid cells.";
                case 5: return "Place near the resource field.";
                case 6: return "Let the harvester unload.";
                case 11: return "Scout east/northeast.";
                case 12: return "Move units into range and attack.";
                case 13: return "Focus the enemy Fabrication Hub.";
                case 14: return "Use the result screen.";
                default: return "Follow the current objective.";
            }
        }

        static string DisplayType(string typeId)
        {
            return string.IsNullOrEmpty(typeId) ? string.Empty : typeId.Replace("_", " ");
        }
    }
}
