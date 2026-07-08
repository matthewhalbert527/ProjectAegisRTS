using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Motion
{
    public sealed class ActorVisualMotionController : MonoBehaviour
    {
        VisualMotionProfile activeProfile;
        Vector3 previousTargetPosition;
        Vector3 targetPosition;
        Vector3 visualPosition;
        float currentVisualFacing;
        float previousVisualFacing;
        float currentVisualSpeed;
        float normalizedVisualSpeed;
        bool initialized;
        int actorId;
        string actorTypeId = string.Empty;

        public float CurrentVisualSpeed { get { return currentVisualSpeed; } }
        public float NormalizedVisualSpeed { get { return normalizedVisualSpeed; } }
        public float CurrentVisualFacing { get { return currentVisualFacing; } }
        public VisualMotionState CurrentMotionState { get; private set; }
        public VisualMotionProfile ActiveProfile { get { return activeProfile; } }
        public Vector3 TargetPosition { get { return targetPosition; } }
        public Vector3 VisualPosition { get { return visualPosition; } }
        public bool IsMoving { get { return CurrentMotionState == VisualMotionState.Starting || CurrentMotionState == VisualMotionState.Moving || CurrentMotionState == VisualMotionState.Braking || CurrentMotionState == VisualMotionState.Turning || CurrentMotionState == VisualMotionState.Arriving; } }
        public float VisualTurnRate { get; private set; }
        public int ActorId { get { return actorId; } }
        public string ActorTypeId { get { return actorTypeId; } }

        public void Initialize(int id, string typeId, VisualMotionProfile profile)
        {
            actorId = id;
            actorTypeId = typeId ?? string.Empty;
            activeProfile = profile != null ? profile : VisualMotionProfile.CreateRuntimeDefault("runtime_default_vehicle", VisualMotionCategory.Vehicle);
            CurrentMotionState = VisualMotionState.Idle;
        }

        public void ApplySnapshot(ActorSnapshot snapshot, BoardCoordinateMapper boardMapper)
        {
            if (snapshot == null || boardMapper == null)
                return;

            ApplySnapshot(snapshot, boardMapper.FixedWorldToBoardWorld(snapshot.FixedWorldPosition));
        }

        public void ApplySnapshot(ActorSnapshot snapshot, Vector3 worldTargetPosition)
        {
            if (snapshot == null)
                return;

            if (!initialized)
            {
                previousTargetPosition = worldTargetPosition;
                targetPosition = worldTargetPosition;
                visualPosition = worldTargetPosition;
                currentVisualFacing = snapshot.FacingDegrees;
                previousVisualFacing = currentVisualFacing;
                transform.position = visualPosition;
                transform.rotation = Quaternion.Euler(0f, currentVisualFacing, 0f);
                initialized = true;
                CurrentMotionState = VisualMotionState.Idle;
                return;
            }

            if ((worldTargetPosition - targetPosition).sqrMagnitude > 0.0001f)
            {
                previousTargetPosition = targetPosition;
                targetPosition = worldTargetPosition;
            }

            if (snapshot.IsProducing)
                CurrentMotionState = VisualMotionState.Producing;
        }

        public void TickVisual(float deltaTime)
        {
            if (!initialized)
                return;

            if (deltaTime <= 0f)
                deltaTime = 0.016f;

            previousVisualFacing = currentVisualFacing;
            var profile = activeProfile != null ? activeProfile : VisualMotionProfile.CreateRuntimeDefault("runtime_default_vehicle", VisualMotionCategory.Vehicle);
            var toTarget = targetPosition - visualPosition;
            var distance = toTarget.magnitude;
            var maxSpeed = Mathf.Max(0f, profile.maxVisualSpeed);
            var desiredSpeed = distance <= profile.visualArrivalDistance ? 0f : maxSpeed;
            var smoothing = desiredSpeed > currentVisualSpeed ? profile.accelerationSmoothing : profile.brakingSmoothing;
            currentVisualSpeed = Mathf.MoveTowards(currentVisualSpeed, desiredSpeed, Mathf.Max(0.01f, smoothing) * deltaTime);

            if (maxSpeed <= 0.001f)
            {
                visualPosition = targetPosition;
                currentVisualSpeed = 0f;
            }
            else if (distance > 0.0001f)
            {
                var step = Mathf.Min(distance, currentVisualSpeed * deltaTime);
                visualPosition += toTarget.normalized * step;
            }

            normalizedVisualSpeed = maxSpeed <= 0.001f ? 0f : Mathf.Clamp01(currentVisualSpeed / maxSpeed);

            var targetFacing = Quaternion.LookRotation(TargetFacingDirection(), Vector3.up).eulerAngles.y;
            if (distance <= 0.001f)
                targetFacing = currentVisualFacing;

            currentVisualFacing = Mathf.LerpAngle(currentVisualFacing, targetFacing, Mathf.Clamp01(Mathf.Max(0.01f, profile.turnSmoothing) * deltaTime));
            VisualTurnRate = Mathf.DeltaAngle(previousVisualFacing, currentVisualFacing) / deltaTime;

            transform.position = visualPosition;
            transform.rotation = Quaternion.Euler(0f, currentVisualFacing, 0f);
            CurrentMotionState = DetermineState(distance, desiredSpeed, profile);
        }

        public void SnapToTarget()
        {
            visualPosition = targetPosition;
            currentVisualSpeed = 0f;
            normalizedVisualSpeed = 0f;
            transform.position = visualPosition;
        }

        public void ResetVisualState()
        {
            initialized = false;
            currentVisualSpeed = 0f;
            normalizedVisualSpeed = 0f;
            CurrentMotionState = VisualMotionState.Idle;
        }

        Vector3 TargetFacingDirection()
        {
            var direction = targetPosition - visualPosition;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
                return transform.forward.sqrMagnitude > 0.0001f ? transform.forward : Vector3.forward;
            return direction.normalized;
        }

        VisualMotionState DetermineState(float distance, float desiredSpeed, VisualMotionProfile profile)
        {
            if (profile.category == VisualMotionCategory.Aircraft && distance <= profile.visualArrivalDistance && profile.aircraftHoverBobAmount > 0f)
                return VisualMotionState.Hovering;
            if (distance <= profile.visualArrivalDistance)
                return currentVisualSpeed > 0.05f ? VisualMotionState.Arriving : VisualMotionState.Idle;
            if (Mathf.Abs(VisualTurnRate) > profile.minimumTurnSpeed * 120f)
                return VisualMotionState.Turning;
            if (currentVisualSpeed < desiredSpeed * 0.35f)
                return VisualMotionState.Starting;
            if (desiredSpeed < currentVisualSpeed)
                return VisualMotionState.Braking;
            return VisualMotionState.Moving;
        }
    }
}
