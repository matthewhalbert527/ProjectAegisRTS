using System.Collections.Generic;

namespace ProjectAegisRTS.Data
{
    public sealed class RtsRules
    {
        readonly Dictionary<string, ActorDefinition> definitions;

        public RtsRules(IEnumerable<ActorDefinition> actorDefinitions)
        {
            definitions = new Dictionary<string, ActorDefinition>();
            foreach (var definition in actorDefinitions)
                definitions.Add(definition.TypeId, definition);
        }

        public IReadOnlyDictionary<string, ActorDefinition> ActorDefinitions
        {
            get { return definitions; }
        }

        public ActorDefinition GetDefinition(string typeId)
        {
            ActorDefinition definition;
            if (!definitions.TryGetValue(typeId, out definition))
                throw new KeyNotFoundException("Unknown actor definition: " + typeId);
            return definition;
        }

        public bool TryGetDefinition(string typeId, out ActorDefinition definition)
        {
            return definitions.TryGetValue(typeId, out definition);
        }
    }
}
