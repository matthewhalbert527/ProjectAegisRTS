using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.TerrainPieces
{
    [CreateAssetMenu(menuName = "ProjectAegisRTS/Stage32/Terrain Piece Library")]
    public sealed class TerrainPieceLibrary : ScriptableObject
    {
        public List<TerrainPieceDefinition> definitions = new List<TerrainPieceDefinition>();

        readonly Dictionary<string, TerrainPieceDefinition> byId = new Dictionary<string, TerrainPieceDefinition>();
        bool initialized;

        public int Count
        {
            get
            {
                EnsureInitialized();
                return byId.Count;
            }
        }

        public void EnsureInitialized()
        {
            if (initialized)
                return;

            initialized = true;
            byId.Clear();
            if (definitions == null)
                definitions = new List<TerrainPieceDefinition>();

            for (var i = 0; i < definitions.Count; i++)
            {
                var definition = definitions[i];
                if (definition == null || string.IsNullOrEmpty(definition.pieceId) || byId.ContainsKey(definition.pieceId))
                    continue;
                byId.Add(definition.pieceId, definition);
            }
        }

        public void RebuildLookup()
        {
            initialized = false;
            EnsureInitialized();
        }

        public TerrainPieceDefinition GetDefinition(string pieceId)
        {
            EnsureInitialized();
            TerrainPieceDefinition definition;
            return !string.IsNullOrEmpty(pieceId) && byId.TryGetValue(pieceId, out definition) ? definition : null;
        }

        public IReadOnlyList<TerrainPieceDefinition> GetDefinitions()
        {
            EnsureInitialized();
            return definitions;
        }

        public int CountByCategory(TerrainPieceCategory category)
        {
            EnsureInitialized();
            var count = 0;
            for (var i = 0; i < definitions.Count; i++)
                if (definitions[i] != null && definitions[i].category == category)
                    count++;
            return count;
        }
    }
}
