using System;
using System.IO;
using ProjectAegisRTS.UnityClient.Performance;
using UnityEditor;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage15PerformanceProfileAssetCreator
    {
        const string ProfileRoot = "Assets/Rts/ScriptableObjects/Performance";

        public static void CreatePerformanceProfilesBatch()
        {
            try
            {
                CreateOrUpdatePerformanceProfiles();
                Debug.Log("Stage 15 performance profiles updated");
                if (Application.isBatchMode)
                    EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                if (Application.isBatchMode)
                    EditorApplication.Exit(1);
                throw;
            }
        }

        [MenuItem("ProjectAegisRTS/Stage 15/Create Performance Budget Profiles")]
        public static void CreatePerformanceProfilesMenu()
        {
            CreateOrUpdatePerformanceProfiles();
        }

        public static PerformanceBudgetProfile[] CreateOrUpdatePerformanceProfiles()
        {
            EnsureFolder(ProfileRoot);
            var quest = CreateOrUpdateProfile(
                "quest",
                "Quest Placeholder Budget",
                Path.Combine(ProfileRoot, "quest_performance_budget.asset").Replace("\\", "/"),
                72,
                900,
                450,
                120,
                48,
                32,
                128,
                0,
                2,
                1,
                18f,
                0.7f,
                "Quest 3S placeholder budget. The target is conservative until physical device profiling exists.");

            var pc = CreateOrUpdateProfile(
                "pc",
                "PC Placeholder Budget",
                Path.Combine(ProfileRoot, "pc_performance_budget.asset").Replace("\\", "/"),
                60,
                1800,
                900,
                220,
                96,
                64,
                256,
                Mathf.Clamp(QualitySettings.GetQualityLevel(), 0, Mathf.Max(0, QualitySettings.names.Length - 1)),
                2,
                2,
                45f,
                1.0f,
                "PC placeholder budget. Keeps standalone support explicit without Quest-specific assumptions.");

            AssetDatabase.SaveAssets();
            return new[] { quest, pc };
        }

        static PerformanceBudgetProfile CreateOrUpdateProfile(string profileId, string displayName, string path, int targetFrameRate, int maxObjects, int maxRenderers, int maxActors, int maxProjectiles, int maxMarkers, int maxInactivePool, int qualityLevel, int antiAliasing, int pixelLightCount, float shadowDistance, float lodBias, string notes)
        {
            var profile = AssetDatabase.LoadAssetAtPath<PerformanceBudgetProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<PerformanceBudgetProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }

            profile.profileId = profileId;
            profile.displayName = displayName;
            profile.targetFrameRate = targetFrameRate;
            profile.maxSceneGameObjects = maxObjects;
            profile.maxActiveRenderers = maxRenderers;
            profile.maxActorViews = maxActors;
            profile.maxProjectileViews = maxProjectiles;
            profile.maxFeedbackMarkers = maxMarkers;
            profile.maxPoolInactiveObjects = maxInactivePool;
            profile.qualityLevel = qualityLevel;
            profile.antiAliasing = antiAliasing;
            profile.pixelLightCount = pixelLightCount;
            profile.shadowDistance = shadowDistance;
            profile.lodBias = lodBias;
            profile.preferVSyncOff = true;
            profile.notes = notes;
            EditorUtility.SetDirty(profile);
            return profile;
        }

        static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
                return;

            var parent = Path.GetDirectoryName(folder).Replace("\\", "/");
            var name = Path.GetFileName(folder);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
