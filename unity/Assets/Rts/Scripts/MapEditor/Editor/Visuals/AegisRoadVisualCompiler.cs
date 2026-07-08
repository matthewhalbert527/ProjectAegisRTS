#if UNITY_EDITOR
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
            var rutMaterial = AegisVisualCompilerPrimitives.Material(context, "road.gravel");

            for (var i = 0; i < context.RoadSegments.Count; i++)
            {
                var segment = context.RoadSegments[i];
                var center = Vector2.Lerp(segment.A, segment.B, 0.5f);
                var length = AegisVisualCompilerPrimitives.SegmentLength(segment.A, segment.B);
                var angle = AegisVisualCompilerPrimitives.DirectionAngle(segment.A, segment.B);
                if (length <= 0.1f)
                    continue;

                AegisVisualCompilerPrimitives.CreateQuad(layer, "road_body_" + i, center, length, segment.Width, 0.055f, roadMaterial, angle);
                var direction = (segment.B - segment.A).normalized;
                var normal = new Vector2(-direction.y, direction.x);
                AegisVisualCompilerPrimitives.CreateQuad(layer, "road_left_rut_" + i, center + normal * 0.62f, length * 0.94f, 0.18f, 0.065f, rutMaterial, angle);
                AegisVisualCompilerPrimitives.CreateQuad(layer, "road_right_rut_" + i, center - normal * 0.62f, length * 0.94f, 0.18f, 0.065f, rutMaterial, angle);
                summary.RoadSegments++;
            }

            return summary;
        }
    }
}
#endif
