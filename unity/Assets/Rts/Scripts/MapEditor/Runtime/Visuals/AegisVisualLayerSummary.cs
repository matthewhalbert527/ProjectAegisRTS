using System.Collections.Generic;
using System.Text;

namespace ProjectAegisRTS.UnityClient.MapEditor.Visuals
{
    public sealed class AegisVisualLayerSummary
    {
        public string LayerName;
        public int TerrainChunks;
        public int TransitionEdges;
        public int WaterCells;
        public int ShorelineEdges;
        public int RoadSegments;
        public int CliffStraightSegments;
        public int CliffCorners;
        public int CliffEndcaps;
        public int ResourceFields;
        public int ResourceVisualInstances;
        public int ResourceDepletedCount;
        public int ResourceRegeneratingCount;
        public int BasePadCount;
        public int ScatterCount;
        public int SkippedPlacementCount;
        public readonly List<string> Warnings = new List<string>();
        public readonly List<string> Errors = new List<string>();

        public AegisVisualLayerSummary()
        {
        }

        public AegisVisualLayerSummary(string layerName)
        {
            LayerName = layerName;
        }

        public void AddWarning(string warning)
        {
            if (!string.IsNullOrEmpty(warning))
                Warnings.Add(warning);
        }

        public void AddError(string error)
        {
            if (!string.IsNullOrEmpty(error))
                Errors.Add(error);
        }

        public string ToDisplayString()
        {
            var builder = new StringBuilder();
            builder.Append(string.IsNullOrEmpty(LayerName) ? "Unnamed layer" : LayerName);
            AppendMetric(builder, "terrain chunks", TerrainChunks);
            AppendMetric(builder, "transition edges", TransitionEdges);
            AppendMetric(builder, "water cells", WaterCells);
            AppendMetric(builder, "shoreline edges", ShorelineEdges);
            AppendMetric(builder, "road segments", RoadSegments);
            AppendMetric(builder, "cliff straights", CliffStraightSegments);
            AppendMetric(builder, "cliff corners", CliffCorners);
            AppendMetric(builder, "cliff endcaps", CliffEndcaps);
            AppendMetric(builder, "resource fields", ResourceFields);
            AppendMetric(builder, "resource visuals", ResourceVisualInstances);
            AppendMetric(builder, "depleted fields", ResourceDepletedCount);
            AppendMetric(builder, "regenerating fields", ResourceRegeneratingCount);
            AppendMetric(builder, "base pads", BasePadCount);
            AppendMetric(builder, "scatter", ScatterCount);
            AppendMetric(builder, "skipped", SkippedPlacementCount);
            return builder.ToString();
        }

        static void AppendMetric(StringBuilder builder, string label, int value)
        {
            if (value <= 0)
                return;

            builder.Append("; ");
            builder.Append(label);
            builder.Append(": ");
            builder.Append(value);
        }
    }
}
