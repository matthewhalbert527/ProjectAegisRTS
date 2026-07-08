#if UNITY_EDITOR
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.MapEditor.Visuals;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    sealed class AegisRoadVisualCompiler : IAegisVisualLayerCompiler
    {
        public AegisVisualLayerSummary Compile(AegisMapVisualCompileContext context)
        {
            var summary = new AegisVisualLayerSummary("Roads And Tire Tracks");
            var layer = AegisVisualCompilerPrimitives.CreateLayer(context, "Roads And Tire Tracks");
            var roadMaterial = AegisVisualCompilerPrimitives.Material(context, "road.dirt");
            var roadDustMaterial = AegisVisualCompilerPrimitives.Material(context, "road.soft_dust");
            var roadEdgeMaterial = AegisVisualCompilerPrimitives.Material(context, "road.worn_edge");
            var leftRutMaterial = AegisVisualCompilerPrimitives.Material(context, "road.tire_left");
            var rightRutMaterial = AegisVisualCompilerPrimitives.Material(context, "road.tire_right");
            var mudTrackMaterial = AegisVisualCompilerPrimitives.Material(context, "road.mud_track");
            var bridgeDeckMaterial = AegisVisualCompilerPrimitives.Material(context, "basepad.panel");
            var bridgeRailMaterial = AegisVisualCompilerPrimitives.Material(context, "basepad.trim");

            for (var i = 0; i < context.RoadSegments.Count; i++)
            {
                var segment = context.RoadSegments[i];
                var runs = SplitByWater(context, segment);
                for (var runIndex = 0; runIndex < runs.Count; runIndex++)
                {
                    var run = runs[runIndex];
                    if (run.IsWater)
                        EmitBridgeRun(context, layer, summary, i, runIndex, run, bridgeDeckMaterial, bridgeRailMaterial);
                    else
                        EmitRoadRun(context, layer, summary, i, runIndex, run, roadMaterial, roadDustMaterial, roadEdgeMaterial, leftRutMaterial, rightRutMaterial, mudTrackMaterial);
                }
            }

            return summary;
        }

        static List<RoadRun> SplitByWater(AegisMapVisualCompileContext context, AegisVisualPathSegment segment)
        {
            var runs = new List<RoadRun>();
            var length = AegisVisualCompilerPrimitives.SegmentLength(segment.A, segment.B);
            if (length <= 0.1f)
                return runs;

            var steps = Mathf.Max(2, Mathf.CeilToInt(length / 0.75f));
            var runStart = segment.A;
            var previousWater = IsWaterAt(context, segment.A);
            for (var step = 1; step <= steps; step++)
            {
                var t = step / (float)steps;
                var point = Vector2.Lerp(segment.A, segment.B, t);
                var water = IsWaterAt(context, point);
                if (water == previousWater)
                    continue;

                runs.Add(new RoadRun(runStart, point, segment.Width, previousWater));
                runStart = point;
                previousWater = water;
            }

            runs.Add(new RoadRun(runStart, segment.B, segment.Width, previousWater));
            return runs;
        }

        static void EmitRoadRun(AegisMapVisualCompileContext context, Transform layer, AegisVisualLayerSummary summary, int segmentIndex, int runIndex, RoadRun run, Material roadMaterial, Material dustMaterial, Material edgeMaterial, Material leftRutMaterial, Material rightRutMaterial, Material mudTrackMaterial)
        {
            var length = AegisVisualCompilerPrimitives.SegmentLength(run.A, run.B);
            if (length <= 0.3f)
                return;

            var center = Vector2.Lerp(run.A, run.B, 0.5f);
            var angle = AegisVisualCompilerPrimitives.DirectionAngle(run.A, run.B);
            var direction = (run.B - run.A).normalized;
            var normal = new Vector2(-direction.y, direction.x);
            var width = Mathf.Clamp(run.Width, 1.85f, 3.35f);

            AegisVisualCompilerPrimitives.CreateQuad(layer, "road_body_" + segmentIndex + "_" + runIndex, center, length, width, 0.055f, roadMaterial, angle);
            AegisVisualCompilerPrimitives.CreateQuad(layer, "road_soft_dust_" + segmentIndex + "_" + runIndex, center, length * 0.96f, width * 1.22f, 0.071f, dustMaterial, angle);
            AegisVisualCompilerPrimitives.CreateQuad(layer, "road_edge_wear_left_" + segmentIndex + "_" + runIndex, center + normal * (width * 0.48f), length * 0.94f, 0.42f, 0.082f, edgeMaterial, angle);
            AegisVisualCompilerPrimitives.CreateQuad(layer, "road_edge_wear_right_" + segmentIndex + "_" + runIndex, center - normal * (width * 0.48f), length * 0.94f, 0.42f, 0.082f, edgeMaterial, angle + 180f);
            summary.RoadDetailDecalCount += 3;
            if (length > 5f)
            {
                AegisVisualCompilerPrimitives.CreateQuad(layer, "road_tire_rut_left_" + segmentIndex + "_" + runIndex, center + normal * 0.48f, length * 0.78f, 0.18f, 0.091f, leftRutMaterial, angle);
                AegisVisualCompilerPrimitives.CreateQuad(layer, "road_tire_rut_right_" + segmentIndex + "_" + runIndex, center - normal * 0.48f, length * 0.78f, 0.18f, 0.091f, rightRutMaterial, angle);
                summary.RoadDetailDecalCount += 2;
            }

            if (length > 8f)
            {
                var count = Mathf.Clamp(Mathf.FloorToInt(length / 14f), 1, 4);
                for (var i = 0; i < count; i++)
                {
                    var t = (i + 1f) / (count + 1f);
                    var p = Vector2.Lerp(run.A, run.B, t) + normal * Mathf.Lerp(-0.35f, 0.35f, context.Hash01(segmentIndex, runIndex, 7100 + i));
                    var patchLength = Mathf.Lerp(1.4f, 3.6f, context.Hash01(segmentIndex, runIndex, 7110 + i));
                    AegisVisualCompilerPrimitives.CreateQuad(layer, "road_mud_track_" + segmentIndex + "_" + runIndex + "_" + i, p, patchLength, width * 0.72f, 0.096f, mudTrackMaterial, angle + Mathf.Lerp(-8f, 8f, context.Hash01(segmentIndex, runIndex, 7120 + i)));
                    summary.RoadDetailDecalCount++;
                }
            }

            summary.RoadSegments++;
        }

        static void EmitBridgeRun(AegisMapVisualCompileContext context, Transform layer, AegisVisualLayerSummary summary, int segmentIndex, int runIndex, RoadRun run, Material deckMaterial, Material railMaterial)
        {
            var length = AegisVisualCompilerPrimitives.SegmentLength(run.A, run.B);
            if (length <= 0.3f)
                return;

            var center = Vector2.Lerp(run.A, run.B, 0.5f);
            var angle = AegisVisualCompilerPrimitives.DirectionAngle(run.A, run.B);
            var direction = (run.B - run.A).normalized;
            var normal = new Vector2(-direction.y, direction.x);
            var deckWidth = Mathf.Clamp(run.Width + 0.62f, 2.5f, 4.2f);

            AegisVisualCompilerPrimitives.CreateQuad(layer, "bridge_prototype_deck_" + segmentIndex + "_" + runIndex, center, length + 0.55f, deckWidth, 0.18f, deckMaterial, angle);
            AegisVisualCompilerPrimitives.CreateQuad(layer, "bridge_prototype_rail_left_" + segmentIndex + "_" + runIndex, center + normal * (deckWidth * 0.48f), length + 0.48f, 0.18f, 0.31f, railMaterial, angle);
            AegisVisualCompilerPrimitives.CreateQuad(layer, "bridge_prototype_rail_right_" + segmentIndex + "_" + runIndex, center - normal * (deckWidth * 0.48f), length + 0.48f, 0.18f, 0.31f, railMaterial, angle);
            summary.BridgeCrossings++;
        }

        static bool IsWaterAt(AegisMapVisualCompileContext context, Vector2 point)
        {
            var x = Mathf.Clamp(Mathf.FloorToInt(point.x), 0, context.Width - 1);
            var y = Mathf.Clamp(Mathf.FloorToInt(point.y), 0, context.Height - 1);
            return context.IsWater(x, y);
        }

        struct RoadRun
        {
            public readonly Vector2 A;
            public readonly Vector2 B;
            public readonly float Width;
            public readonly bool IsWater;

            public RoadRun(Vector2 a, Vector2 b, float width, bool isWater)
            {
                A = a;
                B = b;
                Width = width;
                IsWater = isWater;
            }
        }
    }
}
#endif
