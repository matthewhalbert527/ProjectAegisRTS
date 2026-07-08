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

            var role = context.IsDebugOverlay ? DebugTransitionRole(a, b) : ProductionTransitionRole(a, b);
            var material = AegisVisualCompilerPrimitives.Material(context, role);
            var center = TransitionCenter(context, x, y, dx, dy);
            var width = dx == 0 ? 1.18f : TransitionWidth(context, x, y, dx, dy);
            var height = dy == 0 ? 1.18f : TransitionWidth(context, x, y, dx, dy);
            var angle = context.IsDebugOverlay ? 0f : (context.Hash01(x, y, dx == 0 ? 3310 : 3320) - 0.5f) * 7f;
            var elevation = context.IsDebugOverlay ? 0.012f : 0.066f;
            AegisVisualCompilerPrimitives.CreateQuad(layer, "transition_" + x + "_" + y + "_" + nx + "_" + ny, center, width, height, elevation, material, angle);
            summary.TransitionEdges++;
        }

        static Vector2 TransitionCenter(AegisMapVisualCompileContext context, int x, int y, int dx, int dy)
        {
            var center = new Vector2(x + 0.5f + dx * 0.5f, y + 0.5f + dy * 0.5f);
            if (context.IsDebugOverlay)
                return center;

            var alongJitter = (context.Hash01(x, y, dx == 0 ? 3330 : 3340) - 0.5f) * 0.22f;
            if (dx == 0)
                center.x += alongJitter;
            else
                center.y += alongJitter;
            return center;
        }

        static float TransitionWidth(AegisMapVisualCompileContext context, int x, int y, int dx, int dy)
        {
            if (context.IsDebugOverlay)
                return 0.32f;

            return Mathf.Lerp(0.52f, 0.82f, context.Hash01(x, y, dx == 0 ? 3350 : 3360));
        }

        static string DebugTransitionRole(string a, string b)
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

        static string ProductionTransitionRole(string a, string b)
        {
            if (a == "terrain.concrete_base_pad" || b == "terrain.concrete_base_pad")
                return "terrain.blend_dirt";
            if (a == "terrain.cliff_ground" || b == "terrain.cliff_ground")
                return "terrain.blend_gravel";
            if (a == "terrain.mud" || b == "terrain.mud")
                return "terrain.blend_mud";
            if (a == "terrain.dirt" || b == "terrain.dirt")
                return "terrain.blend_dirt";
            if (a == "terrain.gravel" || b == "terrain.gravel")
                return "terrain.blend_gravel";
            return "terrain.blend_grass";
        }
    }
}
#endif
