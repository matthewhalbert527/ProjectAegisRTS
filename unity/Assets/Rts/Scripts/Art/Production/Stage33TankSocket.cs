using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Art.Production
{
    public enum Stage33TankSocketKind
    {
        Root,
        BodyRoot,
        VisualRoot,
        SelectionAnchor,
        HealthBarAnchor,
        UiAnchor,
        TurretRoot,
        BarrelRoot,
        MuzzlePrimary,
        MuzzleSecondary,
        TrackLeft,
        TrackRight,
        WheelLeft,
        WheelRight,
        VfxSmoke,
        VfxExplosion,
        AudioLoop,
        AimPivot,
        ArmorPanel,
        Detail
    }

    /// <summary>
    /// Lightweight socket marker for generated tank source prefabs.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Stage33TankSocket : MonoBehaviour
    {
        public Stage33TankSocketKind socketKind;
        public string socketName;
        public string actorTypeId;
        public bool required = true;
    }
}
