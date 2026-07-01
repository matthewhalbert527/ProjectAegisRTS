using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Utilities
{
    public sealed class Stage1MaterialLibrary
    {
        public Material Board { get; private set; }
        public Material BoardEdge { get; private set; }
        public Material GridLine { get; private set; }
        public Material HoverCell { get; private set; }
        public Material PlacementValid { get; private set; }
        public Material PlacementInvalid { get; private set; }
        public Material Building { get; private set; }
        public Material BuildingLowPower { get; private set; }
        public Material Infantry { get; private set; }
        public Material Vehicle { get; private set; }
        public Material Aircraft { get; private set; }
        public Material Defense { get; private set; }
        public Material Selection { get; private set; }
        public Material Health { get; private set; }
        public Material LightActive { get; private set; }
        public Material LightInactive { get; private set; }
        public Material Machinery { get; private set; }
        public Material Production { get; private set; }

        public static Stage1MaterialLibrary Create()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");
            if (shader == null)
                shader = Shader.Find("Sprites/Default");

            return new Stage1MaterialLibrary
            {
                Board = CreateMaterial(shader, "Stage1 Board Surface", new Color(0.24f, 0.29f, 0.26f, 1f)),
                BoardEdge = CreateMaterial(shader, "Stage1 Board Edge", new Color(0.12f, 0.15f, 0.17f, 1f)),
                GridLine = CreateMaterial(shader, "Stage1 Grid Line", new Color(0.58f, 0.66f, 0.62f, 1f)),
                HoverCell = CreateMaterial(shader, "Stage1 Hover Cell", new Color(0.35f, 0.70f, 0.95f, 0.55f)),
                PlacementValid = CreateMaterial(shader, "Stage1 Placement Valid", new Color(0.25f, 0.85f, 0.45f, 0.6f)),
                PlacementInvalid = CreateMaterial(shader, "Stage1 Placement Invalid", new Color(0.95f, 0.25f, 0.18f, 0.65f)),
                Building = CreateMaterial(shader, "Stage1 Building", new Color(0.43f, 0.49f, 0.58f, 1f)),
                BuildingLowPower = CreateMaterial(shader, "Stage1 Building Low Power", new Color(0.22f, 0.25f, 0.29f, 1f)),
                Infantry = CreateMaterial(shader, "Stage1 Infantry", new Color(0.28f, 0.63f, 0.36f, 1f)),
                Vehicle = CreateMaterial(shader, "Stage1 Vehicle", new Color(0.68f, 0.62f, 0.39f, 1f)),
                Aircraft = CreateMaterial(shader, "Stage1 Aircraft", new Color(0.55f, 0.70f, 0.82f, 1f)),
                Defense = CreateMaterial(shader, "Stage1 Defense", new Color(0.54f, 0.50f, 0.66f, 1f)),
                Selection = CreateMaterial(shader, "Stage1 Selection", new Color(0.18f, 0.78f, 1f, 1f)),
                Health = CreateMaterial(shader, "Stage1 Health", new Color(0.28f, 0.92f, 0.38f, 1f)),
                LightActive = CreateMaterial(shader, "Stage1 Light Active", new Color(1f, 0.88f, 0.38f, 1f)),
                LightInactive = CreateMaterial(shader, "Stage1 Light Inactive", new Color(0.16f, 0.16f, 0.16f, 1f)),
                Machinery = CreateMaterial(shader, "Stage1 Machinery", new Color(0.28f, 0.72f, 0.88f, 1f)),
                Production = CreateMaterial(shader, "Stage1 Production", new Color(0.95f, 0.44f, 0.24f, 1f))
            };
        }

        static Material CreateMaterial(Shader shader, string materialName, Color color)
        {
            var material = new Material(shader);
            material.name = materialName;

            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color"))
                material.SetColor("_Color", color);
            if (material.HasProperty("_Smoothness"))
                material.SetFloat("_Smoothness", 0.35f);
            if (material.HasProperty("_Metallic"))
                material.SetFloat("_Metallic", 0f);

            return material;
        }
    }
}
