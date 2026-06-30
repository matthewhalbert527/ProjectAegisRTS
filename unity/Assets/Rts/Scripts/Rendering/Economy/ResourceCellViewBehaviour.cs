using ProjectAegisRTS.Snapshots;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Economy
{
    public sealed class ResourceCellViewBehaviour : MonoBehaviour
    {
        GameObject body;

        public int Amount { get; private set; }
        public int MaxAmount { get; private set; }

        public void Apply(ResourceSnapshot snapshot, Vector3 worldPosition)
        {
            EnsureBody();
            Amount = snapshot.Amount;
            MaxAmount = snapshot.MaxAmount;
            transform.position = worldPosition + Vector3.up * 0.04f;
            var normalized = snapshot.MaxAmount <= 0 ? 0f : Mathf.Clamp01(snapshot.Amount / (float)snapshot.MaxAmount);
            body.transform.localScale = new Vector3(0.72f, 0.08f + normalized * 0.32f, 0.72f);
            body.transform.localPosition = Vector3.up * body.transform.localScale.y * 0.5f;
            var renderer = body.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial.color = Color.Lerp(new Color(0.24f, 0.18f, 0.1f, 1f), new Color(0.1f, 0.75f, 0.45f, 1f), normalized);
        }

        void EnsureBody()
        {
            if (body != null)
                return;
            body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Resource Cell Body";
            body.transform.SetParent(transform, false);
            var collider = body.GetComponent<Collider>();
            if (collider != null)
                EconomyObjectUtility.DestroyObject(collider);
            var renderer = body.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
        }
    }
}
