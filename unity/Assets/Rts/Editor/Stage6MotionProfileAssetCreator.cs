using System;
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Rendering.Motion;
using UnityEditor;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage6MotionProfileAssetCreator
    {
        public const string ProfileFolder = "Assets/Rts/ScriptableObjects/MotionProfiles";

        static readonly ProfileSpec[] ProfileSpecs =
        {
            new ProfileSpec("building_static", "Building Static", VisualMotionCategory.Building),
            new ProfileSpec("default_defense", "Defense Static", VisualMotionCategory.Defense),
            new ProfileSpec("infantry_basic", "Infantry Basic", VisualMotionCategory.Infantry),
            new ProfileSpec("infantry_heavy", "Infantry Heavy", VisualMotionCategory.Infantry),
            new ProfileSpec("infantry_assault", "Infantry Assault", VisualMotionCategory.Infantry),
            new ProfileSpec("infantry_utility", "Infantry Utility", VisualMotionCategory.Infantry),
            new ProfileSpec("tracked_light", "Tracked Light Vehicle", VisualMotionCategory.Vehicle),
            new ProfileSpec("tracked_medium", "Tracked Medium Vehicle", VisualMotionCategory.Vehicle),
            new ProfileSpec("tracked_heavy", "Tracked Heavy Vehicle", VisualMotionCategory.Vehicle),
            new ProfileSpec("wheeled_heavy", "Wheeled Heavy Vehicle", VisualMotionCategory.Harvester),
            new ProfileSpec("wheeled_scout", "Wheeled Scout Vehicle", VisualMotionCategory.Vehicle),
            new ProfileSpec("wheeled_apc", "Wheeled APC", VisualMotionCategory.Vehicle),
            new ProfileSpec("aircraft_attack", "Attack Aircraft", VisualMotionCategory.Aircraft),
            new ProfileSpec("aircraft_lifter", "Heavy Lifter Aircraft", VisualMotionCategory.Aircraft),
            new ProfileSpec("default_unknown", "Default Unknown", VisualMotionCategory.Unknown)
        };

        [MenuItem("ProjectAegisRTS/Create Stage 6 Motion Profiles")]
        public static void CreateStage6MotionProfilesMenu()
        {
            CreateOrUpdateProfileAssets();
        }

        public static void CreateStage6MotionProfilesBatch()
        {
            try
            {
                CreateOrUpdateProfileAssets();
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

        public static List<VisualMotionProfile> CreateOrUpdateProfileAssets()
        {
            EnsureFolders();
            var profiles = new List<VisualMotionProfile>();

            for (var i = 0; i < ProfileSpecs.Length; i++)
            {
                var spec = ProfileSpecs[i];
                var path = ProfilePath(spec.ProfileId);
                var profile = AssetDatabase.LoadAssetAtPath<VisualMotionProfile>(path);
                if (profile == null)
                {
                    profile = ScriptableObject.CreateInstance<VisualMotionProfile>();
                    AssetDatabase.CreateAsset(profile, path);
                }

                VisualMotionProfile.ConfigureDefault(profile, spec.ProfileId, spec.DisplayName, spec.Category);
                TuneProfile(profile);
                EditorUtility.SetDirty(profile);
                profiles.Add(profile);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Stage 6 motion profile assets updated at " + ProfileFolder);
            return profiles;
        }

        static string ProfilePath(string id)
        {
            return ProfileFolder + "/" + id + ".asset";
        }

        static void TuneProfile(VisualMotionProfile profile)
        {
            if (profile.profileId == "tracked_heavy")
            {
                profile.maxVisualSpeed = 3.8f;
                profile.accelerationSmoothing = 4.2f;
                profile.turnSmoothing = 4.5f;
                profile.suspensionStrength = 0.045f;
                return;
            }

            if (profile.profileId == "tracked_light")
            {
                profile.maxVisualSpeed = 5.4f;
                profile.accelerationSmoothing = 7.5f;
                profile.turnSmoothing = 7f;
                return;
            }

            if (profile.profileId == "wheeled_scout")
            {
                profile.maxVisualSpeed = 6.4f;
                profile.accelerationSmoothing = 9f;
                profile.turnSmoothing = 9f;
                profile.trackOrWheelAnimationScale = 4.5f;
                return;
            }

            if (profile.profileId == "wheeled_heavy")
            {
                profile.maxVisualSpeed = 3.1f;
                profile.accelerationSmoothing = 4.4f;
                profile.brakingSmoothing = 5.2f;
                profile.suspensionStrength = 0.055f;
                return;
            }

            if (profile.category == VisualMotionCategory.Aircraft)
            {
                profile.aircraftAltitudeOffset = profile.profileId == "aircraft_lifter" ? 2f : 1.7f;
                profile.aircraftBankAmount = profile.profileId == "aircraft_lifter" ? 16f : 26f;
                profile.aircraftHoverBobAmount = 0.12f;
                return;
            }

            if (profile.category == VisualMotionCategory.Infantry)
            {
                profile.infantryStepRate = profile.profileId == "infantry_utility" ? 5.2f : 7.2f;
                profile.infantryStrideLength = profile.profileId == "infantry_heavy" ? 0.34f : 0.42f;
            }
        }

        static void EnsureFolders()
        {
            EnsureFolder("Assets", "Rts");
            EnsureFolder("Assets/Rts", "ScriptableObjects");
            EnsureFolder("Assets/Rts/ScriptableObjects", "MotionProfiles");
        }

        static void EnsureFolder(string parent, string child)
        {
            var path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }

        struct ProfileSpec
        {
            public readonly string ProfileId;
            public readonly string DisplayName;
            public readonly VisualMotionCategory Category;

            public ProfileSpec(string profileId, string displayName, VisualMotionCategory category)
            {
                ProfileId = profileId;
                DisplayName = displayName;
                Category = category;
            }
        }
    }
}
