using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Motion
{
    [CreateAssetMenu(fileName = "VisualMotionProfile", menuName = "ProjectAegisRTS/Visual Motion Profile")]
    public sealed class VisualMotionProfile : ScriptableObject
    {
        public string profileId = "default_vehicle";
        public string displayName = "Default Vehicle";
        public VisualMotionCategory category = VisualMotionCategory.Vehicle;
        public float maxVisualSpeed = 5f;
        public float accelerationSmoothing = 8f;
        public float brakingSmoothing = 10f;
        public float turnSmoothing = 8f;
        public float facingLag = 0.12f;
        public float minimumTurnSpeed = 0.08f;
        public float visualArrivalDistance = 0.04f;
        public float suspensionStrength = 0.035f;
        public float trackOrWheelAnimationScale = 3.2f;
        public float infantryStepRate = 5f;
        public float infantryStrideLength = 0.45f;
        public float aircraftBankAmount = 18f;
        public float aircraftAltitudeOffset = 1.4f;
        public float aircraftHoverBobAmount = 0.08f;
        public float turretLag = 7f;
        public float recoilVisualStrength = 0.08f;

        public void Configure(
            string id,
            string name,
            VisualMotionCategory motionCategory,
            float maxSpeed,
            float acceleration,
            float braking,
            float turn,
            float lag,
            float minTurnSpeed,
            float arrivalDistance,
            float suspension,
            float trackScale,
            float stepRate,
            float strideLength,
            float bankAmount,
            float altitudeOffset,
            float hoverBob,
            float turretLagAmount,
            float recoilStrength)
        {
            profileId = id;
            displayName = name;
            category = motionCategory;
            maxVisualSpeed = maxSpeed;
            accelerationSmoothing = acceleration;
            brakingSmoothing = braking;
            turnSmoothing = turn;
            facingLag = lag;
            minimumTurnSpeed = minTurnSpeed;
            visualArrivalDistance = arrivalDistance;
            suspensionStrength = suspension;
            trackOrWheelAnimationScale = trackScale;
            infantryStepRate = stepRate;
            infantryStrideLength = strideLength;
            aircraftBankAmount = bankAmount;
            aircraftAltitudeOffset = altitudeOffset;
            aircraftHoverBobAmount = hoverBob;
            turretLag = turretLagAmount;
            recoilVisualStrength = recoilStrength;
        }

        public static VisualMotionProfile CreateRuntimeDefault(string id, VisualMotionCategory category)
        {
            var profile = CreateInstance<VisualMotionProfile>();
            profile.hideFlags = HideFlags.DontSave;
            ConfigureDefault(profile, id, ToDisplayName(id), category);
            return profile;
        }

        public static void ConfigureDefault(VisualMotionProfile profile, string id, string displayName, VisualMotionCategory category)
        {
            if (profile == null)
                return;

            if (category == VisualMotionCategory.Infantry)
            {
                profile.Configure(id, displayName, category, 3.2f, 12f, 14f, 14f, 0.08f, 0.04f, 0.035f, 0.012f, 1f, 7f, 0.4f, 0f, 0f, 0f, 7f, 0.05f);
                return;
            }

            if (category == VisualMotionCategory.Aircraft)
            {
                profile.Configure(id, displayName, category, 7.5f, 7f, 8f, 5f, 0.18f, 0.12f, 0.08f, 0.01f, 0f, 0f, 0f, 24f, 1.8f, 0.12f, 6f, 0.04f);
                return;
            }

            if (category == VisualMotionCategory.Building || category == VisualMotionCategory.Defense)
            {
                profile.Configure(id, displayName, category, 0f, 1f, 1f, 4f, 0.05f, 0f, 0.01f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 5f, 0.06f);
                return;
            }

            if (category == VisualMotionCategory.Harvester)
            {
                profile.Configure(id, displayName, category, 3.2f, 4.5f, 6f, 5.5f, 0.18f, 0.08f, 0.06f, 0.05f, 2.6f, 0f, 0f, 0f, 0f, 0f, 4.5f, 0.05f);
                return;
            }

            profile.Configure(id, displayName, category, 4.8f, 6.5f, 8.5f, 6f, 0.14f, 0.08f, 0.05f, 0.035f, 3.2f, 0f, 0f, 0f, 0f, 0f, 5.5f, 0.06f);
        }

        static string ToDisplayName(string id)
        {
            if (string.IsNullOrEmpty(id))
                return "Default Motion";

            return id.Replace('_', ' ');
        }
    }
}
