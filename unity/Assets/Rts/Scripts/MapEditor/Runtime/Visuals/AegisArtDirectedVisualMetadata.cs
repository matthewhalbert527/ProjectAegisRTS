using System;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.MapEditor.Visuals
{
    [Serializable]
    public sealed class AegisArtDirectedVisualMetadata
    {
        public string formatVersion;
        public string mapId;
        public string authoringIntent;
        public AegisAuthoredVisualRoadSegment[] roadSegments;
        public AegisAuthoredVisualCrossing[] crossings;
        public string[] notes;

        public void Normalize()
        {
            roadSegments = roadSegments ?? new AegisAuthoredVisualRoadSegment[0];
            crossings = crossings ?? new AegisAuthoredVisualCrossing[0];
            notes = notes ?? new string[0];
        }

        public bool HasAuthoredRoads
        {
            get { return roadSegments != null && roadSegments.Length > 0; }
        }
    }

    [Serializable]
    public sealed class AegisAuthoredVisualRoadSegment
    {
        public string id;
        public float ax;
        public float ay;
        public float bx;
        public float by;
        public float width = 2.6f;
        public string role;
        public string crossingKind;

        public bool IsBridgeOrFord
        {
            get
            {
                if (string.IsNullOrEmpty(crossingKind))
                    return false;

                var value = crossingKind.ToLowerInvariant();
                return value.Contains("bridge") || value.Contains("ford");
            }
        }

        public Vector2 A
        {
            get { return new Vector2(ax, ay); }
        }

        public Vector2 B
        {
            get { return new Vector2(bx, by); }
        }
    }

    [Serializable]
    public sealed class AegisAuthoredVisualCrossing
    {
        public string id;
        public string roadSegmentId;
        public string crossingKind;
        public float x;
        public float y;
        public string note;

        public bool IsBridgeOrFord
        {
            get
            {
                if (string.IsNullOrEmpty(crossingKind))
                    return false;

                var value = crossingKind.ToLowerInvariant();
                return value.Contains("bridge") || value.Contains("ford");
            }
        }
    }
}
