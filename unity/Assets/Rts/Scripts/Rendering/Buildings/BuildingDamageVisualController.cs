using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Buildings
{
    public sealed class BuildingDamageVisualController : MonoBehaviour
    {
        BuildingPlaceholderPartFactory.PartSet parts;
        BuildingVisualProfile profile;

        public float Health01 { get; private set; } = 1f;
        public bool IsDamaged { get; private set; }
        public bool IsDestroyedPlaceholder { get; private set; }

        public void Initialize(BuildingPlaceholderPartFactory.PartSet partSet, BuildingVisualProfile activeProfile)
        {
            parts = partSet;
            profile = activeProfile;
        }

        public void TickVisual(float health01, bool forceDamaged, bool forceDestroyed)
        {
            Health01 = Mathf.Clamp01(health01);
            var damagedThreshold = profile == null ? 0.5f : profile.damagedHealthThreshold01;
            var destroyedThreshold = profile == null ? 0.05f : profile.destroyedHealthThreshold01;
            IsDestroyedPlaceholder = forceDestroyed || Health01 <= destroyedThreshold;
            IsDamaged = forceDamaged || IsDestroyedPlaceholder || Health01 <= damagedThreshold;

            if (parts == null)
                return;

            if (parts.DamageSmoke != null)
            {
                parts.DamageSmoke.gameObject.SetActive(IsDamaged);
                parts.DamageSmoke.localScale = IsDestroyedPlaceholder ? new Vector3(0.45f, 0.28f, 0.45f) : new Vector3(0.28f, 0.2f, 0.28f);
            }
            if (parts.WarningLight != null && IsDestroyedPlaceholder)
                parts.WarningLight.localScale = Vector3.one * 0.22f;
        }
    }
}
