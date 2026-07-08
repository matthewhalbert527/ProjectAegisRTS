using System.Collections.Generic;
using ProjectAegisRTS.Victory;

namespace ProjectAegisRTS.Scenarios
{
    public enum ScenarioObjectiveState
    {
        Inactive,
        Active,
        Completed,
        Failed
    }

    public sealed class ScenarioObjective
    {
        public string ObjectiveId { get; private set; }
        public string Description { get; private set; }

        public ScenarioObjective(string objectiveId, string description)
        {
            ObjectiveId = objectiveId ?? string.Empty;
            Description = description ?? string.Empty;
        }
    }

    public sealed class ScenarioDefinition
    {
        public static readonly ScenarioDefinition Empty = new ScenarioDefinition(
            string.Empty,
            string.Empty,
            0,
            0,
            new ScenarioObjective[0],
            new VictoryCondition(string.Empty, string.Empty, new string[0], false),
            new DefeatCondition(string.Empty, string.Empty, new string[0]));

        public string ScenarioId { get; private set; }
        public string DisplayName { get; private set; }
        public int LocalPlayerId { get; private set; }
        public int EnemyPlayerId { get; private set; }
        public IReadOnlyList<ScenarioObjective> Objectives { get; private set; }
        public VictoryCondition VictoryCondition { get; private set; }
        public DefeatCondition DefeatCondition { get; private set; }

        public ScenarioDefinition(
            string scenarioId,
            string displayName,
            int localPlayerId,
            int enemyPlayerId,
            IReadOnlyList<ScenarioObjective> objectives,
            VictoryCondition victoryCondition,
            DefeatCondition defeatCondition)
        {
            ScenarioId = scenarioId ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            LocalPlayerId = localPlayerId;
            EnemyPlayerId = enemyPlayerId;
            Objectives = objectives ?? new ScenarioObjective[0];
            VictoryCondition = victoryCondition ?? new VictoryCondition(string.Empty, string.Empty, new string[0], false);
            DefeatCondition = defeatCondition ?? new DefeatCondition(string.Empty, string.Empty, new string[0]);
        }

        public static ScenarioDefinition CreateVerticalSlice()
        {
            return CreateVerticalSlice(1, 2);
        }

        public static ScenarioDefinition CreateVerticalSlice(int localPlayerId, int enemyPlayerId)
        {
            return new ScenarioDefinition(
                "stage16_vertical_slice",
                "Stage 16 Playable Vertical Slice",
                localPlayerId,
                enemyPlayerId,
                new[]
                {
                    new ScenarioObjective("destroy_enemy_base", "Destroy the enemy fabrication hub."),
                    new ScenarioObjective("protect_player_base", "Protect the player fabrication hub.")
                },
                new VictoryCondition(
                    "enemy_base_destroyed",
                    "All enemy command buildings are destroyed.",
                    new[] { "fabrication_hub", "enemy_core" },
                    true),
                new DefeatCondition(
                    "player_base_destroyed",
                    "The player command building is destroyed.",
                    new[] { "fabrication_hub", "construction_yard" }));
        }
    }

    public sealed class ScenarioSnapshot
    {
        public static readonly ScenarioSnapshot Empty = new ScenarioSnapshot(string.Empty, string.Empty, new ScenarioObjectiveSnapshot[0]);

        public string ScenarioId { get; private set; }
        public string DisplayName { get; private set; }
        public IReadOnlyList<ScenarioObjectiveSnapshot> Objectives { get; private set; }

        public ScenarioSnapshot(string scenarioId, string displayName, IReadOnlyList<ScenarioObjectiveSnapshot> objectives)
        {
            ScenarioId = scenarioId ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            Objectives = objectives ?? new ScenarioObjectiveSnapshot[0];
        }
    }

    public sealed class ScenarioObjectiveSnapshot
    {
        public string ObjectiveId { get; private set; }
        public string Description { get; private set; }
        public ScenarioObjectiveState State { get; private set; }

        public ScenarioObjectiveSnapshot(string objectiveId, string description, ScenarioObjectiveState state)
        {
            ObjectiveId = objectiveId ?? string.Empty;
            Description = description ?? string.Empty;
            State = state;
        }
    }
}
