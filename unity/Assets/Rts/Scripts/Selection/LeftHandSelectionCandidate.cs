using ProjectAegisRTS.Core;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Selection
{
    public sealed class LeftHandSelectionCandidate
    {
        public int ActorId;
        public string TypeId;
        public string Category;
        public Int2 Cell;
        public Vector3 WorldPosition;
        public float DistanceToRayOrCell;
        public int Priority;
        public string DisplayName;

        public override string ToString()
        {
            return ActorId + " " + DisplayName + " " + Category + " d=" + DistanceToRayOrCell.ToString("0.00");
        }
    }
}
