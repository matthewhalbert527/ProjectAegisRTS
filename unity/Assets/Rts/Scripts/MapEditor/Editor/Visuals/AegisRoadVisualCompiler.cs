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
            var bridgeDetailMaterial = AegisVisualCompilerPrimitives.Material(context, "basepad.trim_decal");
            var bridgeShadowMaterial = AegisVisualCompilerPrimitives.Material(context, "decal.scorch");

            for (var i = 0; i < context.RoadSegments.Count; i++)
            {
                var segment = context.RoadSegments[i];
                var runs = SplitByWater(context, segment);
                for (var runIndex = 0; runIndex < runs.Count; runIndex++)
                {
                    var run = runs[runIndex];
                    if (run.IsWater)
                        EmitBridgeRun(context, layer, summary, i, runIndex, run, bridgeDeckMaterial, bridgeRailMaterial, bridgeDetailMaterial, bridgeShadowMaterial);
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
            var width = Mathf.Clamp(run.Width, 1.75f, 2.82f);

            AegisVisualCompilerPrimitives.CreateOrganicQuad(layer, "road_body_" + segmentIndex + "_" + runIndex, center, length, width, 0.055f, roadMaterial, angle, context, segmentIndex, runIndex, 7300, width * 0.14f);
            AegisVisualCompilerPrimitives.CreateOrganicQuad(layer, "road_soft_dust_" + segmentIndex + "_" + runIndex, center, length * 0.94f, width * 1.26f, 0.071f, dustMaterial, angle, context, segmentIndex, runIndex, 7310, width * 0.22f);
            AegisVisualCompilerPrimitives.CreateOrganicQuad(layer, "road_edge_wear_left_" + segmentIndex + "_" + runIndex, center + normal * (width * 0.48f), length * 0.88f, 0.38f, 0.082f, edgeMaterial, angle, context, segmentIndex, runIndex, 7200, 0.08f);
            AegisVisualCompilerPrimitives.CreateOrganicQuad(layer, "road_edge_wear_right_" + segmentIndex + "_" + runIndex, center - normal * (width * 0.48f), length * 0.88f, 0.38f, 0.082f, edgeMaterial, angle + 180f, context, segmentIndex, runIndex, 7210, 0.08f);
            summary.RoadDetailDecalCount += 3;
            if (length > 5f)
            {
                AegisVisualCompilerPrimitives.CreateOrganicQuad(layer, "road_tire_rut_left_" + segmentIndex + "_" + runIndex, center + normal * 0.48f, length * 0.74f, 0.16f, 0.091f, leftRutMaterial, angle, context, segmentIndex, runIndex, 7220, 0.04f);
                AegisVisualCompilerPrimitives.CreateOrganicQuad(layer, "road_tire_rut_right_" + segmentIndex + "_" + runIndex, center - normal * 0.48f, length * 0.74f, 0.16f, 0.091f, rightRutMaterial, angle, context, segmentIndex, runIndex, 7230, 0.04f);
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
                    AegisVisualCompilerPrimitives.CreateOrganicQuad(layer, "road_mud_track_" + segmentIndex + "_" + runIndex + "_" + i, p, patchLength, width * 0.72f, 0.096f, mudTrackMaterial, angle + Mathf.Lerp(-8f, 8f, context.Hash01(segmentIndex, runIndex, 7120 + i)), context, segmentIndex, runIndex, 7240 + i, 0.12f);
                    summary.RoadDetailDecalCount++;
                }
            }

            summary.RoadSegments++;
        }

        static void EmitBridgeRun(AegisMapVisualCompileContext context, Transform layer, AegisVisualLayerSummary summary, int segmentIndex, int runIndex, RoadRun run, Material deckMaterial, Material railMaterial, Material detailMaterial, Material shadowMaterial)
        {
            var length = AegisVisualCompilerPrimitives.SegmentLength(run.A, run.B);
            if (length <= 0.3f)
                return;

            var center = Vector2.Lerp(run.A, run.B, 0.5f);
            var angle = AegisVisualCompilerPrimitives.DirectionAngle(run.A, run.B);
            var direction = (run.B - run.A).normalized;
            var normal = new Vector2(-direction.y, direction.x);
            var deckWidth = Mathf.Clamp(run.Width + 0.62f, 2.5f, 4.2f);

            AegisVisualCompilerPrimitives.CreateOrganicQuad(layer, "bridge_shadow_" + segmentIndex + "_" + runIndex, center, length + 0.85f, deckWidth + 0.72f, 0.112f, shadowMaterial, angle, context, segmentIndex, runIndex, 7410, 0.08f);
            AegisVisualCompilerPrimitives.CreateQuad(layer, "bridge_prototype_deck_" + segmentIndex + "_" + runIndex, center, length + 0.55f, deckWidth, 0.18f, deckMaterial, angle);
            AegisVisualCompilerPrimitives.CreateQuad(layer, "bridge_prototype_rail_left_" + segmentIndex + "_" + runIndex, center + normal * (deckWidth * 0.48f), length + 0.48f, 0.18f, 0.31f, railMaterial, angle);
            AegisVisualCompilerPrimitives.CreateQuad(layer, "bridge_prototype_rail_right_" + segmentIndex + "_" + runIndex, center - normal * (deckWidth * 0.48f), length + 0.48f, 0.18f, 0.31f, railMaterial, angle);
            EmitBridgeDeckDetails(context, layer, summary, segmentIndex, runIndex, run, length, deckWidth, normal, angle, detailMaterial, railMaterial);
            summary.BridgeCrossings++;
        }

        static void EmitBridgeDeckDetails(AegisMapVisualCompileContext context, Transform layer, AegisVisualLayerSummary summary, int segmentIndex, int runIndex, RoadRun run, float length, float deckWidth, Vector2 normal, float angle, Material detailMaterial, Material railMaterial)
        {
            var seamCount = Mathf.Clamp(Mathf.FloorToInt(length / 2.6f), 1, 12);
            for (var i = 0; i < seamCount; i++)
            {
                var t = (i + 1f) / (seamCount + 1f);
                var point = Vector2.Lerp(run.A, run.B, t);
                AegisVisualCompilerPrimitives.CreateQuad(layer, "bridge_deck_seam_" + segmentIndex + "_" + runIndex + "_" + i, point, deckWidth * 0.86f, 0.045f, 0.205f, detailMaterial, angle + 90f);
                summary.RoadDetailDecalCount++;
            }

            var postCount = Mathf.Clamp(Mathf.FloorToInt(length / 3.2f), 2, 14);
            for (var i = 0; i < postCount; i++)
            {
                var t = (i + 0.5f) / postCount;
                var point = Vector2.Lerp(run.A, run.B, t);
                var left = point + normal * (deckWidth * 0.52f);
                var right = point - normal * (deckWidth * 0.52f);
                var rotation = Quaternion.Euler(0f, angle, 0f);
                AegisVisualCompilerPrimitives.CreateCube(layer, "bridge_post_left_" + segmentIndex + "_" + runIndex + "_" + i, new Vector3(left.x, 0.34f, left.y), new Vector3(0.16f, 0.34f, 0.16f), rotation, railMaterial);
                AegisVisualCompilerPrimitives.CreateCube(layer, "bridge_post_right_" + segmentIndex + "_" + runIndex + "_" + i, new Vector3(right.x, 0.34f, right.y), new Vector3(0.16f, 0.34f, 0.16f), rotation, railMaterial);
            }
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
