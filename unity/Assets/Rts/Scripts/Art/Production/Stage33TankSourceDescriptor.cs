using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Art.Production
{
    /// <summary>
    /// Runtime-safe visual metadata for generated tank source prefabs.
    /// It never owns deterministic gameplay authority.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Stage33TankSourceDescriptor : MonoBehaviour
    {
        public string actorTypeId = "light_tank";
        public string displayName = "Light Tank";
        public string visualTier = "FirstPassProductionSource";
        public Vector2Int fineFootprint = new Vector2Int(4, 4);
        public float metersPerFineCell = 0.5f;
        public float visualLengthMeters = 1.8f;
        public float visualWidthMeters = 1.35f;
        public float visualHeightMeters = 0.75f;
        public bool gridAccurateBase = true;
        public bool hasTopDetail = true;
        public bool hasFrontDetail = true;
        public bool hasBackDetail = true;
        public bool hasLeftDetail = true;
        public bool hasRightDetail = true;
        public bool hasTurret = true;
        public bool hasTracks = true;
        public bool questSafeProxy = true;
        public string notes = "Procedural tank source prefab. Replace with artist-authored model while preserving sockets, pivot, footprint, and fallback.";

        public Transform root;
        public Transform bodyRoot;
        public Transform visualRoot;
        public Transform selectionAnchor;
        public Transform healthBarAnchor;
        public Transform uiAnchor;
        public Transform turretRoot;
        public Transform barrelRoot;
        public Transform muzzlePrimary;
        public Transform trackLeft;
        public Transform trackRight;
        public Transform vfxSmoke;
        public Transform vfxExplosion;

        public bool HasRequiredSockets()
        {
            return bodyRoot != null && visualRoot != null && selectionAnchor != null && healthBarAnchor != null &&
                   uiAnchor != null && turretRoot != null && barrelRoot != null && muzzlePrimary != null && trackLeft != null &&
                   trackRight != null && vfxSmoke != null && vfxExplosion != null;
        }
    }
}
