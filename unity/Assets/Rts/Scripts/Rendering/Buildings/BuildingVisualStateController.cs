using ProjectAegisRTS.Snapshots;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Buildings
{
    public sealed class BuildingVisualStateController : MonoBehaviour
    {
        BuildingVisualProfile activeProfile;
        BuildingPlaceholderPartFactory.PartSet parts;
        Stage7BuildingMaterialLibrary materials;
        int maxHealth = 1;
        bool initialized;
        BuildingAnimationVisualState? debugForcedState;

        public bool standaloneTickInPlayMode;
        public int ActorId { get; private set; }
        public string ActorTypeId { get; private set; }
        public BuildingVisualProfile ActiveProfile { get { return activeProfile; } }
        public BuildingPowerVisualState PowerVisualState { get; private set; }
        public BuildingAnimationVisualState AnimationVisualState { get; private set; }
        public bool LightsActive { get; private set; }
        public bool MachineryActive { get; private set; }
        public bool IsProducing { get; private set; }
        public float ProductionProgress01 { get; private set; }
        public float Health01 { get; private set; } = 1f;
        public bool VisualDebugEnabled { get; set; } = true;
        public BuildingLightVisualController Lights { get; private set; }
        public BuildingMachineryVisualController Machinery { get; private set; }
        public BuildingProductionVisualController Production { get; private set; }
        public BuildingDoorVisualController Door { get; private set; }
        public BuildingDamageVisualController Damage { get; private set; }
        public BuildingSpecificLoopController SpecificLoop { get; private set; }
        public bool HasGeneratedParts { get { return parts != null && parts.Root != null; } }
        public float DoorOpen01 { get { return Door == null ? 0f : Door.DoorOpen01; } }
        public float MachinerySpeed { get { return Machinery == null ? 0f : Machinery.CurrentMachinerySpeed; } }
        public float MachineryPhase { get { return Machinery == null ? 0f : Machinery.CurrentLoopPhase; } }

        void Update()
        {
            if (standaloneTickInPlayMode)
                TickVisual(Time.deltaTime);
        }

        public void Initialize(int actorId, string actorTypeId, BuildingVisualProfile profile)
        {
            ActorId = actorId;
            ActorTypeId = actorTypeId ?? string.Empty;
            if (activeProfile != profile || !initialized)
            {
                activeProfile = profile != null ? profile : BuildingVisualProfile.CreateRuntimeDefault(ActorTypeId, BuildingVisualProfileLibrary.CategoryForActor(ActorTypeId));
                EnsureParts();
                PowerVisualState = BuildingPowerVisualState.Normal;
                AnimationVisualState = BuildingAnimationVisualState.PoweredIdle;
                LightsActive = true;
                MachineryActive = true;
                Health01 = 1f;
                initialized = true;
            }
        }

        public void ApplySnapshot(ActorSnapshot snapshot)
        {
            ApplySnapshot(snapshot, Mathf.Max(1, snapshot == null ? 1 : snapshot.Health));
        }

        public void ApplySnapshot(ActorSnapshot snapshot, int actorMaxHealth)
        {
            if (snapshot == null)
                return;

            maxHealth = Mathf.Max(1, actorMaxHealth);
            LightsActive = snapshot.LightsActive;
            MachineryActive = snapshot.MachineryActive;
            IsProducing = snapshot.IsProducing;
            Health01 = Mathf.Clamp01(snapshot.Health / (float)maxHealth);
            ProductionProgress01 = snapshot.ProductionProgress <= 0 ? 0f : Mathf.Clamp01(snapshot.ProductionProgress / 100f);

            if (!snapshot.IsPowered)
                PowerVisualState = BuildingPowerVisualState.Offline;
            else if (snapshot.IsLowPower)
                PowerVisualState = BuildingPowerVisualState.LowPower;
            else
                PowerVisualState = BuildingPowerVisualState.Normal;

            AnimationVisualState = DeriveAnimationState();
        }

        public void TickVisual(float deltaTime)
        {
            if (!initialized)
                return;

            var effectiveAnimation = debugForcedState.HasValue ? debugForcedState.Value : AnimationVisualState;
            var forceDamaged = effectiveAnimation == BuildingAnimationVisualState.Damaged;
            var forceDestroyed = effectiveAnimation == BuildingAnimationVisualState.Destroyed;
            var warning = forceDamaged || forceDestroyed || PowerVisualState == BuildingPowerVisualState.LowPower;

            if (Lights != null)
                Lights.TickVisual(deltaTime, PowerVisualState, LightsActive, warning);
            if (Machinery != null)
                Machinery.TickVisual(deltaTime, PowerVisualState, MachineryActive);
            if (Production != null)
                Production.TickVisual(deltaTime, IsProducing || effectiveAnimation == BuildingAnimationVisualState.Producing, ProductionProgress01);
            if (Door != null)
                Door.TickVisual(deltaTime, IsProducing || effectiveAnimation == BuildingAnimationVisualState.Producing);
            if (Damage != null)
                Damage.TickVisual(Health01, forceDamaged, forceDestroyed);
            if (SpecificLoop != null)
                SpecificLoop.TickVisual(deltaTime, PowerVisualState, MachineryActive, IsProducing);
        }

        public void ResetVisualState()
        {
            initialized = false;
            parts = null;
            activeProfile = null;
            var existing = transform.Find("Stage7 Building Visual Parts");
            if (existing != null)
                BuildingPlaceholderPartFactory.DestroyUnityObject(existing.gameObject);
        }

        public void SetDebugForcedState(BuildingAnimationVisualState? forcedState)
        {
            debugForcedState = forcedState;
            if (debugForcedState.HasValue)
                AnimationVisualState = debugForcedState.Value;
        }

        void EnsureParts()
        {
            materials = Stage7BuildingMaterialLibrary.Create();
            parts = BuildingPlaceholderPartFactory.CreateParts(transform, activeProfile, materials);

            Lights = GetOrAdd<BuildingLightVisualController>();
            Lights.Initialize(parts, activeProfile, materials);
            Machinery = GetOrAdd<BuildingMachineryVisualController>();
            Machinery.Initialize(parts, activeProfile);
            Production = GetOrAdd<BuildingProductionVisualController>();
            Production.Initialize(parts, activeProfile);
            Door = GetOrAdd<BuildingDoorVisualController>();
            Door.Initialize(parts, activeProfile);
            Damage = GetOrAdd<BuildingDamageVisualController>();
            Damage.Initialize(parts, activeProfile);
            SpecificLoop = GetOrAdd<BuildingSpecificLoopController>();
            SpecificLoop.Initialize(parts, activeProfile, ActorTypeId);
        }

        BuildingAnimationVisualState DeriveAnimationState()
        {
            if (PowerVisualState == BuildingPowerVisualState.Offline)
                return BuildingAnimationVisualState.Offline;
            if (PowerVisualState == BuildingPowerVisualState.LowPower)
                return BuildingAnimationVisualState.LowPowerIdle;
            if (Health01 <= (activeProfile == null ? 0.05f : activeProfile.destroyedHealthThreshold01))
                return BuildingAnimationVisualState.Destroyed;
            if (Health01 <= (activeProfile == null ? 0.5f : activeProfile.damagedHealthThreshold01))
                return BuildingAnimationVisualState.Damaged;
            if (IsProducing)
                return ProductionProgress01 >= 0.99f ? BuildingAnimationVisualState.ProductionComplete : BuildingAnimationVisualState.Producing;
            return LightsActive ? BuildingAnimationVisualState.PoweredIdle : BuildingAnimationVisualState.Idle;
        }

        T GetOrAdd<T>() where T : Component
        {
            var component = GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }
    }
}
