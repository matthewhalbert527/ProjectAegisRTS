#if UNITY_EDITOR
using ProjectAegisRTS.UnityClient.MapEditor.Visuals;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    sealed class AegisTerrainTransitionCompiler : IAegisVisualLayerCompiler
    {
        public AegisVisualLayerSummary Compile(AegisMapVisualCompileContext context)
        {
            var summary = new AegisVisualLayerSummary("Terrain Transition Masks");
            var layer = AegisVisualCompilerPrimitives.CreateLayer(context, "Terrain Transition Masks");

            for (var y = 0; y < context.Height; y++)
                for (var x = 0; x < context.Width; x++)
                {
                    EmitTransition(context, layer, summary, x, y, 1, 0);
                    EmitTransition(context, layer, summary, x, y, 0, 1);
                }

            return summary;
        }

        static void EmitTransition(AegisMapVisualCompileContext context, Transform layer, AegisVisualLayerSummary summary, int x, int y, int dx, int dy)
        {
            var nx = x + dx;
            var ny = y + dy;
            if (!context.InBounds(nx, ny))
                return;

            var a = context.TerrainRoleAt(x, y);
            var b = context.TerrainRoleAt(nx, ny);
            if (a == b)
                return;

            if (a == "terrain.shallow_water" || a == "terrain.deep_water" || b == "terrain.shallow_water" || b == "terrain.deep_water")
                return;

            var role = TransitionRole(a, b);
            var material = AegisVisualCompilerPrimitives.Material(context, role);
            var center = new Vector2(x + 0.5f + dx * 0.5f, y + 0.5f + dy * 0.5f);
            var width = dx == 0 ? 1.12f : 0.32f;
            var height = dy == 0 ? 1.12f : 0.32f;
            AegisVisualCompilerPrimitives.CreateQuad(layer, "transition_" + x + "_" + y + "_" + nx + "_" + ny, center, width, height, 0.012f, material, 0f);
            summary.TransitionEdges++;
        }

        static string TransitionRole(string a, string b)
        {
            if (a == "terrain.concrete_base_pad" || b == "terrain.concrete_base_pad")
                return "terrain.dirt";
            if (a == "terrain.cliff_ground" || b == "terrain.cliff_ground")
                return "terrain.gravel";
            if (a == "terrain.dirt" || b == "terrain.dirt")
                return "road.dirt";
            if (a == "terrain.gravel" || b == "terrain.gravel")
                return "terrain.gravel";
            return "terrain.dark_grass";
        }
    }
}
#endif
