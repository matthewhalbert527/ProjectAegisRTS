using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Combat
{
    public sealed class MuzzleFlashVisualController : MonoBehaviour
    {
        public int ActiveCount { get; private set; }

        public void Spawn(Vector3 position, CombatVisualProfile profile)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.name = "Muzzle Flash Placeholder";
            obj.transform.SetParent(transform, false);
            obj.transform.position = position + Vector3.up * 0.65f;
            obj.transform.localScale = Vector3.one * 0.28f;
            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = CreateMaterial(profile == null ? new Color(1f, 0.82f, 0.18f, 1f) : profile.color);
            var collider = obj.GetComponent<Collider>();
            if (collider != null)
                CombatObjectUtility.DestroyObject(collider);
            var timed = obj.AddComponent<TimedCombatMarker>();
            timed.duration = profile == null ? 0.16f : profile.muzzleFlashDuration;
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
