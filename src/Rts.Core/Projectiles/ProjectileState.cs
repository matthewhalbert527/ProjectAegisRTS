using ProjectAegisRTS.Core;
using ProjectAegisRTS.Data;

namespace ProjectAegisRTS.Projectiles
{
    public sealed class ProjectileState
    {
        public int ProjectileId { get; private set; }
        public int OwnerPlayerId { get; private set; }
        public int SourceActorId { get; private set; }
        public int TargetActorId { get; private set; }
        public string WeaponId { get; private set; }
        public ProjectileKind ProjectileKind { get; private set; }
        public Int2 CurrentPositionFixed { get; set; }
        public Int2 TargetPositionFixed { get; private set; }
        public Int2 TargetCell { get; private set; }
        public int SpeedSubCellsPerTick { get; private set; }
        public int Damage { get; private set; }
        public int CreatedTick { get; private set; }
        public int RemainingLifetimeTicks { get; set; }
        public bool HasImpacted { get; set; }
        public int ImpactTick { get; set; }

        public ProjectileState(
            int projectileId,
            int ownerPlayerId,
            int sourceActorId,
            int targetActorId,
            string weaponId,
            ProjectileKind projectileKind,
            Int2 currentPositionFixed,
            Int2 targetPositionFixed,
            Int2 targetCell,
            int speedSubCellsPerTick,
            int damage,
            int createdTick,
            int remainingLifetimeTicks)
        {
            ProjectileId = projectileId;
            OwnerPlayerId = ownerPlayerId;
            SourceActorId = sourceActorId;
            TargetActorId = targetActorId;
            WeaponId = weaponId;
            ProjectileKind = projectileKind;
            CurrentPositionFixed = currentPositionFixed;
            TargetPositionFixed = targetPositionFixed;
            TargetCell = targetCell;
            SpeedSubCellsPerTick = speedSubCellsPerTick;
            Damage = damage;
            CreatedTick = createdTick;
            RemainingLifetimeTicks = remainingLifetimeTicks;
            HasImpacted = false;
            ImpactTick = -1;
        }
    }
}
