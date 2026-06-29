using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Art
{
    public enum ActorVisualPrefabMode
    {
        GeneratedBlockout,
        ProductionPrefab,
        Fallback
    }

    [CreateAssetMenu(menuName = "ProjectAegisRTS/Art/Actor Visual Definition")]
    public sealed class ActorVisualDefinition : ScriptableObject
    {
        public string actorTypeId;
        public string displayName;
        public string safeDisplayName;
        public ActorArtCategory category = ActorArtCategory.Unknown;
        public ActorArtProductionStatus productionStatus = ActorArtProductionStatus.ConceptOnly;
        public ConceptArtReference conceptReference;
        public Sprite icon;
        public GameObject generatedBlockoutPrefab;
        public GameObject productionPrefab;
        public GameObject fallbackPrefab;
        public ActorVisualPrefabMode preferredPrefabMode = ActorVisualPrefabMode.GeneratedBlockout;
        public string motionProfileId;
        public string buildingProfileId;
        public float selectionRadius = 0.6f;
        public float selectionHeight = 0.1f;
        public int footprintWidth = 1;
        public int footprintHeight = 1;
        public float visualScale = 1f;
        public float prefabHeightOffset;
        public bool useBuildingVisualController;
        public bool useVehicleMotionController;
        public bool useInfantryMotionController;
        public bool useAircraftMotionController;
        public bool useTurretVisualController;
        public List<ActorPrefabSocketKind> requiredSockets = new List<ActorPrefabSocketKind>();
        [TextArea(2, 5)] public string notes;
        public bool ipReviewRequired;
        public string sourceConceptOriginalLabel;
        public string sourceConceptRelativePath;

        public GameObject GetBestPrefab()
        {
            if (preferredPrefabMode == ActorVisualPrefabMode.ProductionPrefab && productionPrefab != null)
                return productionPrefab;
            if (preferredPrefabMode == ActorVisualPrefabMode.GeneratedBlockout && generatedBlockoutPrefab != null)
                return generatedBlockoutPrefab;
            if (preferredPrefabMode == ActorVisualPrefabMode.Fallback && fallbackPrefab != null)
                return fallbackPrefab;
            if (productionPrefab != null)
                return productionPrefab;
            if (generatedBlockoutPrefab != null)
                return generatedBlockoutPrefab;
            return fallbackPrefab;
        }

        public bool IsUsable()
        {
            return !string.IsNullOrEmpty(actorTypeId) && category != ActorArtCategory.Unknown && GetBestPrefab() != null;
        }

        public bool NeedsProductionArt()
        {
            return productionPrefab == null || productionStatus == ActorArtProductionStatus.ConceptOnly || productionStatus == ActorArtProductionStatus.BlockoutGenerated || productionStatus == ActorArtProductionStatus.NeedsIPReview;
        }

        public string GetValidationSummary()
        {
            if (string.IsNullOrEmpty(actorTypeId))
                return "missing actorTypeId";
            if (category == ActorArtCategory.Unknown)
                return actorTypeId + ": missing category";
            if (GetBestPrefab() == null)
                return actorTypeId + ": missing prefab";
            if (ipReviewRequired)
                return actorTypeId + ": usable with IP review flag";
            return actorTypeId + ": usable";
        }
    }
}
