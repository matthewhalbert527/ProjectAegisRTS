using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Visuals
{
    public sealed class BattlefieldMaterialLibrary : MonoBehaviour
    {
        public Material grassDirt;
        public Material compactedBaseGround;
        public Material concretePad;
        public Material roadPath;
        public Material resourceField;
        public Material rockBlocked;
        public Material water;
        public Material fogExplored;
        public Material foundationEdge;
        public Material wornMetal;
        public Material warmLight;
        public Material shadowFog;
        public Material fineGridGuide;

        public void EnsureRuntimeDefaults()
        {
            grassDirt = grassDirt ?? CreateRuntimeMaterial("Stage29 Runtime Grass Dirt", new Color(0.24f, 0.34f, 0.22f, 1f), 0.12f, 0f);
            compactedBaseGround = compactedBaseGround ?? CreateRuntimeMaterial("Stage29 Runtime Compacted Base Ground", new Color(0.30f, 0.29f, 0.24f, 1f), 0.18f, 0f);
            concretePad = concretePad ?? CreateRuntimeMaterial("Stage29 Runtime Concrete Pad", new Color(0.46f, 0.48f, 0.43f, 1f), 0.22f, 0f);
            roadPath = roadPath ?? CreateRuntimeMaterial("Stage29 Runtime Road Path", new Color(0.25f, 0.24f, 0.21f, 1f), 0.10f, 0f);
            resourceField = resourceField ?? CreateRuntimeMaterial("Stage29 Runtime Resource Field", new Color(0.21f, 0.49f, 0.40f, 1f), 0.30f, 0f);
            rockBlocked = rockBlocked ?? CreateRuntimeMaterial("Stage29 Runtime Rock Blocked", new Color(0.27f, 0.26f, 0.24f, 1f), 0.08f, 0f);
            water = water ?? CreateRuntimeMaterial("Stage29 Runtime Water", new Color(0.10f, 0.22f, 0.31f, 0.92f), 0.58f, 0f);
            fogExplored = fogExplored ?? CreateRuntimeMaterial("Stage29 Runtime Fog Explored", new Color(0.16f, 0.19f, 0.19f, 0.82f), 0.05f, 0f);
            foundationEdge = foundationEdge ?? CreateRuntimeMaterial("Stage29 Runtime Foundation Edge", new Color(0.20f, 0.21f, 0.19f, 1f), 0.16f, 0f);
            wornMetal = wornMetal ?? CreateRuntimeMaterial("Stage29 Runtime Worn Metal", new Color(0.29f, 0.32f, 0.31f, 1f), 0.32f, 0.10f);
            warmLight = warmLight ?? CreateRuntimeMaterial("Stage29 Runtime Warm Light", new Color(0.95f, 0.70f, 0.28f, 1f), 0.45f, 0f);
            shadowFog = shadowFog ?? CreateRuntimeMaterial("Stage29 Runtime Shadow Fog", new Color(0.06f, 0.08f, 0.08f, 0.78f), 0.02f, 0f);
            fineGridGuide = fineGridGuide ?? CreateRuntimeMaterial("Stage29 Runtime Fine Grid Guide", new Color(0.44f, 0.64f, 0.62f, 0.55f), 0.10f, 0f);
        }

        public Material MaterialForTerrainKind(string terrainKind)
        {
            EnsureRuntimeDefaults();
            switch (terrainKind)
            {
                case "GrassDirt": return grassDirt;
                case "CompactedBase": return compactedBaseGround;
                case "ConcretePad": return concretePad;
                case "RoadPath": return roadPath;
                case "ResourceField": return resourceField;
                case "RockBlocked": return rockBlocked;
                case "Water": return water;
                case "FogExplored": return fogExplored;
                default: return grassDirt;
            }
        }

        static Material CreateRuntimeMaterial(string materialName, Color color, float smoothness, float metallic)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");

            var material = new Material(shader);
            material.name = materialName;
            ApplyMaterialProperties(material, color, smoothness, metallic);
            material.hideFlags = HideFlags.DontSave;
            return material;
        }

        public static void ApplyMaterialProperties(Material material, Color color, float smoothness, float metallic)
        {
            if (material == null)
                return;

            material.color = color;
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Smoothness"))
                material.SetFloat("_Smoothness", smoothness);
            if (material.HasProperty("_Metallic"))
                material.SetFloat("_Metallic", metallic);
        }
    }
}
