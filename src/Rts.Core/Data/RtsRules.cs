using System.Collections.Generic;
using ProjectAegisRTS.Support;
using ProjectAegisRTS.Terrain;

namespace ProjectAegisRTS.Data
{
    public sealed class RtsRules
    {
        readonly Dictionary<string, ActorDefinition> definitions;
        readonly Dictionary<TerrainKind, TerrainDefinition> terrainDefinitions;
        readonly Dictionary<string, SupportPowerDefinition> supportPowerDefinitionsById;
        readonly List<SupportPowerDefinition> supportPowerDefinitions;

        public RtsRules(IEnumerable<ActorDefinition> actorDefinitions)
            : this(actorDefinitions, TerrainCatalog.CreateDefaultDefinitions(), SupportPowerCatalog.CreateDefaultDefinitions())
        {
        }

        public RtsRules(IEnumerable<ActorDefinition> actorDefinitions, IEnumerable<TerrainDefinition> terrainDefinitions)
            : this(actorDefinitions, terrainDefinitions, SupportPowerCatalog.CreateDefaultDefinitions())
        {
        }

        public RtsRules(IEnumerable<ActorDefinition> actorDefinitions, IEnumerable<TerrainDefinition> terrainDefinitions, IEnumerable<SupportPowerDefinition> supportPowerDefinitions)
        {
            definitions = new Dictionary<string, ActorDefinition>();
            foreach (var definition in actorDefinitions)
                definitions.Add(definition.TypeId, definition);

            this.terrainDefinitions = new Dictionary<TerrainKind, TerrainDefinition>();
            foreach (var terrainDefinition in terrainDefinitions)
                this.terrainDefinitions[terrainDefinition.Kind] = terrainDefinition;

            this.supportPowerDefinitions = new List<SupportPowerDefinition>();
            supportPowerDefinitionsById = new Dictionary<string, SupportPowerDefinition>();
            if (supportPowerDefinitions != null)
            {
                foreach (var supportPowerDefinition in supportPowerDefinitions)
                {
                    this.supportPowerDefinitions.Add(supportPowerDefinition);
                    supportPowerDefinitionsById[supportPowerDefinition.PowerId] = supportPowerDefinition;
                }
            }
        }

        public IReadOnlyDictionary<string, ActorDefinition> ActorDefinitions
        {
            get { return definitions; }
        }

        public IReadOnlyDictionary<TerrainKind, TerrainDefinition> TerrainDefinitions
        {
            get { return terrainDefinitions; }
        }

        public IReadOnlyList<SupportPowerDefinition> SupportPowerDefinitions
        {
            get { return supportPowerDefinitions; }
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

        public SupportPowerDefinition GetSupportPowerDefinition(string powerId)
        {
            SupportPowerDefinition definition;
            if (!supportPowerDefinitionsById.TryGetValue(powerId, out definition))
                throw new KeyNotFoundException("Unknown support power definition: " + powerId);
            return definition;
        }

        public bool TryGetSupportPowerDefinition(string powerId, out SupportPowerDefinition definition)
        {
            return supportPowerDefinitionsById.TryGetValue(powerId, out definition);
        }
    }
}
