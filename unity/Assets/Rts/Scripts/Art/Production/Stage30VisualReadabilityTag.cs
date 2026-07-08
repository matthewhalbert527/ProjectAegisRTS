namespace ProjectAegisRTS.UnityClient.Art.Production
{
    public sealed class Stage30VisualReadabilityTag : UnityEngine.MonoBehaviour
    {
        public string actorTypeId;
        public bool hasGroundContrastOutline;
        public bool hasTopDownIdentityAccent;
        public bool hasForwardReadabilityCue;
        public bool preservesStage29Detail;
        public bool questSafeOverlayBudget;
        public string notes;

        public bool IsComplete()
        {
            return !string.IsNullOrEmpty(actorTypeId) &&
                   hasGroundContrastOutline &&
                   hasTopDownIdentityAccent &&
                   hasForwardReadabilityCue &&
                   preservesStage29Detail &&
                   questSafeOverlayBudget;
        }
    }
}
