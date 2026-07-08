using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.MapEditor.Visuals
{
    [Serializable]
    public sealed class AegisVisualSemanticRule
    {
        public string VisualId;
        public string SemanticRole;
        public string Biome;
        public string Path;
        public string Material;
        public Vector3 Scale = Vector3.one;
        public string Pivot = "center";
        public string RotationRule = "deterministic";
        public string PlacementRule = "semantic";
        public string DensityRule = "theme";
        public string DepletionStateBehavior = "hide_when_depleted";
        public bool PrototypeOnly = true;
        public bool Transparent;
        public Color Color = Color.white;
        public string AlbedoPath;
        public string NormalPath;
        public string MaskPath;
    }

    [Serializable]
    public sealed class AegisMapVisualTheme
    {
        public string ThemeId;
        public string DisplayName;
        public string Biome;
        public bool PrototypeOnly = true;
        public readonly List<AegisVisualSemanticRule> Rules = new List<AegisVisualSemanticRule>();

        public AegisVisualSemanticRule RuleFor(string semanticRole)
        {
            if (string.IsNullOrEmpty(semanticRole))
                return null;

            for (var i = 0; i < Rules.Count; i++)
            {
                var rule = Rules[i];
                if (rule != null && string.Equals(rule.SemanticRole, semanticRole, StringComparison.OrdinalIgnoreCase))
                    return rule;
            }

            return null;
        }

        public Color ColorFor(string semanticRole, Color fallback)
        {
            var rule = RuleFor(semanticRole);
            return rule == null ? fallback : rule.Color;
        }

        public string TextureFor(string semanticRole)
        {
            var rule = RuleFor(semanticRole);
            return rule == null ? null : rule.AlbedoPath;
        }

        public AegisMapVisualTheme Add(string role, Color color, string visualId = null, string path = null, bool transparent = false, string albedo = null, string normal = null, string mask = null)
        {
            Rules.Add(new AegisVisualSemanticRule
            {
                VisualId = string.IsNullOrEmpty(visualId) ? role : visualId,
                SemanticRole = role,
                Biome = Biome,
                Path = path,
                Material = role + ".mat",
                Transparent = transparent,
                Color = color,
                AlbedoPath = albedo,
                NormalPath = normal,
                MaskPath = mask,
                PrototypeOnly = PrototypeOnly
            });
            return this;
        }
    }
}
