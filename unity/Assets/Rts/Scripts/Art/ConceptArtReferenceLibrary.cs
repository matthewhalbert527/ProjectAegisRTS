using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Art
{
    public sealed class ConceptArtReferenceLibrary : MonoBehaviour
    {
        public List<ConceptArtReference> references = new List<ConceptArtReference>();

        readonly Dictionary<string, ConceptArtReference> byActorTypeId = new Dictionary<string, ConceptArtReference>();
        bool initialized;

        public int ReferenceCount
        {
            get
            {
                EnsureInitialized();
                return byActorTypeId.Count;
            }
        }

        public void EnsureInitialized()
        {
            if (initialized)
                return;

            initialized = true;
            byActorTypeId.Clear();

            if (references == null)
                return;

            for (var i = 0; i < references.Count; i++)
            {
                var reference = references[i];
                if (reference == null || string.IsNullOrEmpty(reference.actorTypeId))
                    continue;
                if (!byActorTypeId.ContainsKey(reference.actorTypeId))
                    byActorTypeId.Add(reference.actorTypeId, reference);
            }
        }

        public void RebuildLookup()
        {
            initialized = false;
            EnsureInitialized();
        }

        public bool TryGetReference(string actorTypeId, out ConceptArtReference reference)
        {
            EnsureInitialized();
            reference = null;
            if (string.IsNullOrEmpty(actorTypeId))
                return false;
            return byActorTypeId.TryGetValue(actorTypeId, out reference);
        }

        public IReadOnlyList<ConceptArtReference> GetAllReferences()
        {
            EnsureInitialized();
            return references == null ? new List<ConceptArtReference>() : references;
        }
    }
}
