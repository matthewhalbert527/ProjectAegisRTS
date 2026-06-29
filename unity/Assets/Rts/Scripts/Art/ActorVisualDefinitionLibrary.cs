using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Art
{
    public sealed class ActorVisualDefinitionLibrary : MonoBehaviour
    {
        public List<ActorVisualDefinition> definitions = new List<ActorVisualDefinition>();
        public ActorVisualDefinition fallbackDefinition;

        readonly Dictionary<string, ActorVisualDefinition> byActorTypeId = new Dictionary<string, ActorVisualDefinition>();
        bool initialized;

        public int DefinitionCount
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

            if (definitions == null)
                return;

            for (var i = 0; i < definitions.Count; i++)
                Register(definitions[i]);
        }

        public void RebuildLookup()
        {
            initialized = false;
            EnsureInitialized();
        }

        public ActorVisualDefinition GetDefinition(string actorTypeId)
        {
            ActorVisualDefinition definition;
            return TryGetDefinition(actorTypeId, out definition) ? definition : fallbackDefinition;
        }

        public bool TryGetDefinition(string actorTypeId, out ActorVisualDefinition definition)
        {
            EnsureInitialized();
            definition = null;
            if (string.IsNullOrEmpty(actorTypeId))
                return false;
            return byActorTypeId.TryGetValue(actorTypeId, out definition);
        }

        public IReadOnlyList<ActorVisualDefinition> GetAllDefinitions()
        {
            EnsureInitialized();
            return definitions == null ? new List<ActorVisualDefinition>() : definitions;
        }

        public List<ActorVisualDefinition> GetDefinitionsByCategory(ActorArtCategory category)
        {
            EnsureInitialized();
            var results = new List<ActorVisualDefinition>();
            if (definitions == null)
                return results;

            for (var i = 0; i < definitions.Count; i++)
                if (definitions[i] != null && definitions[i].category == category)
                    results.Add(definitions[i]);
            return results;
        }

        public GameObject GetBestPrefab(string actorTypeId)
        {
            var definition = GetDefinition(actorTypeId);
            return definition == null ? null : definition.GetBestPrefab();
        }

        public List<string> ValidateAllDefinitions()
        {
            EnsureInitialized();
            var errors = new List<string>();
            if (definitions == null || definitions.Count == 0)
            {
                errors.Add("No actor visual definitions are assigned.");
                return errors;
            }

            for (var i = 0; i < definitions.Count; i++)
            {
                var definition = definitions[i];
                if (definition == null)
                {
                    errors.Add("Definition slot " + i + " is empty.");
                    continue;
                }

                if (!definition.IsUsable())
                    errors.Add(definition.GetValidationSummary());
            }

            return errors;
        }

        void Register(ActorVisualDefinition definition)
        {
            if (definition == null || string.IsNullOrEmpty(definition.actorTypeId))
                return;
            if (!byActorTypeId.ContainsKey(definition.actorTypeId))
                byActorTypeId.Add(definition.actorTypeId, definition);
        }
    }
}
