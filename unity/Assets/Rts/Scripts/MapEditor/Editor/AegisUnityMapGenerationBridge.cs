#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ProjectAegisRTS.UnityClient.MapEditor;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public sealed class AegisUnityMapGenerationSettings
    {
        public string PromptText;
        public string SizePreset;
        public int CustomWidth;
        public int CustomHeight;
        public int PlayerCount;
        public string Biome;
        public string ResourceDensity;
        public string CliffDensity;
        public string Rockiness;
        public string WaterAmount;
        public string Symmetry;
        public bool HasExplicitSeed;
        public int Seed;
        public string GameplayProfile;
        public bool OreRegenerationEnabled;
        public int OreRegenerationRatePerTick;
        public int OreRegenerationDelayTicks;
    }

    public sealed class AegisGeneratedMapPreviewModel
    {
        public bool Success;
        public bool UsedCoreBridge;
        public string AegisMapJson;
        public string TiledJson;
        public string SummaryText;
        public int FairnessScore;
        public readonly List<string> Warnings = new List<string>();
        public readonly List<string> Errors = new List<string>();
    }

    public sealed class AegisUnityMapGenerationBridge
    {
        const string CoreBridgeTypeName = "ProjectAegisRTS.Maps.Generation.AegisMapGenerationBridge";
        const string CoreBridgeRequestTypeName = "ProjectAegisRTS.Maps.Generation.AegisMapGenerationBridgeRequest";

        public AegisGeneratedMapPreviewModel Generate(AegisUnityMapGenerationSettings settings)
        {
            settings = settings ?? new AegisUnityMapGenerationSettings();
            var model = TryGenerateWithCore(settings);
            if (model.Success)
                return model;

            return GenerateFallbackShell(settings, model.Errors);
        }

        AegisGeneratedMapPreviewModel TryGenerateWithCore(AegisUnityMapGenerationSettings settings)
        {
            var model = new AegisGeneratedMapPreviewModel();
            try
            {
                var bridgeType = FindType(CoreBridgeTypeName);
                var requestType = FindType(CoreBridgeRequestTypeName);
                if (bridgeType == null || requestType == null)
                {
                    model.Errors.Add("CoreBridgeUnavailable:Rts.Core map generation bridge was not found in the Unity plugin assembly.");
                    return model;
                }

                var request = Activator.CreateInstance(requestType);
                Set(request, "PromptText", settings.PromptText);
                Set(request, "SizePreset", settings.SizePreset);
                Set(request, "CustomWidth", settings.CustomWidth);
                Set(request, "CustomHeight", settings.CustomHeight);
                Set(request, "PlayerCount", settings.PlayerCount);
                Set(request, "Biome", settings.Biome);
                Set(request, "ResourceDensity", settings.ResourceDensity);
                Set(request, "CliffDensity", settings.CliffDensity);
                Set(request, "Rockiness", settings.Rockiness);
                Set(request, "WaterAmount", settings.WaterAmount);
                Set(request, "Symmetry", settings.Symmetry);
                Set(request, "HasExplicitSeed", settings.HasExplicitSeed);
                Set(request, "Seed", settings.Seed);
                Set(request, "GameplayProfile", settings.GameplayProfile);
                Set(request, "OreRegenerationEnabled", settings.OreRegenerationEnabled);
                Set(request, "OreRegenerationRatePerTick", settings.OreRegenerationRatePerTick);
                Set(request, "OreRegenerationDelayTicks", settings.OreRegenerationDelayTicks);

                var bridge = Activator.CreateInstance(bridgeType);
                var result = bridgeType.GetMethod("Generate").Invoke(bridge, new[] { request });
                model.Success = ReadBool(result, "Success");
                model.UsedCoreBridge = true;
                model.AegisMapJson = ReadString(result, "AegisMapJson");
                model.TiledJson = ReadString(result, "TiledJson");
                model.SummaryText = ReadString(result, "SummaryText");
                model.FairnessScore = ReadInt(result, "FairnessScore");
                CopyStrings(result, "Warnings", model.Warnings);
                CopyStrings(result, "Errors", model.Errors);
                return model;
            }
            catch (Exception ex)
            {
                model.Errors.Add("CoreBridgeException:" + ex.GetType().Name + ":" + ex.Message);
                return model;
            }
        }

        static AegisGeneratedMapPreviewModel GenerateFallbackShell(AegisUnityMapGenerationSettings settings, IReadOnlyList<string> bridgeErrors)
        {
            var model = new AegisGeneratedMapPreviewModel();
            var width = ResolveWidth(settings);
            var height = ResolveHeight(settings);
            var mapId = "procedural_" + width + "x" + height + "_seed_" + settings.Seed;
            model.AegisMapJson = AegisMapEditorFileTemplates.CreateProceduralAegisMapJson(
                mapId,
                "Procedural " + settings.SizePreset + " " + settings.Biome,
                settings.PromptText,
                width,
                height,
                settings.PlayerCount,
                settings.Biome,
                settings.ResourceDensity,
                settings.CliffDensity,
                settings.Rockiness,
                settings.WaterAmount,
                settings.Symmetry,
                settings.Seed,
                settings.GameplayProfile,
                settings.OreRegenerationEnabled,
                settings.OreRegenerationRatePerTick,
                settings.OreRegenerationDelayTicks);
            model.TiledJson = AegisMapEditorFileTemplates.CreateTiledJsonShellFromAegisMap(model.AegisMapJson, mapId);
            model.SummaryText = "Fallback shell generated because the Rts.Core bridge was unavailable.\n" +
                "Size: " + width + "x" + height + "\n" +
                "Seed: " + settings.Seed + "\n" +
                "Players: " + settings.PlayerCount + "\n" +
                "Biome: " + settings.Biome + "\n" +
                "Fairness: unavailable until the Rts.Core bridge runs.";
            model.Success = true;
            model.UsedCoreBridge = false;
            model.Warnings.Add("UnityFallbackShell:Generated a compatible shell instead of running Rts.Core.");
            if (bridgeErrors != null)
                for (var i = 0; i < bridgeErrors.Count; i++)
                    model.Warnings.Add(bridgeErrors[i]);
            return model;
        }

        static Type FindType(string fullName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (var i = 0; i < assemblies.Length; i++)
            {
                var type = assemblies[i].GetType(fullName, false);
                if (type != null)
                    return type;
            }
            return null;
        }

        static void Set(object target, string propertyName, object value)
        {
            var property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property != null && property.CanWrite)
                property.SetValue(target, value, null);
        }

        static bool ReadBool(object target, string propertyName)
        {
            var value = Read(target, propertyName);
            return value is bool && (bool)value;
        }

        static int ReadInt(object target, string propertyName)
        {
            var value = Read(target, propertyName);
            return value is int ? (int)value : 0;
        }

        static string ReadString(object target, string propertyName)
        {
            return Read(target, propertyName) as string ?? string.Empty;
        }

        static object Read(object target, string propertyName)
        {
            if (target == null)
                return null;
            var property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            return property == null ? null : property.GetValue(target, null);
        }

        static void CopyStrings(object target, string propertyName, List<string> values)
        {
            var enumerable = Read(target, propertyName) as IEnumerable;
            if (enumerable == null)
                return;

            foreach (var item in enumerable)
                if (item != null)
                    values.Add(item.ToString());
        }

        static int ResolveWidth(AegisUnityMapGenerationSettings settings)
        {
            if (settings.SizePreset == "medium")
                return 200;
            if (settings.SizePreset == "large")
                return 400;
            if (settings.SizePreset == "custom")
                return settings.CustomWidth;
            return 100;
        }

        static int ResolveHeight(AegisUnityMapGenerationSettings settings)
        {
            if (settings.SizePreset == "medium")
                return 200;
            if (settings.SizePreset == "large")
                return 400;
            if (settings.SizePreset == "custom")
                return settings.CustomHeight;
            return 100;
        }
    }
}
#endif
