using System.Collections.Generic;
using ProjectAegisRTS.Actors;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Data;
using ProjectAegisRTS.Scenarios;
using ProjectAegisRTS.Simulation;

namespace ProjectAegisRTS.Match
{
    public enum MatchPhase
    {
        NotStarted,
        Starting,
        Running,
        Won,
        Lost,
        Draw
    }

    public enum PlayerOutcome
    {
        None,
        Victory,
        Defeat,
        Draw
    }

    public sealed class MatchState
    {
        readonly Dictionary<string, ScenarioObjectiveState> objectiveStates;

        public ScenarioDefinition ScenarioDefinition { get; private set; }
        public MatchPhase Phase { get; private set; }
        public PlayerOutcome LocalPlayerOutcome { get; private set; }
        public int StartedTick { get; private set; }
        public int ElapsedTicks { get; private set; }
        public int WinningPlayerId { get; private set; }

        public MatchState()
        {
            objectiveStates = new Dictionary<string, ScenarioObjectiveState>();
            ScenarioDefinition = ScenarioDefinition.Empty;
            Phase = MatchPhase.NotStarted;
            LocalPlayerOutcome = PlayerOutcome.None;
            StartedTick = -1;
        }

        public bool IsConfigured
        {
            get { return !string.IsNullOrEmpty(ScenarioDefinition.ScenarioId); }
        }

        public CommandResult Configure(ScenarioDefinition scenarioDefinition)
        {
            if (scenarioDefinition == null || string.IsNullOrEmpty(scenarioDefinition.ScenarioId))
                return CommandResult.Fail("ScenarioMissing", "A scenario definition is required before starting a match.");

            ScenarioDefinition = scenarioDefinition;
            Phase = MatchPhase.NotStarted;
            LocalPlayerOutcome = PlayerOutcome.None;
            StartedTick = -1;
            ElapsedTicks = 0;
            WinningPlayerId = 0;
            objectiveStates.Clear();
            for (var i = 0; i < ScenarioDefinition.Objectives.Count; i++)
                objectiveStates[ScenarioDefinition.Objectives[i].ObjectiveId] = ScenarioObjectiveState.Inactive;

            return CommandResult.Ok("Scenario configured.");
        }

        public CommandResult StartMatch(int tick)
        {
            if (!IsConfigured)
                return CommandResult.Fail("ScenarioMissing", "Configure a scenario before starting the match.");

            if (Phase == MatchPhase.Running || Phase == MatchPhase.Starting)
                return CommandResult.Ok("Match is already running.");

            Phase = MatchPhase.Starting;
            LocalPlayerOutcome = PlayerOutcome.None;
            StartedTick = tick;
            ElapsedTicks = 0;
            WinningPlayerId = 0;
            for (var i = 0; i < ScenarioDefinition.Objectives.Count; i++)
                objectiveStates[ScenarioDefinition.Objectives[i].ObjectiveId] = ScenarioObjectiveState.Active;
            Phase = MatchPhase.Running;
            return CommandResult.Ok("Match started.");
        }

        public CommandResult ResetMatch()
        {
            Phase = MatchPhase.NotStarted;
            LocalPlayerOutcome = PlayerOutcome.None;
            StartedTick = -1;
            ElapsedTicks = 0;
            WinningPlayerId = 0;
            for (var i = 0; i < ScenarioDefinition.Objectives.Count; i++)
                objectiveStates[ScenarioDefinition.Objectives[i].ObjectiveId] = ScenarioObjectiveState.Inactive;

            return CommandResult.Ok("Match reset.");
        }

        public void Update(RtsWorld world)
        {
            if (world == null || Phase != MatchPhase.Running)
                return;

            ElapsedTicks = StartedTick < 0 ? 0 : world.TickNumber - StartedTick;

            var victory = IsVictoryReached(world);
            var defeat = IsDefeatReached(world);
            if (victory && defeat)
            {
                Complete(MatchPhase.Draw, PlayerOutcome.Draw, 0);
                return;
            }
            if (victory)
            {
                Complete(MatchPhase.Won, PlayerOutcome.Victory, ScenarioDefinition.LocalPlayerId);
                return;
            }
            if (defeat)
                Complete(MatchPhase.Lost, PlayerOutcome.Defeat, ScenarioDefinition.EnemyPlayerId);
        }

        public MatchSnapshot CreateSnapshot()
        {
            return new MatchSnapshot(
                ScenarioDefinition.ScenarioId,
                ScenarioDefinition.DisplayName,
                Phase,
                ScenarioDefinition.LocalPlayerId,
                WinningPlayerId,
                LocalPlayerOutcome,
                StartedTick,
                ElapsedTicks);
        }

        public ScenarioSnapshot CreateScenarioSnapshot()
        {
            var objectives = new List<ScenarioObjectiveSnapshot>();
            for (var i = 0; i < ScenarioDefinition.Objectives.Count; i++)
            {
                var objective = ScenarioDefinition.Objectives[i];
                ScenarioObjectiveState state;
                if (!objectiveStates.TryGetValue(objective.ObjectiveId, out state))
                    state = ScenarioObjectiveState.Inactive;

                objectives.Add(new ScenarioObjectiveSnapshot(objective.ObjectiveId, objective.Description, state));
            }

            return new ScenarioSnapshot(ScenarioDefinition.ScenarioId, ScenarioDefinition.DisplayName, objectives);
        }

        bool IsVictoryReached(RtsWorld world)
        {
            var required = ScenarioDefinition.VictoryCondition.RequiredEnemyCommandTypeIds;
            if (required.Count > 0 && AreAllRequiredTypesDestroyed(world, ScenarioDefinition.EnemyPlayerId, required))
                return true;

            return ScenarioDefinition.VictoryCondition.FallbackToAllEnemyCombatOrBuildingActors &&
                AreAllEnemyCombatOrBuildingActorsDestroyed(world);
        }

        bool IsDefeatReached(RtsWorld world)
        {
            var required = ScenarioDefinition.DefeatCondition.RequiredPlayerCommandTypeIds;
            return required.Count > 0 && AreAllRequiredTypesDestroyed(world, ScenarioDefinition.LocalPlayerId, required);
        }

        bool AreAllRequiredTypesDestroyed(RtsWorld world, int ownerPlayerId, IReadOnlyList<string> typeIds)
        {
            var foundAnyKnownRequiredType = false;
            for (var i = 0; i < typeIds.Count; i++)
            {
                var typeId = typeIds[i];
                ActorDefinition ignored;
                if (!world.Rules.TryGetDefinition(typeId, out ignored))
                    continue;

                foundAnyKnownRequiredType = true;
                var foundAlive = false;
                foreach (var pair in world.Actors)
                {
                    var actor = pair.Value;
                    if (actor.OwnerPlayerId == ownerPlayerId && actor.TypeId == typeId && !actor.IsDestroyed)
                    {
                        foundAlive = true;
                        break;
                    }
                }

                if (foundAlive)
                    return false;
            }

            return foundAnyKnownRequiredType;
        }

        bool AreAllEnemyCombatOrBuildingActorsDestroyed(RtsWorld world)
        {
            var foundEnemyObjectiveActor = false;
            foreach (var pair in world.Actors)
            {
                var actor = pair.Value;
                if (actor.OwnerPlayerId != ScenarioDefinition.EnemyPlayerId)
                    continue;

                var definition = world.Rules.GetDefinition(actor.TypeId);
                if (definition.Kind == ActorKind.Building || definition.Weapon != null)
                {
                    foundEnemyObjectiveActor = true;
                    if (!actor.IsDestroyed)
                        return false;
                }
            }

            return foundEnemyObjectiveActor;
        }

        void Complete(MatchPhase phase, PlayerOutcome outcome, int winningPlayerId)
        {
            Phase = phase;
            LocalPlayerOutcome = outcome;
            WinningPlayerId = winningPlayerId;

            for (var i = 0; i < ScenarioDefinition.Objectives.Count; i++)
            {
                var objective = ScenarioDefinition.Objectives[i];
                if (objective.ObjectiveId == "destroy_enemy_base")
                    objectiveStates[objective.ObjectiveId] = outcome == PlayerOutcome.Victory || outcome == PlayerOutcome.Draw ? ScenarioObjectiveState.Completed : ScenarioObjectiveState.Failed;
                else if (objective.ObjectiveId == "protect_player_base")
                    objectiveStates[objective.ObjectiveId] = outcome == PlayerOutcome.Defeat ? ScenarioObjectiveState.Failed : ScenarioObjectiveState.Completed;
            }
        }
    }

    public sealed class MatchSnapshot
    {
        public static readonly MatchSnapshot Empty = new MatchSnapshot(string.Empty, string.Empty, MatchPhase.NotStarted, 0, 0, PlayerOutcome.None, -1, 0);

        public string ScenarioId { get; private set; }
        public string ScenarioName { get; private set; }
        public MatchPhase Phase { get; private set; }
        public int LocalPlayerId { get; private set; }
        public int WinningPlayerId { get; private set; }
        public PlayerOutcome LocalPlayerOutcome { get; private set; }
        public int StartedTick { get; private set; }
        public int ElapsedTicks { get; private set; }

        public MatchSnapshot(string scenarioId, string scenarioName, MatchPhase phase, int localPlayerId, int winningPlayerId, PlayerOutcome localPlayerOutcome, int startedTick, int elapsedTicks)
        {
            ScenarioId = scenarioId ?? string.Empty;
            ScenarioName = scenarioName ?? string.Empty;
            Phase = phase;
            LocalPlayerId = localPlayerId;
            WinningPlayerId = winningPlayerId;
            LocalPlayerOutcome = localPlayerOutcome;
            StartedTick = startedTick;
            ElapsedTicks = elapsedTicks;
        }
    }
}
