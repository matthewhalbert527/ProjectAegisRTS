namespace ProjectAegisRTS.Maps.Tiled
{
    public sealed class AegisTiledImportOptions
    {
        public string DefaultMapId { get; set; }
        public string DefaultDisplayName { get; set; }
        public string DefaultTerrainId { get; set; }
        public string DefaultResourceKind { get; set; }
        public int DefaultResourceAmount { get; set; }

        public AegisTiledImportOptions()
        {
            DefaultMapId = "tiled_import";
            DefaultDisplayName = "Tiled Import";
            DefaultTerrainId = AegisMapTerrainIds.Clear;
            DefaultResourceKind = "ore";
            DefaultResourceAmount = 500;
        }
    }
}
