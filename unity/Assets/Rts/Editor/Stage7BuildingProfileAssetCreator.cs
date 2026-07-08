using System;
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Rendering.Buildings;
using UnityEditor;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage7BuildingProfileAssetCreator
    {
        public const string ProfileFolder = "Assets/Rts/ScriptableObjects/BuildingProfiles";

        static readonly ProfileSpec[] ProfileSpecs =
        {
            new ProfileSpec("fabrication_hub", 3, 3),
            new ProfileSpec("power_plant", 2, 2),
            new ProfileSpec("advanced_power_plant", 2, 2),
            new ProfileSpec("barracks", 2, 2),
            new ProfileSpec("war_factory", 3, 2),
            new ProfileSpec("refinery", 3, 3),
            new ProfileSpec("gun_tower", 1, 1),
            new ProfileSpec("cannon_turret", 1, 1),
            new ProfileSpec("advanced_gun_tower", 1, 1),
            new ProfileSpec("comm_center", 2, 2),
            new ProfileSpec("repair_bay", 3, 2),
            new ProfileSpec("tech_center", 2, 2),
            new ProfileSpec("field_hospital", 2, 2),
            new ProfileSpec("dual_helipad", 3, 2),
            new ProfileSpec("default_building", 2, 2),
            new ProfileSpec("default_defense", 1, 1)
        };

        [MenuItem("ProjectAegisRTS/Create Stage 7 Building Visual Profiles")]
        public static void CreateBuildingProfilesMenu()
        {
            CreateOrUpdateProfileAssets();
        }

        public static void CreateBuildingProfilesBatch()
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

        public static List<BuildingVisualProfile> CreateOrUpdateProfileAssets()
        {
            EnsureFolders();
            var profiles = new List<BuildingVisualProfile>();

            for (var i = 0; i < ProfileSpecs.Length; i++)
            {
                var spec = ProfileSpecs[i];
                var path = ProfileFolder + "/" + spec.ActorTypeId + "_building_visual.asset";
                var profile = AssetDatabase.LoadAssetAtPath<BuildingVisualProfile>(path);
                if (profile == null)
                {
                    profile = ScriptableObject.CreateInstance<BuildingVisualProfile>();
                    AssetDatabase.CreateAsset(profile, path);
                }

                profile.ConfigureDefault(spec.ActorTypeId + "_building_visual", spec.ActorTypeId, BuildingVisualProfileLibrary.CategoryForActor(spec.ActorTypeId), spec.Width, spec.Height);
                EditorUtility.SetDirty(profile);
                profiles.Add(profile);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Stage 7 building visual profile assets updated at " + ProfileFolder);
            return profiles;
        }

        static void EnsureFolders()
        {
            EnsureFolder("Assets", "Rts");
            EnsureFolder("Assets/Rts", "ScriptableObjects");
            EnsureFolder("Assets/Rts/ScriptableObjects", "BuildingProfiles");
        }

        static void EnsureFolder(string parent, string child)
        {
            var path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }

        struct ProfileSpec
        {
            public readonly string ActorTypeId;
            public readonly int Width;
            public readonly int Height;

            public ProfileSpec(string actorTypeId, int width, int height)
            {
                ActorTypeId = actorTypeId;
                Width = width;
                Height = height;
            }
        }
    }
}
