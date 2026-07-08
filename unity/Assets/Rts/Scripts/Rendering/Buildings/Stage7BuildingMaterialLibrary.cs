using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Buildings
{
    public sealed class Stage7BuildingMaterialLibrary
    {
        public Material PoweredLight { get; private set; }
        public Material LowPowerLight { get; private set; }
        public Material OfflineLight { get; private set; }
        public Material Machinery { get; private set; }
        public Material ProductionIndicator { get; private set; }
        public Material WarningDamaged { get; private set; }
        public Material Door { get; private set; }
        public Material RadarTurbineCrane { get; private set; }
        public Material SmokePlaceholder { get; private set; }

        public static Stage7BuildingMaterialLibrary Create()
        {
            return new Stage7BuildingMaterialLibrary
            {
                PoweredLight = CreateMaterial(new Color(0.3f, 1f, 0.55f, 1f)),
                LowPowerLight = CreateMaterial(new Color(1f, 0.75f, 0.22f, 1f)),
                OfflineLight = CreateMaterial(new Color(0.08f, 0.09f, 0.1f, 1f)),
                Machinery = CreateMaterial(new Color(0.52f, 0.58f, 0.62f, 1f)),
                ProductionIndicator = CreateMaterial(new Color(0.2f, 0.82f, 1f, 1f)),
                WarningDamaged = CreateMaterial(new Color(1f, 0.18f, 0.12f, 1f)),
                Door = CreateMaterial(new Color(0.18f, 0.24f, 0.28f, 1f)),
                RadarTurbineCrane = CreateMaterial(new Color(0.72f, 0.78f, 0.82f, 1f)),
                SmokePlaceholder = CreateMaterial(new Color(0.35f, 0.38f, 0.4f, 0.75f))
            };
        }

        static Material CreateMaterial(Color color)
        {
            var material = new Material(Shader.Find("Standard"));
            material.color = color;
            return material;
        }
    }
}
