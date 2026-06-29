using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Art
{
    public sealed class ActorVisualPrefabResolver : MonoBehaviour
    {
        public ActorVisualDefinitionLibrary definitionLibrary;

        public GameObject ResolvePrefab(string actorTypeId)
        {
            var definition = ResolveDefinition(actorTypeId);
            return definition == null ? null : definition.GetBestPrefab();
        }

        public bool ResolvePrefab(string actorTypeId, out ActorVisualDefinition definition, out GameObject prefab)
        {
            definition = ResolveDefinition(actorTypeId);
            prefab = definition == null ? null : definition.GetBestPrefab();
            return prefab != null;
        }

        public ActorVisualDefinition ResolveDefinition(string actorTypeId)
        {
            EnsureLibrary();
            if (definitionLibrary == null)
                return null;
            return definitionLibrary.GetDefinition(actorTypeId);
        }

        public string GetValidationStatus(string actorTypeId)
        {
            var definition = ResolveDefinition(actorTypeId);
            if (definition == null)
                return actorTypeId + ": missing definition";
            return definition.GetValidationSummary();
        }

        void EnsureLibrary()
        {
            if (definitionLibrary == null)
                definitionLibrary = Object.FindFirstObjectByType<ActorVisualDefinitionLibrary>();
            if (definitionLibrary != null)
                definitionLibrary.EnsureInitialized();
        }
    }
}
