using System.Collections.Generic;
using System.Text;

namespace ProjectAegisRTS.UnityClient.MapEditor.Visuals
{
    public sealed class AegisVisualLayerSummary
    {
        public string LayerName;
        public int TerrainChunks;
        public int MixedTerrainChunks;
        public int FallbackRoleCount;
        public int TransitionEdges;
        public int WaterCells;
        public int WaterStrips;
        public int WaterMeshes;
        public int WaterSurfaceDetailDecalCount;
        public int ShorelineEdges;
        public int ShorelineMeshes;
        public int RoadSegments;
        public int AuthoredRoadSegments;
        public int FallbackRoadSegments;
        public int AuthoredCrossings;
        public int GeneratedRoadFallbacksUsed;
        public int BridgeCrossings;
        public int FordCrossings;
        public int RoadWaterConflicts;
        public int RoadDetailDecalCount;
        public int CliffStraightSegments;
        public int CliffCorners;
        public int CliffEndcaps;
        public int HiddenDebugFillCount;
        public int TerrainDetailDecalCount;
        public int ShorelineDetailDecalCount;
        public int ResourceFields;
        public int ResourceVisualInstances;
        public int ResourceGlintCount;
        public int ResourceDustDecalCount;
        public int ResourceDepletedCount;
        public int ResourceRegeneratingCount;
        public int BasePadCount;
        public int BasePadDetailDecalCount;
        public int TreeCount;
        public int BushCount;
        public int GrassCount;
        public int RockCount;
        public int ScatterCount;
        public int SkippedPlacementCount;
        public int OrganicTransitionMeshCount;
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
            AppendMetric(builder, "mixed chunks", MixedTerrainChunks);
            AppendMetric(builder, "fallback roles", FallbackRoleCount);
            AppendMetric(builder, "transition edges", TransitionEdges);
            AppendMetric(builder, "water cells", WaterCells);
            AppendMetric(builder, "water strips", WaterStrips);
            AppendMetric(builder, "water meshes", WaterMeshes);
            AppendMetric(builder, "water surface detail decals", WaterSurfaceDetailDecalCount);
            AppendMetric(builder, "shoreline edges", ShorelineEdges);
            AppendMetric(builder, "shoreline meshes", ShorelineMeshes);
            AppendMetric(builder, "road segments", RoadSegments);
            AppendMetric(builder, "authored road segments", AuthoredRoadSegments);
            AppendMetric(builder, "fallback road segments", FallbackRoadSegments);
            AppendMetric(builder, "authored crossings", AuthoredCrossings);
            AppendMetric(builder, "generated road fallback", GeneratedRoadFallbacksUsed);
            AppendMetric(builder, "bridges", BridgeCrossings);
            AppendMetric(builder, "fords", FordCrossings);
            AppendMetric(builder, "road-water conflicts", RoadWaterConflicts);
            AppendMetric(builder, "road detail decals", RoadDetailDecalCount);
            AppendMetric(builder, "cliff straights", CliffStraightSegments);
            AppendMetric(builder, "cliff corners", CliffCorners);
            AppendMetric(builder, "cliff endcaps", CliffEndcaps);
            AppendMetric(builder, "hidden debug fill", HiddenDebugFillCount);
            AppendMetric(builder, "terrain detail decals", TerrainDetailDecalCount);
            AppendMetric(builder, "shoreline detail decals", ShorelineDetailDecalCount);
            AppendMetric(builder, "resource fields", ResourceFields);
            AppendMetric(builder, "resource visuals", ResourceVisualInstances);
            AppendMetric(builder, "resource glints", ResourceGlintCount);
            AppendMetric(builder, "resource dust", ResourceDustDecalCount);
            AppendMetric(builder, "depleted fields", ResourceDepletedCount);
            AppendMetric(builder, "regenerating fields", ResourceRegeneratingCount);
            AppendMetric(builder, "base pads", BasePadCount);
            AppendMetric(builder, "base pad decals", BasePadDetailDecalCount);
            AppendMetric(builder, "trees", TreeCount);
            AppendMetric(builder, "bushes", BushCount);
            AppendMetric(builder, "grass", GrassCount);
            AppendMetric(builder, "rocks", RockCount);
            AppendMetric(builder, "scatter", ScatterCount);
            AppendMetric(builder, "skipped", SkippedPlacementCount);
            AppendMetric(builder, "organic transition meshes", OrganicTransitionMeshCount);
            for (var i = 0; i < Warnings.Count; i++)
                builder.Append("; warning: ").Append(Warnings[i]);
            for (var i = 0; i < Errors.Count; i++)
                builder.Append("; error: ").Append(Errors[i]);
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
