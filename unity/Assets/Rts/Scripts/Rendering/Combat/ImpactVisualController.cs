using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Combat
{
    public sealed class ImpactVisualController : MonoBehaviour
    {
        public int ActiveCount { get; private set; }

        public void Spawn(Vector3 position, CombatVisualProfile profile)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            obj.name = "Impact Placeholder";
            obj.transform.SetParent(transform, false);
            obj.transform.position = position + Vector3.up * 0.08f;
            obj.transform.localScale = new Vector3(0.42f, 0.035f, 0.42f);
            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = CreateMaterial(profile == null ? new Color(1f, 0.28f, 0.1f, 0.9f) : profile.color);
            var collider = obj.GetComponent<Collider>();
            if (collider != null)
                CombatObjectUtility.DestroyObject(collider);
            var timed = obj.AddComponent<TimedCombatMarker>();
            timed.duration = profile == null ? 0.35f : profile.impactDuration;
            ActiveCount++;
        }

        static Material CreateMaterial(Color color)
        {
            var material = new Material(Shader.Find("Sprites/Default"));
            material.color = color;
            return material;
        }
    }
}
