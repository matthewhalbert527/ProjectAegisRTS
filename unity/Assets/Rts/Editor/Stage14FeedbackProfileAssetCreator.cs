using System;
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Feedback;
using UnityEditor;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage14FeedbackProfileAssetCreator
    {
        public const string FeedbackProfileRoot = "Assets/Rts/ScriptableObjects/Feedback";

        [MenuItem("ProjectAegisRTS/Stage 14/Create Feedback Profiles")]
        public static void CreateFeedbackProfilesMenu()
        {
            CreateOrUpdateFeedbackProfiles();
        }

        public static void CreateFeedbackProfilesBatch()
        {
            try
            {
                CreateOrUpdateFeedbackProfiles();
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

        public static FeedbackProfile[] CreateOrUpdateFeedbackProfiles()
        {
            EnsureFolder("Assets/Rts/ScriptableObjects");
            EnsureFolder(FeedbackProfileRoot);

            var profiles = new List<FeedbackProfile>();
            var values = (FeedbackEventType[])Enum.GetValues(typeof(FeedbackEventType));
            for (var i = 0; i < values.Length; i++)
            {
                var type = values[i];
                var path = FeedbackProfileRoot + "/" + SafeName(type) + "_feedback.asset";
                var profile = AssetDatabase.LoadAssetAtPath<FeedbackProfile>(path);
                if (profile == null)
                {
                    profile = ScriptableObject.CreateInstance<FeedbackProfile>();
                    AssetDatabase.CreateAsset(profile, path);
                }

                Configure(profile, type);
                EditorUtility.SetDirty(profile);
                profiles.Add(profile);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Stage 14 feedback profiles updated: " + profiles.Count);
            return profiles.ToArray();
        }

        static void Configure(FeedbackProfile profile, FeedbackEventType type)
        {
            profile.eventType = type;
            profile.displayName = type.ToString();
            profile.silentAudioCue = true;
            profile.spawnVisualMarker = true;
            profile.hapticPulse = type == FeedbackEventType.InvalidCommand ||
                                  type == FeedbackEventType.ProjectileImpact ||
                                  type == FeedbackEventType.Damage ||
                                  type == FeedbackEventType.DeathDestruction ||
                                  type == FeedbackEventType.LowPower;

            profile.markerPrimitive = PrimitiveType.Sphere;
            profile.markerScale = 0.3f;
            profile.durationSeconds = 0.7f;
            profile.floatSpeed = 0.6f;
            profile.audioVolume = 0.3f;
            profile.audioPitch = 1f;
            profile.hapticIntensity = 0.25f;

            if (type == FeedbackEventType.SelectionChanged)
                profile.color = new Color(0.2f, 0.95f, 1f, 0.9f);
            else if (type == FeedbackEventType.MoveCommand)
                profile.color = new Color(0.35f, 0.9f, 0.45f, 0.9f);
            else if (type == FeedbackEventType.InvalidCommand)
            {
                profile.color = new Color(1f, 0.2f, 0.18f, 0.95f);
                profile.markerPrimitive = PrimitiveType.Cube;
            }
            else if (type == FeedbackEventType.ProductionStarted || type == FeedbackEventType.ProductionCompleted || type == FeedbackEventType.BuildingPlaced)
                profile.color = new Color(0.9f, 0.65f, 0.2f, 0.95f);
            else if (type == FeedbackEventType.LowPower)
            {
                profile.color = new Color(1f, 0.35f, 0.05f, 0.95f);
                profile.markerPrimitive = PrimitiveType.Cube;
            }
            else if (type == FeedbackEventType.Harvest)
                profile.color = new Color(0.1f, 0.85f, 0.45f, 0.95f);
            else if (type == FeedbackEventType.Unload)
                profile.color = new Color(1f, 0.9f, 0.2f, 0.95f);
            else if (type == FeedbackEventType.Attack || type == FeedbackEventType.ProjectileImpact)
                profile.color = new Color(1f, 0.45f, 0.15f, 0.95f);
            else if (type == FeedbackEventType.Damage || type == FeedbackEventType.DeathDestruction)
            {
                profile.color = new Color(1f, 0.05f, 0.05f, 0.95f);
                profile.markerScale = type == FeedbackEventType.DeathDestruction ? 0.55f : 0.35f;
            }
            else if (type == FeedbackEventType.FogRadarChanged)
                profile.color = new Color(0.55f, 0.55f, 1f, 0.9f);
            else
                profile.color = new Color(0.8f, 0.8f, 0.8f, 0.85f);
        }

        static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
                return;

            var slash = folderPath.LastIndexOf('/');
            var parent = folderPath.Substring(0, slash);
            var name = folderPath.Substring(slash + 1);
            if (!AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        static string SafeName(FeedbackEventType type)
        {
            var text = type.ToString();
            var result = string.Empty;
            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (i > 0 && char.IsUpper(c))
                    result += "_";
                result += char.ToLowerInvariant(c);
            }

            return result;
        }
    }
}
