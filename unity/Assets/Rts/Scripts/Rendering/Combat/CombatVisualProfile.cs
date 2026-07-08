using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Combat
{
    [CreateAssetMenu(menuName = "ProjectAegisRTS/Combat Visual Profile")]
    public sealed class CombatVisualProfile : ScriptableObject
    {
        public string profileId = "impact_placeholder";
        public string displayName = "Impact Placeholder";
        public CombatVisualCategory category = CombatVisualCategory.Impact;
        public GameObject projectilePrefab;
        public GameObject impactPrefab;
        public GameObject muzzleFlashPrefab;
        public GameObject deathPrefab;
        public float projectileSpeedVisual = 16f;
        public float projectileScale = 0.18f;
        public float impactDuration = 0.35f;
        public float muzzleFlashDuration = 0.16f;
        public float deathVisualDuration = 2f;
        public float tracerLength = 0.7f;
        public bool useArc;
        public float arcHeight = 0.35f;
        public Color color = Color.white;
        public Material material;
    }
}
