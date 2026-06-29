using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Art
{
    public sealed class ActorPrefabSocket : MonoBehaviour
    {
        public ActorPrefabSocketKind socketKind = ActorPrefabSocketKind.Root;
        public string socketName;
        public string actorTypeId;
        [TextArea(1, 3)] public string notes;
    }
}
