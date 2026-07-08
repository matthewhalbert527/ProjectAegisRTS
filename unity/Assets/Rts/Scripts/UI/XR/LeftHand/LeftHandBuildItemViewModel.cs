namespace ProjectAegisRTS.UnityClient.UI.XR.LeftHand
{
    public sealed class LeftHandBuildItemViewModel
    {
        public string ActorTypeId;
        public string DisplayName;
        public LeftHandBuildCategory Category;
        public int Cost;
        public int BuildTimeTicks;
        public bool IsAvailable;
        public bool IsQueued;
        public bool IsProducing;
        public bool IsPendingPlacement;
        public float Progress01;
        public string StatusText;
    }
}
