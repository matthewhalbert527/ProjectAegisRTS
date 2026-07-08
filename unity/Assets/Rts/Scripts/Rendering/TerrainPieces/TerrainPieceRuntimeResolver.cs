using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.TerrainPieces
{
    public sealed class TerrainPieceRuntimeResolver : MonoBehaviour
    {
        public TerrainPieceLibrary pieceLibrary;
        public Stage32TerrainPieceMaterialLibrary materialLibrary;

        public TerrainPieceDefinition ResolvePiece(string pieceId)
        {
            if (pieceLibrary == null)
                return null;
            return pieceLibrary.GetDefinition(pieceId);
        }

        public GameObject InstantiatePiece(string pieceId, Transform parent)
        {
            var definition = ResolvePiece(pieceId);
            if (definition == null || definition.prefab == null)
                return null;

            var instance = Instantiate(definition.prefab, parent, false);
            instance.name = "Stage32 " + definition.pieceId;
            RemoveColliders(instance);
            return instance;
        }

        public void ApplyTint(GameObject instance, Color tint)
        {
            if (instance == null || tint == Color.white)
                return;

            var block = new MaterialPropertyBlock();
            var renderers = instance.GetComponentsInChildren<Renderer>(true);
            for (var i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                renderer.GetPropertyBlock(block);
                block.SetColor("_BaseColor", tint);
                block.SetColor("_Color", tint);
                renderer.SetPropertyBlock(block);
            }
        }

        static void RemoveColliders(GameObject instance)
        {
            var colliders = instance.GetComponentsInChildren<Collider>(true);
            for (var i = colliders.Length - 1; i >= 0; i--)
                DestroyUnityObject(colliders[i]);
        }

        static void DestroyUnityObject(Object target)
        {
            if (target == null)
                return;
            if (Application.isPlaying)
                Destroy(target);
            else
                DestroyImmediate(target);
        }
    }
}
