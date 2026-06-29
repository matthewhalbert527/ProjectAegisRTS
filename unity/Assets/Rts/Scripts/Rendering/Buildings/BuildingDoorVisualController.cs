using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Buildings
{
    public sealed class BuildingDoorVisualController : MonoBehaviour
    {
        BuildingPlaceholderPartFactory.PartSet parts;
        BuildingVisualProfile profile;
        Vector3 closedLocalPosition;

        public float DoorOpen01 { get; private set; }
        public bool IsOpening { get; private set; }
        public bool IsClosing { get; private set; }

        public void Initialize(BuildingPlaceholderPartFactory.PartSet partSet, BuildingVisualProfile activeProfile)
        {
            parts = partSet;
            profile = activeProfile;
            if (parts != null && parts.Door != null)
                closedLocalPosition = parts.Door.localPosition;
        }

        public void TickVisual(float deltaTime, bool shouldOpen)
        {
            var previous = DoorOpen01;
            var speed = profile == null ? 4f : profile.doorOpenSpeed;
            DoorOpen01 = Mathf.MoveTowards(DoorOpen01, shouldOpen ? 1f : 0f, Mathf.Max(0.1f, speed) * deltaTime);
            IsOpening = DoorOpen01 > previous;
            IsClosing = DoorOpen01 < previous;

            if (parts == null || parts.Door == null)
                return;

            var distance = profile == null ? 0.35f : profile.doorOpenDistance;
            parts.Door.localPosition = closedLocalPosition + Vector3.down * distance * DoorOpen01;
        }
    }
}
