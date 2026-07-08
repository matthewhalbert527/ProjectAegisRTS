namespace ProjectAegisRTS.UnityClient.MapEditor.Visuals
{
    public enum AegisMapVisualRenderMode
    {
        ProductionPreview,
        DebugOverlay,
        Hybrid
    }

    public sealed class AegisMapVisualOverlaySettings
    {
        public bool Terrain;
        public bool Blockers;
        public bool Resources;
        public bool BuildPads;
        public bool Cliffs;
        public bool Pathability;

        public static AegisMapVisualOverlaySettings None()
        {
            return new AegisMapVisualOverlaySettings();
        }

        public static AegisMapVisualOverlaySettings AllDebug()
        {
            return new AegisMapVisualOverlaySettings
            {
                Terrain = true,
                Blockers = true,
                Resources = true,
                BuildPads = true,
                Cliffs = true,
                Pathability = true
            };
        }
    }

    public sealed class AegisMapVisualCompileSettings
    {
        public AegisMapVisualRenderMode RenderMode = AegisMapVisualRenderMode.ProductionPreview;
        public AegisMapVisualOverlaySettings Overlays = AegisMapVisualOverlaySettings.None();

        public static AegisMapVisualCompileSettings ProductionDefault()
        {
            return new AegisMapVisualCompileSettings();
        }

        public static AegisMapVisualCompileSettings DebugDefault()
        {
            return new AegisMapVisualCompileSettings
            {
                RenderMode = AegisMapVisualRenderMode.DebugOverlay,
                Overlays = AegisMapVisualOverlaySettings.AllDebug()
            };
        }
    }
}
