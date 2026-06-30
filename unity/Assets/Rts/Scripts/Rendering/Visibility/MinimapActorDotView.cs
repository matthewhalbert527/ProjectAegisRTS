using ProjectAegisRTS.Snapshots;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Visibility
{
    public sealed class MinimapActorDotView : MonoBehaviour
    {
        Renderer cachedRenderer;
        Material ownMaterial;
        Material enemyMaterial;

        public int ActorId { get; private set; }

        public void Apply(MinimapActorDotSnapshot dot, Vector3 position, float cellSize)
        {
            ActorId = dot.ActorId;
            EnsureMaterials();
            if (cachedRenderer == null)
                cachedRenderer = GetComponent<Renderer>();

            transform.position = position;
            transform.localScale = Vector3.one * Mathf.Max(0.08f, cellSize);
            if (cachedRenderer != null)
                cachedRenderer.sharedMaterial = dot.IsEnemy ? enemyMaterial : ownMaterial;
        }

        void EnsureMaterials()
        {
            if (ownMaterial == null)
            {
                ownMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                ownMaterial.color = new Color(0.1f, 0.85f, 0.75f, 1f);
            }

            if (enemyMaterial == null)
            {
                enemyMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                enemyMaterial.color = new Color(0.95f, 0.22f, 0.18f, 1f);
            }
        }
    }
}
