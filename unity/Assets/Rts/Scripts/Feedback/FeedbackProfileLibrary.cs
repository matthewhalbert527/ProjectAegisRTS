using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Feedback
{
    public sealed class FeedbackProfileLibrary : MonoBehaviour
    {
        readonly Dictionary<FeedbackEventType, FeedbackProfile> byType = new Dictionary<FeedbackEventType, FeedbackProfile>();

        public FeedbackProfile defaultProfile;
        public FeedbackProfile[] profiles = new FeedbackProfile[0];

        public int ProfileCount { get { return byType.Count; } }

        public void EnsureInitialized()
        {
            byType.Clear();
            if (profiles == null)
                return;

            for (var i = 0; i < profiles.Length; i++)
            {
                var profile = profiles[i];
                if (profile == null)
                    continue;
                byType[profile.eventType] = profile;
                if (defaultProfile == null || profile.eventType == FeedbackEventType.Generic)
                    defaultProfile = profile;
            }
        }

        public FeedbackProfile GetProfile(FeedbackEventType eventType)
        {
            if (byType.Count == 0)
                EnsureInitialized();

            FeedbackProfile profile;
            return byType.TryGetValue(eventType, out profile) ? profile : defaultProfile;
        }
    }
}
