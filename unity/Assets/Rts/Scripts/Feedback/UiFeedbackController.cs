using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Feedback
{
    public sealed class UiFeedbackController : MonoBehaviour
    {
        const int MaxMessages = 8;
        readonly Queue<string> messages = new Queue<string>();

        public FeedbackEventBus eventBus;

        public int MessageCount { get; private set; }
        public string LastMessage { get; private set; }

        void OnEnable()
        {
            Subscribe();
        }

        void OnDisable()
        {
            Unsubscribe();
        }

        public void Initialize(FeedbackEventBus bus)
        {
            Unsubscribe();
            eventBus = bus;
            Subscribe();
        }

        public string Summary
        {
            get
            {
                return string.IsNullOrEmpty(LastMessage) ? "No feedback messages." : LastMessage;
            }
        }

        public string[] RecentMessages()
        {
            return messages.ToArray();
        }

        void HandleFeedback(FeedbackEvent feedbackEvent)
        {
            if (feedbackEvent == null)
                return;

            LastMessage = feedbackEvent.eventType + ": " + feedbackEvent.message;
            messages.Enqueue(LastMessage);
            while (messages.Count > MaxMessages)
                messages.Dequeue();
            MessageCount++;
        }

        void Subscribe()
        {
            if (eventBus != null)
                eventBus.EventEmitted += HandleFeedback;
        }

        void Unsubscribe()
        {
            if (eventBus != null)
                eventBus.EventEmitted -= HandleFeedback;
        }
    }
}
