using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Combat
{
    public sealed class DamageVisualController : MonoBehaviour
    {
        public int ActiveCount { get; private set; }

        public void Spawn(Vector3 position, int damage)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = "Damage Marker " + damage;
            obj.transform.SetParent(transform, false);
            obj.transform.position = position + Vector3.up * 1.1f;
            obj.transform.localScale = new Vector3(0.18f, 0.18f, 0.18f);
            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = CreateMaterial(new Color(1f, 0.12f, 0.12f, 0.95f));
            var collider = obj.GetComponent<Collider>();
            if (collider != null)
                CombatObjectUtility.DestroyObject(collider);
            var timed = obj.AddComponent<TimedCombatMarker>();
            timed.duration = 0.55f;
            timed.floatUp = true;
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
