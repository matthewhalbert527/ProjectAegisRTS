using System;
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Rendering.Combat;
using UnityEditor;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage9CombatProfileAssetCreator
    {
        public const string CombatProfileRoot = "Assets/Rts/ScriptableObjects/Combat";

        [MenuItem("ProjectAegisRTS/Stage 9/Create Combat Visual Profiles")]
        public static void CreateCombatProfilesMenu()
        {
            CreateOrUpdateProfiles();
        }

        public static void CreateCombatProfilesBatch()
        {
            try
            {
                CreateOrUpdateProfiles();
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

        public static List<CombatVisualProfile> CreateOrUpdateProfiles()
        {
            EnsureFolder("Assets/Rts/ScriptableObjects");
            EnsureFolder(CombatProfileRoot);

            var profiles = new List<CombatVisualProfile>
            {
                CreateOrUpdate("rifle_bullet", "Rifle Bullet", CombatVisualCategory.Bullet, new Color(1f, 0.92f, 0.45f, 1f), 0.11f, 0.55f),
                CreateOrUpdate("tank_shell", "Tank Shell", CombatVisualCategory.Shell, new Color(1f, 0.56f, 0.2f, 1f), 0.2f, 0.75f),
                CreateOrUpdate("tower_shell", "Tower Shell", CombatVisualCategory.Shell, new Color(1f, 0.35f, 0.18f, 1f), 0.2f, 0.75f),
                CreateOrUpdate("rocket_placeholder", "Rocket Placeholder", CombatVisualCategory.Rocket, new Color(1f, 0.22f, 0.12f, 1f), 0.22f, 0.9f),
                CreateOrUpdate("explosion_placeholder", "Explosion Placeholder", CombatVisualCategory.Explosion, new Color(1f, 0.2f, 0.08f, 1f), 0.55f, 0.9f),
                CreateOrUpdate("impact_placeholder", "Impact Placeholder", CombatVisualCategory.Impact, new Color(1f, 0.75f, 0.25f, 1f), 0.32f, 0.7f)
            };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Stage 9 combat visual profiles updated: " + profiles.Count);
            return profiles;
        }

        public static List<CombatVisualProfile> LoadProfiles()
        {
            var ids = new[] { "rifle_bullet", "tank_shell", "tower_shell", "rocket_placeholder", "explosion_placeholder", "impact_placeholder" };
            var profiles = new List<CombatVisualProfile>();
            for (var i = 0; i < ids.Length; i++)
            {
                var profile = AssetDatabase.LoadAssetAtPath<CombatVisualProfile>(ProfilePath(ids[i]));
                if (profile != null)
                    profiles.Add(profile);
            }
            return profiles;
        }

        static CombatVisualProfile CreateOrUpdate(string profileId, string displayName, CombatVisualCategory category, Color color, float scale, float tracerLength)
        {
            var path = ProfilePath(profileId);
            var profile = AssetDatabase.LoadAssetAtPath<CombatVisualProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CombatVisualProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }

            profile.profileId = profileId;
            profile.displayName = displayName;
            profile.category = category;
            profile.projectileScale = scale;
            profile.tracerLength = tracerLength;
            profile.impactDuration = category == CombatVisualCategory.Explosion ? 0.85f : 0.35f;
            profile.muzzleFlashDuration = 0.16f;
            profile.deathVisualDuration = 2f;
            profile.color = color;
            EditorUtility.SetDirty(profile);
            return profile;
        }

        static string ProfilePath(string profileId)
        {
            return CombatProfileRoot + "/" + profileId + ".asset";
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            var slash = path.LastIndexOf('/');
            var parent = path.Substring(0, slash);
            var name = path.Substring(slash + 1);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
