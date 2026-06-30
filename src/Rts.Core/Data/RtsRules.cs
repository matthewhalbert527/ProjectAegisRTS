using System.Collections.Generic;
using ProjectAegisRTS.Terrain;

namespace ProjectAegisRTS.Data
{
    public sealed class RtsRules
    {
        readonly Dictionary<string, ActorDefinition> definitions;
        readonly Dictionary<TerrainKind, TerrainDefinition> terrainDefinitions;

        public RtsRules(IEnumerable<ActorDefinition> actorDefinitions)
            : this(actorDefinitions, TerrainCatalog.CreateDefaultDefinitions())
        {
        }

        public RtsRules(IEnumerable<ActorDefinition> actorDefinitions, IEnumerable<TerrainDefinition> terrainDefinitions)
        {
            definitions = new Dictionary<string, ActorDefinition>();
            foreach (var definition in actorDefinitions)
                definitions.Add(definition.TypeId, definition);

            this.terrainDefinitions = new Dictionary<TerrainKind, TerrainDefinition>();
            foreach (var terrainDefinition in terrainDefinitions)
                this.terrainDefinitions[terrainDefinition.Kind] = terrainDefinition;
        }

        public IReadOnlyDictionary<string, ActorDefinition> ActorDefinitions
        {
            get { return definitions; }
        }

        public IReadOnlyDictionary<TerrainKind, TerrainDefinition> TerrainDefinitions
        {
            get { return terrainDefinitions; }
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

        public TerrainDefinition GetTerrainDefinition(TerrainKind kind)
        {
            TerrainDefinition definition;
            if (!terrainDefinitions.TryGetValue(kind, out definition))
                throw new KeyNotFoundException("Unknown terrain definition: " + kind);
            return definition;
        }

        public bool TryGetTerrainDefinition(TerrainKind kind, out TerrainDefinition definition)
        {
            return terrainDefinitions.TryGetValue(kind, out definition);
        }
    }
}
