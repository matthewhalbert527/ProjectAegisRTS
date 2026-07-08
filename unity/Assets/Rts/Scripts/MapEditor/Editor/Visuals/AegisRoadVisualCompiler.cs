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
            var edgeGrassMaterial = AegisVisualCompilerPrimitives.Material(context, "road.edge_grass");
            var pebbleBreakupMaterial = AegisVisualCompilerPrimitives.Material(context, "road.pebble_breakup");
            var bridgeDeckMaterial = AegisVisualCompilerPrimitives.Material(context, "bridge.deck");
            var bridgeRailMaterial = AegisVisualCompilerPrimitives.Material(context, "bridge.rail");
            var bridgeDetailMaterial = AegisVisualCompilerPrimitives.Material(context, "bridge.grime");
            var bridgeShadowMaterial = AegisVisualCompilerPrimitives.Material(context, "decal.scorch");
            var bridgePebbleMaterial = AegisVisualCompilerPrimitives.Material(context, "river.bank_pebbles");
            var bridgeRubbleMaterial = AegisVisualCompilerPrimitives.Material(context, "decal.rubble");
            var bridgeGrassMaterial = AegisVisualCompilerPrimitives.Material(context, "road.edge_grass");
            var bridgeRockMaterial = AegisVisualCompilerPrimitives.Material(context, "blocker.rock");
            summary.AuthoredRoadSegments = context.AuthoredRoadSegmentCount;
            summary.FallbackRoadSegments = context.FallbackRoadSegmentCount;
            summary.AuthoredCrossings = context.AuthoredCrossingCount;
            summary.GeneratedRoadFallbacksUsed = context.UsesFallbackRoads ? 1 : 0;

            for (var i = 0; i < context.RoadSegments.Count; i++)
            {
                var segment = context.RoadSegments[i];
                var runs = SplitByWater(context, segment);
                for (var runIndex = 0; runIndex < runs.Count; runIndex++)
                {
                    var run = runs[runIndex];
                    if (run.IsWater)
                        EmitBridgeRun(context, layer, summary, i, runIndex, run, bridgeDeckMaterial, bridgeRailMaterial, bridgeDetailMaterial, bridgeShadowMaterial, roadMaterial, roadDustMaterial, roadEdgeMaterial, bridgePebbleMaterial, bridgeRubbleMaterial, bridgeGrassMaterial, bridgeRockMaterial);
                    else
                        EmitRoadRun(context, layer, summary, i, runIndex, run, roadMaterial, roadDustMaterial, roadEdgeMaterial, leftRutMaterial, rightRutMaterial, mudTrackMaterial, edgeGrassMaterial, pebbleBreakupMaterial);
                }
            }

            summary.AddWarning("Road plan: segments=" + context.RoadSegments.Count +
                ", authored=" + context.AuthoredRoadSegmentCount +
                ", fallback=" + context.FallbackRoadSegmentCount +
                ", generatedConnectivity=" + (context.GeneratedConnectivityRoadsUsed ? "yes" : "no") +
                ", authoredCrossings=" + context.AuthoredCrossingCount +
                ", bridges=" + summary.BridgeCrossings +
                ", fords=" + summary.FordCrossings +
                ", unbridgedWaterConflicts=" + summary.RoadWaterConflicts + ".");
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

        static void EmitRoadRun(AegisMapVisualCompileContext context, Transform layer, AegisVisualLayerSummary summary, int segmentIndex, int runIndex, RoadRun run, Material roadMaterial, Material dustMaterial, Material edgeMaterial, Material leftRutMaterial, Material rightRutMaterial, Material mudTrackMaterial, Material edgeGrassMaterial, Material pebbleBreakupMaterial)
        {
            var length = AegisVisualCompilerPrimitives.SegmentLength(run.A, run.B);
            if (length <= 0.3f)
                return;

            var width = Mathf.Clamp(run.Width * 0.54f, 0.92f, 1.60f);
            var dustWidth = Mathf.Clamp(run.Width * 0.76f, 1.25f, 2.30f);
            var path = BuildRoadPath(context, segmentIndex, runIndex, run, length);

            CreateRoadRibbonMesh(layer, "road_soft_dust_" + segmentIndex + "_" + runIndex, path, dustWidth, 0f, 0.052f, dustMaterial, context, segmentIndex, runIndex, 7310, 0.16f, 22f);
            CreateRoadRibbonMesh(layer, "road_body_" + segmentIndex + "_" + runIndex, path, width, 0f, 0.070f, roadMaterial, context, segmentIndex, runIndex, 7300, 0.10f, 10f);
            CreateRoadRibbonMesh(layer, "road_edge_wear_left_" + segmentIndex + "_" + runIndex, path, 0.26f, width * 0.56f, 0.084f, edgeMaterial, context, segmentIndex, runIndex, 7200, 0.18f, 16f);
            CreateRoadRibbonMesh(layer, "road_edge_wear_right_" + segmentIndex + "_" + runIndex, path, 0.26f, -width * 0.56f, 0.084f, edgeMaterial, context, segmentIndex, runIndex, 7210, 0.18f, 16f);
            summary.RoadDetailDecalCount += 4;
            if (length > 5f)
            {
                CreateRoadRibbonMesh(layer, "road_tire_rut_left_" + segmentIndex + "_" + runIndex, path, 0.13f, width * 0.26f, 0.093f, leftRutMaterial, context, segmentIndex, runIndex, 7220, 0.08f, 12f);
                CreateRoadRibbonMesh(layer, "road_tire_rut_right_" + segmentIndex + "_" + runIndex, path, 0.13f, -width * 0.26f, 0.093f, rightRutMaterial, context, segmentIndex, runIndex, 7230, 0.08f, 12f);
                summary.RoadDetailDecalCount += 2;
            }

            if (length > 8f)
            {
                var count = Mathf.Clamp(Mathf.FloorToInt(length / 14f), 1, 4);
                for (var i = 0; i < count; i++)
                {
                    var t = (i + 1f) / (count + 1f);
                    var sample = SampleRoadPath(path, t);
                    var p = sample.Center + sample.Normal * Mathf.Lerp(-0.35f, 0.35f, context.Hash01(segmentIndex, runIndex, 7100 + i));
                    var patchLength = Mathf.Lerp(1.4f, 3.6f, context.Hash01(segmentIndex, runIndex, 7110 + i));
                    var patchAngle = Mathf.Atan2(sample.Direction.y, sample.Direction.x) * Mathf.Rad2Deg + Mathf.Lerp(-8f, 8f, context.Hash01(segmentIndex, runIndex, 7120 + i));
                    AegisVisualCompilerPrimitives.CreateOrganicQuad(layer, "road_mud_track_" + segmentIndex + "_" + runIndex + "_" + i, p, patchLength, width * 0.62f, 0.096f, mudTrackMaterial, patchAngle, context, segmentIndex, runIndex, 7240 + i, 0.12f);
                    summary.RoadDetailDecalCount++;
                }
            }

            EmitRoadEdgeBreakup(context, layer, summary, segmentIndex, runIndex, path, width, length, edgeGrassMaterial, pebbleBreakupMaterial);
            summary.RoadSegments++;
        }

        static void EmitRoadEdgeBreakup(AegisMapVisualCompileContext context, Transform layer, AegisVisualLayerSummary summary, int segmentIndex, int runIndex, RoadPath path, float roadWidth, float length, Material edgeGrassMaterial, Material pebbleBreakupMaterial)
        {
            if (path.Points == null || path.Points.Length < 2 || length <= 3.5f)
                return;

            var edgePatchCount = Mathf.Clamp(Mathf.FloorToInt(length / 2.8f), 2, 24);
            for (var i = 0; i < edgePatchCount; i++)
            {
                var t = (i + 0.45f + context.Hash01(segmentIndex, runIndex, 7900 + i) * 0.35f) / edgePatchCount;
                var sample = SampleRoadPath(path, t);
                var angle = Mathf.Atan2(sample.Direction.y, sample.Direction.x) * Mathf.Rad2Deg + Mathf.Lerp(-16f, 16f, context.Hash01(segmentIndex, runIndex, 7908 + i));
                for (var sideIndex = 0; sideIndex < 2; sideIndex++)
                {
                    if (context.Hash01(segmentIndex + sideIndex, runIndex, 7916 + i) < 0.18f)
                        continue;

                    var side = sideIndex == 0 ? -1f : 1f;
                    var edgeCenter = sample.Center + sample.Normal * side * Mathf.Lerp(roadWidth * 0.38f, roadWidth * 0.72f, context.Hash01(segmentIndex + sideIndex, runIndex, 7924 + i));
                    var patchLength = Mathf.Lerp(1.15f, 3.35f, context.Hash01(segmentIndex + sideIndex, runIndex, 7932 + i));
                    var patchWidth = Mathf.Lerp(0.42f, 1.14f, context.Hash01(segmentIndex + sideIndex, runIndex, 7940 + i));

                    AegisVisualCompilerPrimitives.CreateOrganicQuad(layer, "road_edge_grass_intrusion_" + segmentIndex + "_" + runIndex + "_" + i + "_" + sideIndex, edgeCenter, patchLength, patchWidth, 0.101f, edgeGrassMaterial, angle, context, segmentIndex + sideIndex, runIndex, 7950 + i, 0.20f, 5.5f);
                    summary.RoadDetailDecalCount++;
                }

                if (context.Hash01(segmentIndex, runIndex, 7960 + i) > 0.58f)
                {
                    var pebbleSide = context.Hash01(segmentIndex, runIndex, 7964 + i) < 0.5f ? -1f : 1f;
                    var smallCenter = sample.Center + sample.Normal * pebbleSide * Mathf.Lerp(roadWidth * 0.34f, roadWidth * 0.56f, context.Hash01(segmentIndex, runIndex, 7968 + i));
                    var smallLength = Mathf.Lerp(0.60f, 1.55f, context.Hash01(segmentIndex, runIndex, 7976 + i));
                    var smallWidth = Mathf.Lerp(0.22f, 0.54f, context.Hash01(segmentIndex, runIndex, 7984 + i));
                    AegisVisualCompilerPrimitives.CreateOrganicQuad(layer, "road_edge_bare_pebble_" + segmentIndex + "_" + runIndex + "_" + i, smallCenter, smallLength, smallWidth, 0.102f, pebbleBreakupMaterial, angle + Mathf.Lerp(-22f, 22f, context.Hash01(segmentIndex, runIndex, 7992 + i)), context, segmentIndex, runIndex, 8000 + i, 0.14f, 4.5f);
                    summary.RoadDetailDecalCount++;
                }
            }

            var centerPatchCount = Mathf.Clamp(Mathf.FloorToInt(length / 6.5f), 1, 9);
            for (var i = 0; i < centerPatchCount; i++)
            {
                var t = (i + 1f) / (centerPatchCount + 1f);
                var sample = SampleRoadPath(path, t);
                var lateral = Mathf.Lerp(-roadWidth * 0.24f, roadWidth * 0.24f, context.Hash01(segmentIndex, runIndex, 8020 + i));
                var center = sample.Center + sample.Normal * lateral;
                var angle = Mathf.Atan2(sample.Direction.y, sample.Direction.x) * Mathf.Rad2Deg + Mathf.Lerp(-10f, 10f, context.Hash01(segmentIndex, runIndex, 8030 + i));
                var patchLength = Mathf.Lerp(0.75f, 2.15f, context.Hash01(segmentIndex, runIndex, 8040 + i));
                var patchWidth = Mathf.Lerp(0.34f, 0.76f, context.Hash01(segmentIndex, runIndex, 8050 + i));

                AegisVisualCompilerPrimitives.CreateOrganicQuad(layer, "road_surface_pebble_breakup_" + segmentIndex + "_" + runIndex + "_" + i, center, patchLength, patchWidth, 0.103f, pebbleBreakupMaterial, angle, context, segmentIndex, runIndex, 8060 + i, 0.16f, 4.0f);
                summary.RoadDetailDecalCount++;
            }
        }

        static RoadPath BuildRoadPath(AegisMapVisualCompileContext context, int segmentIndex, int runIndex, RoadRun run, float length)
        {
            var sections = Mathf.Clamp(Mathf.CeilToInt(length / 2.15f), 3, 32);
            var points = new Vector2[sections + 1];
            var distances = new float[sections + 1];
            var baseDirection = (run.B - run.A).normalized;
            var baseNormal = new Vector2(-baseDirection.y, baseDirection.x);
            var amplitude = Mathf.Clamp((length - 4f) * 0.028f, 0f, 0.46f);
            var phaseA = context.Hash01(segmentIndex, runIndex, 7800) * Mathf.PI * 2f;
            var phaseB = context.Hash01(segmentIndex, runIndex, 7810) * Mathf.PI * 2f;

            for (var i = 0; i <= sections; i++)
            {
                var t = i / (float)sections;
                var basePoint = Vector2.Lerp(run.A, run.B, t);
                var envelope = Mathf.Sin(t * Mathf.PI);
                var wave = Mathf.Sin(t * Mathf.PI * 2f + phaseA) * 0.62f + Mathf.Sin(t * Mathf.PI * 4f + phaseB) * 0.25f;
                var jitter = (context.Hash01(segmentIndex + i, runIndex, 7820) - 0.5f) * 0.18f;
                var candidate = basePoint + baseNormal * ((wave + jitter) * amplitude * envelope);
                points[i] = IsWaterAt(context, candidate) && !IsWaterAt(context, basePoint) ? basePoint : candidate;
                if (i > 0)
                    distances[i] = distances[i - 1] + Vector2.Distance(points[i - 1], points[i]);
            }

            return new RoadPath(points, distances, distances[sections]);
        }

        static void CreateRoadRibbonMesh(Transform parent, string name, RoadPath path, float width, float lateralOffset, float elevation, Material material, AegisMapVisualCompileContext context, int segmentIndex, int runIndex, int salt, float widthJitter, float uvWorldScale)
        {
            if (path.Points == null || path.Points.Length < 2)
                return;

            var count = path.Points.Length;
            var vertices = new Vector3[count * 2];
            var uvs = new Vector2[count * 2];
            for (var i = 0; i < count; i++)
            {
                var direction = RoadDirection(path, i);
                var normal = new Vector2(-direction.y, direction.x);
                var sectionWidth = Mathf.Max(0.04f, width * Mathf.Lerp(1f - widthJitter, 1f + widthJitter, context.Hash01(segmentIndex + i, runIndex, salt + 31)));
                var center = path.Points[i] + normal * lateralOffset;
                var halfWidth = sectionWidth * 0.5f;
                vertices[i * 2] = new Vector3(center.x - normal.x * halfWidth, elevation, center.y - normal.y * halfWidth);
                vertices[i * 2 + 1] = new Vector3(center.x + normal.x * halfWidth, elevation, center.y + normal.y * halfWidth);
                var v = path.Distances[i] / Mathf.Max(0.001f, uvWorldScale);
                uvs[i * 2] = new Vector2(0f, v);
                uvs[i * 2 + 1] = new Vector2(1f, v);
            }

            var triangles = new int[(count - 1) * 6];
            var triangle = 0;
            for (var i = 0; i < count - 1; i++)
            {
                var a = i * 2;
                var b = a + 1;
                var c = (i + 1) * 2;
                var d = c + 1;
                triangles[triangle++] = a;
                triangles[triangle++] = b;
                triangles[triangle++] = c;
                triangles[triangle++] = b;
                triangles[triangle++] = d;
                triangles[triangle++] = c;
            }

            var mesh = new Mesh();
            mesh.name = name + "_ribbon_mesh";
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
        }

        static RoadSample SampleRoadPath(RoadPath path, float t)
        {
            if (path.Points == null || path.Points.Length == 0)
                return new RoadSample(Vector2.zero, Vector2.right, Vector2.up);

            var target = Mathf.Clamp01(t) * path.TotalLength;
            for (var i = 1; i < path.Distances.Length; i++)
            {
                if (path.Distances[i] < target)
                    continue;

                var distance = Mathf.Max(0.001f, path.Distances[i] - path.Distances[i - 1]);
                var localT = Mathf.Clamp01((target - path.Distances[i - 1]) / distance);
                var center = Vector2.Lerp(path.Points[i - 1], path.Points[i], localT);
                var direction = (path.Points[i] - path.Points[i - 1]).normalized;
                if (direction.sqrMagnitude < 0.0001f)
                    direction = RoadDirection(path, i);
                var normal = new Vector2(-direction.y, direction.x);
                return new RoadSample(center, direction, normal);
            }

            var endDirection = RoadDirection(path, path.Points.Length - 1);
            return new RoadSample(path.Points[path.Points.Length - 1], endDirection, new Vector2(-endDirection.y, endDirection.x));
        }

        static Vector2 RoadDirection(RoadPath path, int index)
        {
            if (path.Points.Length == 1)
                return Vector2.right;

            if (index <= 0)
                return SafeDirection(path.Points[1] - path.Points[0]);
            if (index >= path.Points.Length - 1)
                return SafeDirection(path.Points[path.Points.Length - 1] - path.Points[path.Points.Length - 2]);

            return SafeDirection(path.Points[index + 1] - path.Points[index - 1]);
        }

        static Vector2 SafeDirection(Vector2 direction)
        {
            return direction.sqrMagnitude < 0.0001f ? Vector2.right : direction.normalized;
        }

        static void EmitBridgeRun(AegisMapVisualCompileContext context, Transform layer, AegisVisualLayerSummary summary, int segmentIndex, int runIndex, RoadRun run, Material deckMaterial, Material railMaterial, Material detailMaterial, Material shadowMaterial, Material roadMaterial, Material roadDustMaterial, Material roadEdgeMaterial, Material pebbleMaterial, Material rubbleMaterial, Material grassMaterial, Material rockMaterial)
        {
            var length = AegisVisualCompilerPrimitives.SegmentLength(run.A, run.B);
            if (length <= 0.3f)
                return;

            var center = Vector2.Lerp(run.A, run.B, 0.5f);
            var angle = AegisVisualCompilerPrimitives.DirectionAngle(run.A, run.B);
            var direction = (run.B - run.A).normalized;
            var normal = new Vector2(-direction.y, direction.x);
            var deckWidth = Mathf.Clamp(run.Width + 0.22f, 2.15f, 3.25f);

            AegisVisualCompilerPrimitives.CreateOrganicQuad(layer, "bridge_shadow_" + segmentIndex + "_" + runIndex, center, length + 0.85f, deckWidth + 0.72f, 0.112f, shadowMaterial, angle, context, segmentIndex, runIndex, 7410, 0.08f);
            EmitBridgeApproachRoadBlend(context, layer, summary, segmentIndex, runIndex, run, length, deckWidth, direction, normal, angle, roadMaterial, roadDustMaterial, roadEdgeMaterial);
            EmitBridgeAbutments(context, layer, summary, segmentIndex, runIndex, run, deckWidth, direction, normal, angle, railMaterial, shadowMaterial);
            AegisVisualCompilerPrimitives.CreateOrganicQuad(layer, "bridge_deck_" + segmentIndex + "_" + runIndex, center, length + 0.48f, deckWidth, 0.18f, deckMaterial, angle, context, segmentIndex, runIndex, 7420, 0.035f, 14f);
            EmitBridgeSideRails(context, layer, segmentIndex, runIndex, run, length, deckWidth, normal, angle, railMaterial);
            EmitBridgeApproachDust(context, layer, summary, segmentIndex, runIndex, run, deckWidth, normal, angle, shadowMaterial);
            EmitBridgeDeckDetails(context, layer, summary, segmentIndex, runIndex, run, length, deckWidth, normal, angle, detailMaterial, railMaterial);
            EmitBridgeContactDressing(context, layer, summary, segmentIndex, runIndex, run, length, deckWidth, direction, normal, angle, roadDustMaterial, detailMaterial, pebbleMaterial, rubbleMaterial, grassMaterial, rockMaterial);
            summary.BridgeCrossings++;
        }

        static void EmitBridgeAbutments(AegisMapVisualCompileContext context, Transform layer, AegisVisualLayerSummary summary, int segmentIndex, int runIndex, RoadRun run, float deckWidth, Vector2 direction, Vector2 normal, float angle, Material abutmentMaterial, Material shadowMaterial)
        {
            var rotation = Quaternion.Euler(0f, angle, 0f);
            EmitBridgeAbutmentEnd(context, layer, summary, segmentIndex, runIndex, run.A - direction * 0.16f, direction, normal, deckWidth, angle, rotation, abutmentMaterial, shadowMaterial, "entry", 7520);
            EmitBridgeAbutmentEnd(context, layer, summary, segmentIndex, runIndex, run.B + direction * 0.16f, -direction, normal, deckWidth, angle, rotation, abutmentMaterial, shadowMaterial, "exit", 7530);
        }

        static void EmitBridgeAbutmentEnd(AegisMapVisualCompileContext context, Transform layer, AegisVisualLayerSummary summary, int segmentIndex, int runIndex, Vector2 center, Vector2 awayFromBridge, Vector2 normal, float deckWidth, float angle, Quaternion rotation, Material abutmentMaterial, Material shadowMaterial, string suffix, int salt)
        {
            var padWidth = deckWidth + Mathf.Lerp(0.72f, 1.18f, context.Hash01(segmentIndex, runIndex, salt + 1));
            var padLength = Mathf.Lerp(0.56f, 0.88f, context.Hash01(segmentIndex, runIndex, salt + 2));
            var blockCenter = center + awayFromBridge * 0.18f;
            AegisVisualCompilerPrimitives.CreateCube(layer, "bridge_abutment_" + suffix + "_" + segmentIndex + "_" + runIndex, new Vector3(blockCenter.x, 0.155f, blockCenter.y), new Vector3(padLength, 0.22f, padWidth), rotation, abutmentMaterial);

            var apronCenter = center + awayFromBridge * 0.62f;
            AegisVisualCompilerPrimitives.CreateOrganicQuad(layer, "bridge_abutment_wet_shadow_" + suffix + "_" + segmentIndex + "_" + runIndex, apronCenter, padLength * 1.85f, padWidth + 0.46f, 0.108f, shadowMaterial, angle, context, segmentIndex, runIndex, salt + 3, 0.10f);
            var left = center + normal * (padWidth * 0.48f) + awayFromBridge * 0.36f;
            var right = center - normal * (padWidth * 0.48f) + awayFromBridge * 0.36f;
            AegisVisualCompilerPrimitives.CreateOrganicQuad(layer, "bridge_abutment_side_wear_left_" + suffix + "_" + segmentIndex + "_" + runIndex, left, padLength * 1.28f, 0.32f, 0.119f, shadowMaterial, angle, context, segmentIndex, runIndex, salt + 4, 0.10f);
            AegisVisualCompilerPrimitives.CreateOrganicQuad(layer, "bridge_abutment_side_wear_right_" + suffix + "_" + segmentIndex + "_" + runIndex, right, padLength * 1.28f, 0.32f, 0.119f, shadowMaterial, angle, context, segmentIndex, runIndex, salt + 5, 0.10f);
            summary.RoadDetailDecalCount += 3;
        }

        static void EmitBridgeSideRails(AegisMapVisualCompileContext context, Transform layer, int segmentIndex, int runIndex, RoadRun run, float length, float deckWidth, Vector2 normal, float angle, Material railMaterial)
        {
            var rotation = Quaternion.Euler(0f, angle, 0f);
            var segmentCount = Mathf.Clamp(Mathf.CeilToInt(length / 2.75f), 1, 12);
            var step = length / segmentCount;
            var railLength = Mathf.Max(0.72f, step * 0.68f);
            for (var i = 0; i < segmentCount; i++)
            {
                var t = (i + 0.5f) / segmentCount;
                var point = Vector2.Lerp(run.A, run.B, t);
                var stagger = (context.Hash01(segmentIndex + i, runIndex, 7540) - 0.5f) * 0.08f;
                var railHeight = Mathf.Lerp(0.105f, 0.150f, context.Hash01(segmentIndex + i, runIndex, 7541));
                var railThickness = Mathf.Lerp(0.105f, 0.150f, context.Hash01(segmentIndex + i, runIndex, 7542));
                var left = point + normal * (deckWidth * 0.53f + stagger);
                var right = point - normal * (deckWidth * 0.53f + stagger);
                AegisVisualCompilerPrimitives.CreateCube(layer, "bridge_rail_section_left_" + segmentIndex + "_" + runIndex + "_" + i, new Vector3(left.x, 0.285f, left.y), new Vector3(railLength, railHeight, railThickness), rotation, railMaterial);
                AegisVisualCompilerPrimitives.CreateCube(layer, "bridge_rail_section_right_" + segmentIndex + "_" + runIndex + "_" + i, new Vector3(right.x, 0.285f, right.y), new Vector3(railLength, railHeight, railThickness), rotation, railMaterial);
            }
        }

        static void EmitBridgeApproachRoadBlend(AegisMapVisualCompileContext context, Transform layer, AegisVisualLayerSummary summary, int segmentIndex, int runIndex, RoadRun run, float bridgeLength, float deckWidth, Vector2 direction, Vector2 normal, float angle, Material roadMaterial, Material dustMaterial, Material edgeMaterial)
        {
            var heavyApron = bridgeLength >= 5.5f;
            EmitBridgeApproachRoadBlendEnd(context, layer, summary, segmentIndex, runIndex, run.A - direction * 2.25f, run.A + direction * 0.16f, deckWidth, direction, roadMaterial, dustMaterial, edgeMaterial, true, heavyApron);
            EmitBridgeApproachRoadBlendEnd(context, layer, summary, segmentIndex, runIndex, run.B + direction * 2.25f, run.B - direction * 0.16f, deckWidth, -direction, roadMaterial, dustMaterial, edgeMaterial, false, heavyApron);
        }

        static void EmitBridgeApproachRoadBlendEnd(AegisMapVisualCompileContext context, Transform layer, AegisVisualLayerSummary summary, int segmentIndex, int runIndex, Vector2 landPoint, Vector2 bridgePoint, float deckWidth, Vector2 directionToBridge, Material roadMaterial, Material dustMaterial, Material edgeMaterial, bool entry, bool heavyApron)
        {
            var nameSuffix = entry ? "entry" : "exit";
            var salt = entry ? 7480 : 7490;
            var roadWidth = deckWidth * Mathf.Lerp(0.56f, 0.74f, context.Hash01(segmentIndex, runIndex, salt + 1));
            var dustWidth = deckWidth * Mathf.Lerp(0.84f, 1.04f, context.Hash01(segmentIndex, runIndex, salt + 2));
            var bridgeWidth = deckWidth * (heavyApron ? 0.70f : 0.46f);
            var dustBridgeWidth = deckWidth * (heavyApron ? 0.98f : 0.74f);

            CreateTaperedApproachRibbon(layer, "bridge_road_dust_" + nameSuffix + "_" + segmentIndex + "_" + runIndex, landPoint, bridgePoint, directionToBridge, dustWidth, dustBridgeWidth, 0.107f, dustMaterial, context, segmentIndex, runIndex, salt + 3, 0.16f, 30f);
            if (heavyApron)
                CreateTaperedApproachRibbon(layer, "bridge_road_apron_" + nameSuffix + "_" + segmentIndex + "_" + runIndex, landPoint, bridgePoint, directionToBridge, roadWidth, bridgeWidth, 0.116f, roadMaterial, context, segmentIndex, runIndex, salt + 4, 0.12f, 26f);
            CreateTaperedApproachRibbon(layer, "bridge_road_edge_left_" + nameSuffix + "_" + segmentIndex + "_" + runIndex, landPoint, bridgePoint, directionToBridge, 0.18f, 0.22f, 0.125f, edgeMaterial, context, segmentIndex, runIndex, salt + 5, 0.08f, 12f, roadWidth * 0.42f);
            CreateTaperedApproachRibbon(layer, "bridge_road_edge_right_" + nameSuffix + "_" + segmentIndex + "_" + runIndex, landPoint, bridgePoint, directionToBridge, 0.18f, 0.22f, 0.125f, edgeMaterial, context, segmentIndex, runIndex, salt + 6, 0.08f, 12f, -roadWidth * 0.42f);
            summary.RoadDetailDecalCount += heavyApron ? 4 : 3;
        }

        static void CreateTaperedApproachRibbon(Transform parent, string name, Vector2 landPoint, Vector2 bridgePoint, Vector2 directionToBridge, float landWidth, float bridgeWidth, float elevation, Material material, AegisMapVisualCompileContext context, int segmentIndex, int runIndex, int salt, float widthJitter, float uvWorldScale, float lateralOffset = 0f)
        {
            var direction = SafeDirection(directionToBridge);
            var normal = new Vector2(-direction.y, direction.x);
            const int sections = 4;
            var vertices = new Vector3[(sections + 1) * 2];
            var uvs = new Vector2[(sections + 1) * 2];
            var totalLength = Vector2.Distance(landPoint, bridgePoint);

            for (var i = 0; i <= sections; i++)
            {
                var t = i / (float)sections;
                var center = Vector2.Lerp(landPoint, bridgePoint, t);
                var fade = Mathf.SmoothStep(0f, 1f, t);
                var width = Mathf.Lerp(landWidth, bridgeWidth, fade);
                width *= Mathf.Lerp(1f - widthJitter, 1f + widthJitter, context.Hash01(segmentIndex + i, runIndex, salt + 21));
                var centerOffset = normal * (lateralOffset * Mathf.Lerp(1f, 0.72f, fade));
                center += centerOffset;
                var side = normal * (width * 0.5f);
                vertices[i * 2] = new Vector3(center.x - side.x, elevation, center.y - side.y);
                vertices[i * 2 + 1] = new Vector3(center.x + side.x, elevation, center.y + side.y);
                var v = totalLength * t / Mathf.Max(0.001f, uvWorldScale);
                uvs[i * 2] = new Vector2(0f, v);
                uvs[i * 2 + 1] = new Vector2(1f, v);
            }

            var triangles = new int[sections * 6];
            var triangle = 0;
            for (var i = 0; i < sections; i++)
            {
                var a = i * 2;
                var b = a + 1;
                var c = (i + 1) * 2;
                var d = c + 1;
                triangles[triangle++] = a;
                triangles[triangle++] = b;
                triangles[triangle++] = c;
                triangles[triangle++] = b;
                triangles[triangle++] = d;
                triangles[triangle++] = c;
            }

            var mesh = new Mesh();
            mesh.name = name + "_tapered_mesh";
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
        }

        static void EmitBridgeApproachDust(AegisMapVisualCompileContext context, Transform layer, AegisVisualLayerSummary summary, int segmentIndex, int runIndex, RoadRun run, float deckWidth, Vector2 normal, float angle, Material material)
        {
            var direction = (run.B - run.A).normalized;
            var offset = Mathf.Min(1.25f, AegisVisualCompilerPrimitives.SegmentLength(run.A, run.B) * 0.18f);
            var a = run.A - direction * offset;
            var b = run.B + direction * offset;
            AegisVisualCompilerPrimitives.CreateOrganicQuad(layer, "bridge_approach_shadow_a_" + segmentIndex + "_" + runIndex, a, 2.1f, deckWidth + 0.55f, 0.106f, material, angle, context, segmentIndex, runIndex, 7440, 0.16f);
            AegisVisualCompilerPrimitives.CreateOrganicQuad(layer, "bridge_approach_shadow_b_" + segmentIndex + "_" + runIndex, b, 2.1f, deckWidth + 0.55f, 0.106f, material, angle, context, segmentIndex, runIndex, 7450, 0.16f);
            summary.RoadDetailDecalCount += 2;
        }

        static void EmitBridgeDeckDetails(AegisMapVisualCompileContext context, Transform layer, AegisVisualLayerSummary summary, int segmentIndex, int runIndex, RoadRun run, float length, float deckWidth, Vector2 normal, float angle, Material detailMaterial, Material railMaterial)
        {
            var center = Vector2.Lerp(run.A, run.B, 0.5f);
            AegisVisualCompilerPrimitives.CreateOrganicQuad(layer, "bridge_deck_grime_left_" + segmentIndex + "_" + runIndex, center + normal * (deckWidth * 0.20f), length * 0.82f, 0.22f, 0.207f, detailMaterial, angle, context, segmentIndex, runIndex, 7460, 0.07f, 18f);
            AegisVisualCompilerPrimitives.CreateOrganicQuad(layer, "bridge_deck_grime_right_" + segmentIndex + "_" + runIndex, center - normal * (deckWidth * 0.20f), length * 0.82f, 0.22f, 0.207f, detailMaterial, angle, context, segmentIndex, runIndex, 7470, 0.07f, 18f);
            AegisVisualCompilerPrimitives.CreateOrganicQuad(layer, "bridge_deck_edge_wear_left_" + segmentIndex + "_" + runIndex, center + normal * (deckWidth * 0.42f), length * 0.88f, 0.18f, 0.209f, detailMaterial, angle, context, segmentIndex, runIndex, 7475, 0.06f, 12f);
            AegisVisualCompilerPrimitives.CreateOrganicQuad(layer, "bridge_deck_edge_wear_right_" + segmentIndex + "_" + runIndex, center - normal * (deckWidth * 0.42f), length * 0.88f, 0.18f, 0.209f, detailMaterial, angle, context, segmentIndex, runIndex, 7476, 0.06f, 12f);
            summary.RoadDetailDecalCount += 4;

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

        static void EmitBridgeContactDressing(AegisMapVisualCompileContext context, Transform layer, AegisVisualLayerSummary summary, int segmentIndex, int runIndex, RoadRun run, float length, float deckWidth, Vector2 direction, Vector2 normal, float angle, Material dustMaterial, Material grimeMaterial, Material pebbleMaterial, Material rubbleMaterial, Material grassMaterial, Material rockMaterial)
        {
            var center = Vector2.Lerp(run.A, run.B, 0.5f);
            var centerDustWidth = Mathf.Max(0.38f, deckWidth * 0.46f);
            AegisVisualCompilerPrimitives.CreateOrganicQuad(layer, "bridge_deck_tracked_dust_" + segmentIndex + "_" + runIndex, center, length * 0.88f, centerDustWidth, 0.213f, dustMaterial, angle, context, segmentIndex, runIndex, 7580, 0.10f, 8f);
            AegisVisualCompilerPrimitives.CreateOrganicQuad(layer, "bridge_deck_wet_grime_" + segmentIndex + "_" + runIndex, center - direction * (length * 0.18f), length * 0.36f, deckWidth * 0.60f, 0.216f, grimeMaterial, angle, context, segmentIndex, runIndex, 7586, 0.08f, 6f);
            summary.RoadDetailDecalCount += 2;

            EmitBridgeContactEnd(context, layer, summary, segmentIndex, runIndex, run.A, -direction, normal, deckWidth, angle, pebbleMaterial, rubbleMaterial, grassMaterial, rockMaterial, "entry", 7590);
            EmitBridgeContactEnd(context, layer, summary, segmentIndex, runIndex, run.B, direction, normal, deckWidth, angle, pebbleMaterial, rubbleMaterial, grassMaterial, rockMaterial, "exit", 7620);
        }

        static void EmitBridgeContactEnd(AegisMapVisualCompileContext context, Transform layer, AegisVisualLayerSummary summary, int segmentIndex, int runIndex, Vector2 bridgeEnd, Vector2 awayFromBridge, Vector2 normal, float deckWidth, float angle, Material pebbleMaterial, Material rubbleMaterial, Material grassMaterial, Material rockMaterial, string suffix, int salt)
        {
            var bankCenter = bridgeEnd + awayFromBridge * 0.45f;
            AegisVisualCompilerPrimitives.CreateOrganicQuad(layer, "bridge_bank_pebble_fan_" + suffix + "_" + segmentIndex + "_" + runIndex, bankCenter, deckWidth * 1.22f, 1.15f, 0.131f, pebbleMaterial, angle, context, segmentIndex, runIndex, salt, 0.20f, 4.0f);
            AegisVisualCompilerPrimitives.CreateOrganicQuad(layer, "bridge_bank_rubble_shadow_" + suffix + "_" + segmentIndex + "_" + runIndex, bankCenter + awayFromBridge * 0.28f, deckWidth * 1.05f, 0.72f, 0.134f, rubbleMaterial, angle, context, segmentIndex, runIndex, salt + 1, 0.16f, 4.0f);
            summary.RoadDetailDecalCount += 2;

            for (var sideIndex = 0; sideIndex < 2; sideIndex++)
            {
                var side = sideIndex == 0 ? -1f : 1f;
                var sideCenter = bridgeEnd + awayFromBridge * Mathf.Lerp(0.25f, 0.92f, context.Hash01(segmentIndex + sideIndex, runIndex, salt + 2)) + normal * side * Mathf.Lerp(deckWidth * 0.52f, deckWidth * 0.90f, context.Hash01(segmentIndex + sideIndex, runIndex, salt + 3));
                var grassWidth = Mathf.Lerp(0.38f, 0.82f, context.Hash01(segmentIndex + sideIndex, runIndex, salt + 4));
                var grassLength = Mathf.Lerp(0.75f, 1.85f, context.Hash01(segmentIndex + sideIndex, runIndex, salt + 5));
                AegisVisualCompilerPrimitives.CreateOrganicQuad(layer, "bridge_bank_grass_clump_" + suffix + "_" + segmentIndex + "_" + runIndex + "_" + sideIndex, sideCenter, grassWidth, grassLength, 0.136f, grassMaterial, angle + Mathf.Lerp(-28f, 28f, context.Hash01(segmentIndex + sideIndex, runIndex, salt + 6)), context, segmentIndex + sideIndex, runIndex, salt + 7, 0.18f, 3.5f);
                summary.RoadDetailDecalCount++;

                EmitBridgeContactProp(context, layer, summary, segmentIndex + sideIndex, runIndex, sideCenter + normal * side * 0.22f, AegisMapArtPack.PebbleMeshes, rockMaterial, suffix + "_bank_" + sideIndex, salt + 10 + sideIndex);
            }
        }

        static void EmitBridgeContactProp(AegisMapVisualCompileContext context, Transform layer, AegisVisualLayerSummary summary, int x, int y, Vector2 center, string[] meshCandidates, Material fallbackMaterial, string nameSuffix, int salt)
        {
            if (context.Hash01(x, y, salt) > 0.86f)
                return;

            var prefab = AegisMapArtPack.Pick(meshCandidates, context.Seed, x + salt, y);
            var position = new Vector3(center.x + Mathf.Lerp(-0.18f, 0.18f, context.Hash01(x, y, salt + 1)), 0.115f, center.y + Mathf.Lerp(-0.18f, 0.18f, context.Hash01(x, y, salt + 2)));
            var rotation = Quaternion.Euler(0f, context.Hash01(x, y, salt + 3) * 360f, 0f);
            var scale = Vector3.one * Mathf.Lerp(0.28f, 0.54f, context.Hash01(x, y, salt + 4));
            if (AegisMapArtPack.TryInstantiatePrefab(layer, "bridge_contact_prop_" + nameSuffix + "_" + x + "_" + y, prefab, position, rotation, scale, fallbackMaterial))
                summary.ScatterCount++;
            else
                summary.SkippedPlacementCount++;
        }

        static bool IsWaterAt(AegisMapVisualCompileContext context, Vector2 point)
        {
            var x = Mathf.Clamp(Mathf.FloorToInt(point.x), 0, context.Width - 1);
            var y = Mathf.Clamp(Mathf.FloorToInt(point.y), 0, context.Height - 1);
            return context.IsWater(x, y);
        }

        struct RoadPath
        {
            public readonly Vector2[] Points;
            public readonly float[] Distances;
            public readonly float TotalLength;

            public RoadPath(Vector2[] points, float[] distances, float totalLength)
            {
                Points = points;
                Distances = distances;
                TotalLength = totalLength;
            }
        }

        struct RoadSample
        {
            public readonly Vector2 Center;
            public readonly Vector2 Direction;
            public readonly Vector2 Normal;

            public RoadSample(Vector2 center, Vector2 direction, Vector2 normal)
            {
                Center = center;
                Direction = direction;
                Normal = normal;
            }
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
