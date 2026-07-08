using ProjectAegisRTS.Snapshots;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Combat
{
    public sealed class ProjectileViewBehaviour : MonoBehaviour
    {
        GameObject body;
        LineRenderer tracer;
        CombatVisualProfile activeProfile;

        public int ProjectileId { get; private set; }
        public string WeaponId { get; private set; }
        public int SourceActorId { get; private set; }
        public int TargetActorId { get; private set; }
        public Vector3 CurrentWorldPosition { get; private set; }
        public Vector3 TargetWorldPosition { get; private set; }
        public bool HasImpacted { get; private set; }

        public void ApplySnapshot(ProjectileSnapshot snapshot, Vector3 currentWorld, Vector3 targetWorld, CombatVisualProfile profile)
        {
            ProjectileId = snapshot.ProjectileId;
            WeaponId = snapshot.WeaponId;
            SourceActorId = snapshot.SourceActorId;
            TargetActorId = snapshot.TargetActorId;
            CurrentWorldPosition = currentWorld;
            TargetWorldPosition = targetWorld;
            HasImpacted = snapshot.HasImpacted;
            activeProfile = profile;

            EnsureVisuals();
            transform.position = currentWorld + Vector3.up * 0.45f;
            body.transform.localScale = Vector3.one * (profile == null ? 0.18f : profile.projectileScale);
            var renderer = body.GetComponent<Renderer>();
            if (renderer != null && profile != null)
            {
                if (profile.material != null)
                    renderer.sharedMaterial = profile.material;
                else
                    renderer.sharedMaterial.color = profile.color;
            }

            if (tracer != null)
            {
                tracer.SetPosition(0, transform.position);
                var direction = (targetWorld - currentWorld).normalized;
                if (direction.sqrMagnitude < 0.001f)
                    direction = transform.forward;
                var length = profile == null ? 0.7f : profile.tracerLength;
                tracer.SetPosition(1, transform.position - direction * length);
            }
        }

        void EnsureVisuals()
        {
            if (body == null)
            {
                body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                body.name = "Projectile Body";
                body.transform.SetParent(transform, false);
                var collider = body.GetComponent<Collider>();
                if (collider != null)
                    CombatObjectUtility.DestroyObject(collider);
            }

            if (tracer == null)
            {
                tracer = gameObject.AddComponent<LineRenderer>();
                tracer.positionCount = 2;
                tracer.widthMultiplier = 0.035f;
                tracer.material = new Material(Shader.Find("Sprites/Default"));
                tracer.startColor = activeProfile == null ? Color.white : activeProfile.color;
                tracer.endColor = new Color(1f, 1f, 1f, 0f);
            }
        }
    }
}
