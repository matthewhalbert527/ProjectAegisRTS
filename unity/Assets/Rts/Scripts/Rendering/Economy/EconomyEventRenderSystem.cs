using System.Collections.Generic;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Economy
{
    public sealed class EconomyEventRenderSystem : MonoBehaviour
    {
        readonly HashSet<int> playedEventIds = new HashSet<int>();
        readonly Queue<int> playedOrder = new Queue<int>();
        const int MaxPlayedEvents = 128;

        public RtsSimulationDriver driver;
        public BoardCoordinateMapper mapper;
        public int PlayedEventCount { get; private set; }
        public string LastEventType { get; private set; }

        void Update()
        {
            if (driver != null && driver.LatestSnapshot != null)
                RenderSnapshot(driver.LatestSnapshot);
        }

        public void Initialize(RtsSimulationDriver simulationDriver, BoardCoordinateMapper coordinateMapper)
        {
            driver = simulationDriver;
            mapper = coordinateMapper;
        }

        public void RenderSnapshot(WorldSnapshot snapshot)
        {
            if (snapshot == null || snapshot.Economy == null)
                return;
            if (mapper == null)
                mapper = Object.FindFirstObjectByType<BoardCoordinateMapper>();
            if (mapper == null)
                return;

            for (var i = 0; i < snapshot.Economy.Events.Count; i++)
            {
                var economyEvent = snapshot.Economy.Events[i];
                if (playedEventIds.Contains(economyEvent.EventId))
                    continue;

                MarkPlayed(economyEvent.EventId);
                LastEventType = economyEvent.EventType;
                var marker = GameObject.CreatePrimitive(economyEvent.EventType == "HarvesterUnloaded" ? PrimitiveType.Sphere : PrimitiveType.Cube);
                marker.name = "Economy Event " + economyEvent.EventType;
                marker.transform.SetParent(transform, false);
                marker.transform.position = mapper.CellToWorldCenter(economyEvent.Cell) + Vector3.up * 0.9f;
                marker.transform.localScale = Vector3.one * (economyEvent.EventType == "HarvesterUnloaded" ? 0.22f : 0.16f);
                var collider = marker.GetComponent<Collider>();
                if (collider != null)
                    EconomyObjectUtility.DestroyObject(collider);
                var renderer = marker.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
                    renderer.sharedMaterial.color = economyEvent.EventType == "HarvesterUnloaded" ? new Color(0.95f, 0.9f, 0.2f, 1f) : new Color(0.1f, 0.85f, 0.5f, 1f);
                }
                var timed = marker.AddComponent<TimedEconomyMarker>();
                timed.duration = 0.5f;
            }

            PlayedEventCount = playedEventIds.Count;
        }

        void MarkPlayed(int eventId)
        {
            playedEventIds.Add(eventId);
            playedOrder.Enqueue(eventId);
            while (playedOrder.Count > MaxPlayedEvents)
                playedEventIds.Remove(playedOrder.Dequeue());
        }
    }
}
