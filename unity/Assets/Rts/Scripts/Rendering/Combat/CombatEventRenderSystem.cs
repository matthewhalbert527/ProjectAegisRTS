using System.Collections.Generic;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Combat
{
    public sealed class CombatEventRenderSystem : MonoBehaviour
    {
        readonly HashSet<int> playedEventIds = new HashSet<int>();
        readonly Queue<int> playedOrder = new Queue<int>();
        const int MaxPlayedEvents = 128;

        public RtsSimulationDriver driver;
        public BoardCoordinateMapper mapper;
        public CombatVisualProfileLibrary profileLibrary;
        public MuzzleFlashVisualController muzzleFlashController;
        public ImpactVisualController impactController;
        public DamageVisualController damageController;
        public DeathVisualController deathController;

        public int PlayedEventCount { get; private set; }
        public int LastEventId { get; private set; }

        void Update()
        {
            if (driver != null && driver.LatestSnapshot != null)
                RenderSnapshot(driver.LatestSnapshot);
        }

        public void Initialize(RtsSimulationDriver simulationDriver, BoardCoordinateMapper coordinateMapper, CombatVisualProfileLibrary library)
        {
            driver = simulationDriver;
            mapper = coordinateMapper;
            profileLibrary = library;
            if (profileLibrary != null)
                profileLibrary.EnsureInitialized();
            EnsureControllers();
        }

        public void RenderSnapshot(WorldSnapshot snapshot)
        {
            if (snapshot == null)
                return;
            if (mapper == null)
                mapper = Object.FindFirstObjectByType<BoardCoordinateMapper>();
            if (profileLibrary == null)
                profileLibrary = Object.FindFirstObjectByType<CombatVisualProfileLibrary>();
            if (mapper == null)
                return;

            EnsureControllers();
            for (var i = 0; i < snapshot.CombatEvents.Count; i++)
            {
                var combatEvent = snapshot.CombatEvents[i];
                if (playedEventIds.Contains(combatEvent.EventId))
                    continue;

                MarkPlayed(combatEvent.EventId);
                var position = mapper.FixedWorldToBoardWorld(combatEvent.FixedWorldPosition);
                var profile = profileLibrary == null ? null : profileLibrary.GetProfileForWeapon(combatEvent.WeaponId);

                if (combatEvent.EventType == "WeaponFired")
                    muzzleFlashController.Spawn(position, profile);
                else if (combatEvent.EventType == "DamageApplied")
                {
                    impactController.Spawn(position, profile);
                    damageController.Spawn(position, combatEvent.Damage);
                }
                else if (combatEvent.EventType == "ActorDestroyed")
                    deathController.Spawn(position, profileLibrary == null ? null : profileLibrary.GetProfile("explosion_placeholder"));

                LastEventId = combatEvent.EventId;
            }

            PlayedEventCount = playedEventIds.Count;
        }

        void EnsureControllers()
        {
            if (muzzleFlashController == null)
                muzzleFlashController = GetOrAdd<MuzzleFlashVisualController>();
            if (impactController == null)
                impactController = GetOrAdd<ImpactVisualController>();
            if (damageController == null)
                damageController = GetOrAdd<DamageVisualController>();
            if (deathController == null)
                deathController = GetOrAdd<DeathVisualController>();
        }

        void MarkPlayed(int eventId)
        {
            playedEventIds.Add(eventId);
            playedOrder.Enqueue(eventId);
            while (playedOrder.Count > MaxPlayedEvents)
                playedEventIds.Remove(playedOrder.Dequeue());
        }

        T GetOrAdd<T>() where T : Component
        {
            var component = GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }
    }
}
