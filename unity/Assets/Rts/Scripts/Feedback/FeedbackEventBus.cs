using System;
using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Power;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Feedback
{
    public sealed class FeedbackEventBus : MonoBehaviour
    {
        const int MaxRecentEvents = 80;

        readonly List<FeedbackEvent> recentEvents = new List<FeedbackEvent>();
        readonly HashSet<int> seenCombatEvents = new HashSet<int>();
        readonly HashSet<int> seenEconomyEvents = new HashSet<int>();
        readonly HashSet<int> knownActors = new HashSet<int>();
        readonly Dictionary<string, string> productionStates = new Dictionary<string, string>();

        int nextFeedbackEventId = 1;
        bool snapshotStateSeeded;
        string selectedKey = string.Empty;
        string playerPowerState = string.Empty;
        bool radarActive;

        public RtsSimulationDriver driver;
        public BoardCoordinateMapper mapper;

        public event Action<FeedbackEvent> EventEmitted;

        public IReadOnlyList<FeedbackEvent> RecentEvents { get { return recentEvents; } }
        public int TotalEventCount { get; private set; }
        public int EventsThisFrame { get; private set; }
        public FeedbackEventType LastEventType { get; private set; }
        public string LastMessage { get; private set; }

        void Update()
        {
            if (driver != null && driver.LatestSnapshot != null)
                RenderSnapshot(driver.LatestSnapshot);
        }

        public void Initialize(RtsSimulationDriver simulationDriver, BoardCoordinateMapper coordinateMapper)
        {
            driver = simulationDriver;
            mapper = coordinateMapper;
            if (driver != null)
                driver.feedbackEventBus = this;
            ResetFeedbackState();
        }

        public void ResetFeedbackState()
        {
            recentEvents.Clear();
            ResetSnapshotTracking();
            nextFeedbackEventId = 1;
            TotalEventCount = 0;
            LastEventType = FeedbackEventType.Generic;
            LastMessage = string.Empty;
        }

        public void ResetSnapshotTracking()
        {
            seenCombatEvents.Clear();
            seenEconomyEvents.Clear();
            knownActors.Clear();
            productionStates.Clear();
            EventsThisFrame = 0;
            selectedKey = string.Empty;
            playerPowerState = string.Empty;
            radarActive = false;
            snapshotStateSeeded = false;
        }

        public void EmitCommandFeedback(FeedbackEventType successType, RtsCommandResult result, Int2 cell, int sourceActorId, string label)
        {
            var snapshot = driver == null ? null : driver.LatestSnapshot;
            var type = result != null && !result.Success ? FeedbackEventType.InvalidCommand : successType;
            var tick = snapshot == null ? 0 : snapshot.Tick;
            var message = result == null ? (label ?? string.Empty) : result.ToString();
            Emit(type, tick, sourceActorId, 0, cell, ResolveCellPosition(cell), message, result != null && !result.Success ? 0.65f : 1f, true);
        }

        public void EmitManual(FeedbackEventType eventType, string message)
        {
            var snapshot = driver == null ? null : driver.LatestSnapshot;
            var cell = driver != null && driver.HasHoveredCell ? driver.HoveredCell : new Int2(16, 16);
            Emit(eventType, snapshot == null ? 0 : snapshot.Tick, 0, 0, cell, ResolveCellPosition(cell), message, 0.8f, true);
        }

        public void RenderSnapshot(WorldSnapshot snapshot)
        {
            EventsThisFrame = 0;
            if (snapshot == null)
                return;

            if (!snapshotStateSeeded)
            {
                SeedSnapshotState(snapshot);
                snapshotStateSeeded = true;
            }

            ProcessSelection(snapshot);
            ProcessPlayers(snapshot);
            ProcessActors(snapshot);
            ProcessCombatEvents(snapshot);
            ProcessEconomyEvents(snapshot);
            ProcessRadar(snapshot);
        }

        void SeedSnapshotState(WorldSnapshot snapshot)
        {
            selectedKey = BuildSelectionKey();
            for (var i = 0; i < snapshot.Actors.Count; i++)
                knownActors.Add(snapshot.Actors[i].ActorId);

            var player = GetLocalPlayer(snapshot);
            if (player != null)
            {
                playerPowerState = player.Power == null ? string.Empty : player.Power.State.ToString();
                for (var i = 0; i < player.Production.Count; i++)
                {
                    var item = player.Production[i];
                    productionStates[ProductionKey(player.PlayerId, item.QueueItemId)] = item.State;
                }
            }

            radarActive = snapshot.Radar != null && snapshot.Radar.IsActive;
            for (var i = 0; i < snapshot.CombatEvents.Count; i++)
                seenCombatEvents.Add(snapshot.CombatEvents[i].EventId);
            if (snapshot.Economy != null)
                for (var i = 0; i < snapshot.Economy.Events.Count; i++)
                    seenEconomyEvents.Add(snapshot.Economy.Events[i].EventId);
        }

        void ProcessSelection(WorldSnapshot snapshot)
        {
            var key = BuildSelectionKey();
            if (key == selectedKey)
                return;

            selectedKey = key;
            Emit(FeedbackEventType.SelectionChanged, snapshot.Tick, 0, 0, Int2.Zero, ResolveCellPosition(Int2.Zero), "Selection changed: " + (string.IsNullOrEmpty(key) ? "none" : key), 0.7f, false);
        }

        void ProcessPlayers(WorldSnapshot snapshot)
        {
            var player = GetLocalPlayer(snapshot);
            if (player == null)
                return;

            var nextPowerState = player.Power == null ? string.Empty : player.Power.State.ToString();
            if (nextPowerState != playerPowerState)
            {
                playerPowerState = nextPowerState;
                if (player.Power != null && player.Power.State != PlayerPowerState.Normal)
                    Emit(FeedbackEventType.LowPower, snapshot.Tick, 0, 0, Int2.Zero, ResolveCellPosition(Int2.Zero), "Power state: " + nextPowerState, 0.9f, false);
            }

            for (var i = 0; i < player.Production.Count; i++)
            {
                var item = player.Production[i];
                var key = ProductionKey(player.PlayerId, item.QueueItemId);
                string previous;
                if (!productionStates.TryGetValue(key, out previous))
                {
                    productionStates[key] = item.State;
                    Emit(FeedbackEventType.ProductionStarted, snapshot.Tick, item.ProducerActorId, 0, Int2.Zero, ResolveCellPosition(Int2.Zero), "Production started: " + item.TypeId, 0.75f, false);
                    continue;
                }

                if (previous != item.State)
                {
                    productionStates[key] = item.State;
                    if (item.State == "CompletedPendingPlacement")
                        Emit(FeedbackEventType.ProductionCompleted, snapshot.Tick, item.ProducerActorId, 0, Int2.Zero, ResolveCellPosition(Int2.Zero), "Production completed: " + item.TypeId, 0.9f, false);
                }
            }
        }

        void ProcessActors(WorldSnapshot snapshot)
        {
            for (var i = 0; i < snapshot.Actors.Count; i++)
            {
                var actor = snapshot.Actors[i];
                if (knownActors.Contains(actor.ActorId))
                    continue;

                knownActors.Add(actor.ActorId);
                if (actor.OwnerId == (driver == null ? 1 : driver.PlayerId))
                    Emit(FeedbackEventType.BuildingPlaced, snapshot.Tick, actor.ActorId, 0, actor.CellPosition, ResolveCellPosition(actor.CellPosition), "Actor appeared: " + actor.TypeId, 0.8f, false);
            }
        }

        void ProcessCombatEvents(WorldSnapshot snapshot)
        {
            for (var i = 0; i < snapshot.CombatEvents.Count; i++)
            {
                var combatEvent = snapshot.CombatEvents[i];
                if (seenCombatEvents.Contains(combatEvent.EventId))
                    continue;

                seenCombatEvents.Add(combatEvent.EventId);
                var position = mapper == null ? ResolveCellPosition(combatEvent.Cell) : mapper.FixedWorldToBoardWorld(combatEvent.FixedWorldPosition);

                if (combatEvent.EventType == "WeaponFired")
                    Emit(FeedbackEventType.Attack, combatEvent.Tick, combatEvent.SourceActorId, combatEvent.TargetActorId, combatEvent.Cell, position, "Attack: " + combatEvent.WeaponId, 0.8f, false);
                else if (combatEvent.EventType == "DamageApplied")
                {
                    Emit(FeedbackEventType.ProjectileImpact, combatEvent.Tick, combatEvent.SourceActorId, combatEvent.TargetActorId, combatEvent.Cell, position, "Impact: " + combatEvent.WeaponId, 0.9f, false);
                    Emit(FeedbackEventType.Damage, combatEvent.Tick, combatEvent.SourceActorId, combatEvent.TargetActorId, combatEvent.Cell, position + Vector3.up * 0.15f, "Damage: " + combatEvent.Damage, 1f, false);
                }
                else if (combatEvent.EventType == "ActorDestroyed")
                    Emit(FeedbackEventType.DeathDestruction, combatEvent.Tick, combatEvent.SourceActorId, combatEvent.TargetActorId, combatEvent.Cell, position, "Destroyed actor " + combatEvent.TargetActorId, 1f, false);
            }
        }

        void ProcessEconomyEvents(WorldSnapshot snapshot)
        {
            if (snapshot.Economy == null)
                return;

            for (var i = 0; i < snapshot.Economy.Events.Count; i++)
            {
                var economyEvent = snapshot.Economy.Events[i];
                if (seenEconomyEvents.Contains(economyEvent.EventId))
                    continue;

                seenEconomyEvents.Add(economyEvent.EventId);
                var type = economyEvent.EventType == "HarvesterUnloaded" ? FeedbackEventType.Unload : FeedbackEventType.Harvest;
                Emit(type, economyEvent.Tick, economyEvent.HarvesterActorId, economyEvent.RefineryActorId, economyEvent.Cell, ResolveCellPosition(economyEvent.Cell), economyEvent.EventType + ": " + economyEvent.Amount, 0.75f, false);
            }
        }

        void ProcessRadar(WorldSnapshot snapshot)
        {
            if (snapshot.Radar == null)
                return;

            if (snapshot.Radar.IsActive == radarActive)
                return;

            radarActive = snapshot.Radar.IsActive;
            Emit(FeedbackEventType.FogRadarChanged, snapshot.Tick, snapshot.Radar.ProviderActorId, 0, Int2.Zero, ResolveCellPosition(Int2.Zero), "Radar active: " + radarActive, 0.7f, false);
        }

        void Emit(FeedbackEventType type, int tick, int sourceActorId, int targetActorId, Int2 cell, Vector3 worldPosition, string message, float intensity, bool commandEvent)
        {
            var feedbackEvent = new FeedbackEvent(nextFeedbackEventId++, tick, type, sourceActorId, targetActorId, cell, worldPosition, message, Mathf.Clamp01(intensity), commandEvent);
            recentEvents.Add(feedbackEvent);
            while (recentEvents.Count > MaxRecentEvents)
                recentEvents.RemoveAt(0);

            LastEventType = type;
            LastMessage = feedbackEvent.message;
            TotalEventCount++;
            EventsThisFrame++;

            var handler = EventEmitted;
            if (handler != null)
                handler(feedbackEvent);
        }

        Vector3 ResolveCellPosition(Int2 cell)
        {
            return mapper == null ? new Vector3(cell.X + 0.5f, 0.8f, cell.Y + 0.5f) : mapper.CellToWorldCenter(cell) + Vector3.up * 0.8f;
        }

        PlayerSnapshot GetLocalPlayer(WorldSnapshot snapshot)
        {
            var playerId = driver == null ? 1 : driver.PlayerId;
            for (var i = 0; i < snapshot.Players.Count; i++)
                if (snapshot.Players[i].PlayerId == playerId)
                    return snapshot.Players[i];
            return null;
        }

        string BuildSelectionKey()
        {
            if (driver == null || driver.SelectedActorIds == null || driver.SelectedActorIds.Count == 0)
                return string.Empty;

            return string.Join(",", driver.SelectedActorIds);
        }

        static string ProductionKey(int playerId, int queueItemId)
        {
            return playerId.ToString() + ":" + queueItemId.ToString();
        }
    }
}
