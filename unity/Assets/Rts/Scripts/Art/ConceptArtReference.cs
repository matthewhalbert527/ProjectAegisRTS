using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Art
{
    [CreateAssetMenu(menuName = "ProjectAegisRTS/Art/Concept Art Reference")]
    public sealed class ConceptArtReference : ScriptableObject
    {
        public string actorTypeId;
        public string originalLabel;
        public string safeDisplayName;
        public string sourceFileRelativePath;
        public Texture2D unityTexture;
        public Sprite unitySprite;
        public ActorArtCategory category = ActorArtCategory.Unknown;
        [TextArea(2, 4)] public string intendedRole;
        [TextArea(2, 5)] public string artNotes;
        [TextArea(2, 5)] public string animationNotes;
        public bool ipReviewRequired;

        public bool HasTexture()
        {
            return unityTexture != null || unitySprite != null;
        }

        public string GetSummary()
        {
            var label = string.IsNullOrEmpty(safeDisplayName) ? actorTypeId : safeDisplayName;
            return label + " / " + category + (ipReviewRequired ? " / IP review" : string.Empty);
        }
    }
}
