using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Art.Production
{
    public enum ArtistModelImportStatus
    {
        NoCandidateFound,
        CandidateSourceFound,
        CandidateImported,
        NeedsValidation,
        Blocked
    }

    [Serializable]
    public sealed class ArtistModelImportEntry
    {
        public string actorTypeId;
        public ArtistModelImportStatus status = ArtistModelImportStatus.NoCandidateFound;
        public string sourceAssetPath;
        public string importedAssetPath;
        public string draftPrefabPath;
        [TextArea(2, 5)] public string notes;
    }

    [CreateAssetMenu(menuName = "ProjectAegisRTS/Art/Artist Model Import Manifest")]
    public sealed class ArtistModelImportManifest : ScriptableObject
    {
        public List<ArtistModelImportEntry> entries = new List<ArtistModelImportEntry>();
        [TextArea(2, 5)] public string notes;

        public ArtistModelImportEntry FindEntry(string actorTypeId)
        {
            if (string.IsNullOrEmpty(actorTypeId) || entries == null)
                return null;

            for (var i = 0; i < entries.Count; i++)
                if (entries[i] != null && entries[i].actorTypeId == actorTypeId)
                    return entries[i];
            return null;
        }

        public ArtistModelImportStatus StatusFor(string actorTypeId)
        {
            var entry = FindEntry(actorTypeId);
            return entry == null ? ArtistModelImportStatus.NoCandidateFound : entry.status;
        }
    }
}
