using System;
using ProjectAegisRTS.Core;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Feedback
{
    [Serializable]
    public sealed class FeedbackEvent
    {
        public int eventId;
        public int tick;
        public FeedbackEventType eventType;
        public int sourceActorId;
        public int targetActorId;
        public Int2 cell;
        public Vector3 worldPosition;
        public string message;
        public float intensity;
        public bool commandEvent;

        public FeedbackEvent(
            int eventId,
            int tick,
            FeedbackEventType eventType,
            int sourceActorId,
            int targetActorId,
            Int2 cell,
            Vector3 worldPosition,
            string message,
            float intensity,
            bool commandEvent)
        {
            this.eventId = eventId;
            this.tick = tick;
            this.eventType = eventType;
            this.sourceActorId = sourceActorId;
            this.targetActorId = targetActorId;
            this.cell = cell;
            this.worldPosition = worldPosition;
            this.message = message ?? string.Empty;
            this.intensity = intensity;
            this.commandEvent = commandEvent;
        }
    }
}
