using System.Collections.Generic;

namespace ProjectAegisRTS.Victory
{
    public sealed class VictoryCondition
    {
        public string ConditionId { get; private set; }
        public string Description { get; private set; }
        public IReadOnlyList<string> RequiredEnemyCommandTypeIds { get; private set; }
        public bool FallbackToAllEnemyCombatOrBuildingActors { get; private set; }

        public VictoryCondition(string conditionId, string description, IReadOnlyList<string> requiredEnemyCommandTypeIds, bool fallbackToAllEnemyCombatOrBuildingActors)
        {
            ConditionId = conditionId ?? string.Empty;
            Description = description ?? string.Empty;
            RequiredEnemyCommandTypeIds = requiredEnemyCommandTypeIds ?? new string[0];
            FallbackToAllEnemyCombatOrBuildingActors = fallbackToAllEnemyCombatOrBuildingActors;
        }
    }

    public sealed class DefeatCondition
    {
        public string ConditionId { get; private set; }
        public string Description { get; private set; }
        public IReadOnlyList<string> RequiredPlayerCommandTypeIds { get; private set; }

        public DefeatCondition(string conditionId, string description, IReadOnlyList<string> requiredPlayerCommandTypeIds)
        {
            ConditionId = conditionId ?? string.Empty;
            Description = description ?? string.Empty;
            RequiredPlayerCommandTypeIds = requiredPlayerCommandTypeIds ?? new string[0];
        }
    }
}
