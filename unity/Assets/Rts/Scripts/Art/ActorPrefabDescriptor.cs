using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Art
{
    public sealed class ActorPrefabDescriptor : MonoBehaviour
    {
        public string actorTypeId;
        public ActorArtCategory category = ActorArtCategory.Unknown;
        public ActorVisualDefinition sourceDefinition;
        public ActorArtProductionStatus productionStatus = ActorArtProductionStatus.BlockoutGenerated;
        public bool generatedByStage8 = true;
        public bool requiredSocketsPresent;
        public List<ActorPrefabSocketKind> declaredRequiredSockets = new List<ActorPrefabSocketKind>();
        [TextArea(1, 4)] public string notes;

        public ActorPrefabSocket[] GetSockets()
        {
            return GetComponentsInChildren<ActorPrefabSocket>(true);
        }

        public bool TryGetSocket(ActorPrefabSocketKind kind, out Transform socketTransform)
        {
            var sockets = GetSockets();
            for (var i = 0; i < sockets.Length; i++)
                if (sockets[i] != null && sockets[i].socketKind == kind)
                {
                    socketTransform = sockets[i].transform;
                    return true;
                }

            socketTransform = null;
            return false;
        }

        public List<string> ValidateRequiredSockets(IReadOnlyList<ActorPrefabSocketKind> requiredSockets)
        {
            var missing = new List<string>();
            if (requiredSockets == null)
            {
                requiredSocketsPresent = true;
                return missing;
            }

            for (var i = 0; i < requiredSockets.Count; i++)
            {
                Transform socketTransform;
                if (!TryGetSocket(requiredSockets[i], out socketTransform))
                    missing.Add(requiredSockets[i].ToString());
            }

            requiredSocketsPresent = missing.Count == 0;
            return missing;
        }
    }
}
