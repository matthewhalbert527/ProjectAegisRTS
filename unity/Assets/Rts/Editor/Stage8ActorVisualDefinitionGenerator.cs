using System;
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Art;
using UnityEditor;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage8ActorVisualDefinitionGenerator
    {
        [MenuItem("ProjectAegisRTS/Stage 8/Create Actor Visual Definitions")]
        public static void CreateDefinitionsMenu()
        {
            CreateOrUpdateDefinitions();
        }

        public static void CreateDefinitionsBatch()
        {
            try
            {
                CreateOrUpdateDefinitions();
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

        public static List<ActorVisualDefinition> CreateOrUpdateDefinitions()
        {
            Stage8ActorCatalog.EnsureStage8Folders();
            var specs = Stage8ActorCatalog.LoadSpecs();
            var definitions = new List<ActorVisualDefinition>();

            for (var i = 0; i < specs.Count; i++)
            {
                var spec = specs[i];
                var path = Stage8ActorCatalog.DefinitionAssetPath(spec);
                var definition = AssetDatabase.LoadAssetAtPath<ActorVisualDefinition>(path);
                if (definition == null)
                {
                    definition = ScriptableObject.CreateInstance<ActorVisualDefinition>();
                    AssetDatabase.CreateAsset(definition, path);
                }

                definition.actorTypeId = spec.ActorTypeId;
                definition.displayName = spec.OriginalLabel;
                definition.safeDisplayName = spec.SafeDisplayName;
                definition.category = spec.Category;
                definition.productionStatus = spec.IpReviewRequired ? ActorArtProductionStatus.NeedsIPReview : ActorArtProductionStatus.BlockoutGenerated;
                definition.conceptReference = AssetDatabase.LoadAssetAtPath<ConceptArtReference>(Stage8ActorCatalog.ConceptReferenceAssetPath(spec));
                definition.icon = Stage8ActorCatalog.LoadSpriteAtPath(Stage8ActorCatalog.IconAssetPath(spec));
                definition.generatedBlockoutPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Stage8ActorCatalog.BlockoutPrefabPath(spec));
                definition.preferredPrefabMode = ActorVisualPrefabMode.GeneratedBlockout;
                definition.motionProfileId = Stage8ActorCatalog.MotionProfileIdFor(spec);
                definition.buildingProfileId = Stage8ActorCatalog.BuildingProfileIdFor(spec);
                definition.selectionRadius = spec.Category == ActorArtCategory.Building || spec.Category == ActorArtCategory.Support ? Mathf.Max(spec.FootprintWidth, spec.FootprintHeight) * 0.55f : 0.6f;
                definition.selectionHeight = spec.Category == ActorArtCategory.Aircraft ? 1.6f : 0.1f;
                definition.footprintWidth = spec.FootprintWidth;
                definition.footprintHeight = spec.FootprintHeight;
                definition.visualScale = 1f;
                definition.prefabHeightOffset = spec.Category == ActorArtCategory.Aircraft ? 1.2f : 0f;
                definition.useBuildingVisualController = spec.Category == ActorArtCategory.Building || spec.Category == ActorArtCategory.Support || spec.Category == ActorArtCategory.Defense;
                definition.useVehicleMotionController = spec.Category == ActorArtCategory.Vehicle || spec.Category == ActorArtCategory.Resource;
                definition.useInfantryMotionController = spec.Category == ActorArtCategory.Infantry;
                definition.useAircraftMotionController = spec.Category == ActorArtCategory.Aircraft;
                definition.useTurretVisualController = spec.ActorTypeId.Contains("tank") || spec.Category == ActorArtCategory.Defense || spec.ActorTypeId == "apc" || spec.ActorTypeId == "attack_aircraft";
                definition.requiredSockets = Stage8ActorCatalog.RequiredSocketsFor(spec);
                definition.notes = spec.ArtNotes + "\n" + spec.AnimationNotes;
                definition.ipReviewRequired = spec.IpReviewRequired;
                definition.sourceConceptOriginalLabel = spec.OriginalLabel;
                definition.sourceConceptRelativePath = string.IsNullOrEmpty(spec.SourceFile) ? string.Empty : "art/concepts/source/" + spec.SourceFile;
                EditorUtility.SetDirty(definition);
                definitions.Add(definition);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Stage 8 actor visual definitions updated: " + definitions.Count);
            return definitions;
        }
    }
}
