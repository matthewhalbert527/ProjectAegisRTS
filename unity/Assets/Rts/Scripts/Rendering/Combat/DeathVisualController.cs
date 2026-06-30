using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Combat
{
    public sealed class DeathVisualController : MonoBehaviour
    {
        public int ActiveCount { get; private set; }

        public void Spawn(Vector3 position, CombatVisualProfile profile)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            obj.name = "Death Placeholder";
            obj.transform.SetParent(transform, false);
            obj.transform.position = position + Vector3.up * 0.18f;
            obj.transform.localScale = new Vector3(0.75f, 0.12f, 0.75f);
            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = CreateMaterial(new Color(0.12f, 0.12f, 0.12f, 0.92f));
            var collider = obj.GetComponent<Collider>();
            if (collider != null)
                CombatObjectUtility.DestroyObject(collider);
            var timed = obj.AddComponent<TimedCombatMarker>();
            timed.duration = profile == null ? 2f : profile.deathVisualDuration;
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
