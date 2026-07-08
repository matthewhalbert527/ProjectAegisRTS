using System.Collections.Generic;

namespace ProjectAegisRTS.Support
{
    public enum SupportPowerTargetKind
    {
        None,
        Cell
    }

    public enum SupportPowerEffectKind
    {
        None,
        RevealScan,
        EmergencyRepairPulse,
        PrecisionStrikePlaceholder,
        ProductionBoostPlaceholder,
        PowerSurgePlaceholder
    }

    public sealed class SupportPowerDefinition
    {
        public string PowerId { get; private set; }
        public string DisplayName { get; private set; }
        public SupportPowerEffectKind EffectKind { get; private set; }
        public SupportPowerTargetKind TargetKind { get; private set; }
        public int CooldownTicks { get; private set; }
        public int RadiusCells { get; private set; }
        public int Amount { get; private set; }
        public IReadOnlyList<string> PrerequisiteTypeIds { get; private set; }

        public SupportPowerDefinition(
            string powerId,
            string displayName,
            SupportPowerEffectKind effectKind,
            SupportPowerTargetKind targetKind,
            int cooldownTicks,
            int radiusCells,
            int amount,
            IReadOnlyList<string> prerequisiteTypeIds)
        {
            PowerId = powerId;
            DisplayName = displayName;
            EffectKind = effectKind;
            TargetKind = targetKind;
            CooldownTicks = cooldownTicks < 0 ? 0 : cooldownTicks;
            RadiusCells = radiusCells < 0 ? 0 : radiusCells;
            Amount = amount < 0 ? 0 : amount;

            var prerequisites = new List<string>();
            if (prerequisiteTypeIds != null)
            {
                for (var i = 0; i < prerequisiteTypeIds.Count; i++)
                    if (!string.IsNullOrEmpty(prerequisiteTypeIds[i]))
                        prerequisites.Add(prerequisiteTypeIds[i]);
            }

            PrerequisiteTypeIds = prerequisites;
        }
    }

    public sealed class SupportPowerState
    {
        public string PowerId { get; private set; }
        public int CooldownRemainingTicks { get; private set; }
        public int LastActivatedTick { get; private set; }
        public int ActivationCount { get; private set; }

        public SupportPowerState(string powerId)
        {
            PowerId = powerId;
            LastActivatedTick = -1;
        }

        public void MarkActivated(int tick, int cooldownTicks)
        {
            LastActivatedTick = tick;
            ActivationCount++;
            CooldownRemainingTicks = cooldownTicks < 0 ? 0 : cooldownTicks;
        }

        public void TickCooldown()
        {
            if (CooldownRemainingTicks > 0)
                CooldownRemainingTicks--;
        }
    }

    public static class SupportPowerCatalog
    {
        public static IReadOnlyList<SupportPowerDefinition> CreateDefaultDefinitions()
        {
            return new[]
            {
                new SupportPowerDefinition(
                    "reveal_scan",
                    "Reveal Scan",
                    SupportPowerEffectKind.RevealScan,
                    SupportPowerTargetKind.Cell,
                    180,
                    6,
                    0,
                    new[] { "comm_center" }),
                new SupportPowerDefinition(
                    "emergency_repair_pulse",
                    "Emergency Repair Pulse",
                    SupportPowerEffectKind.EmergencyRepairPulse,
                    SupportPowerTargetKind.Cell,
                    240,
                    4,
                    120,
                    new[] { "repair_bay" }),
                new SupportPowerDefinition(
                    "precision_strike",
                    "Precision Strike",
                    SupportPowerEffectKind.PrecisionStrikePlaceholder,
                    SupportPowerTargetKind.Cell,
                    300,
                    2,
                    0,
                    new[] { "tech_center" }),
                new SupportPowerDefinition(
                    "production_boost",
                    "Production Boost",
                    SupportPowerEffectKind.ProductionBoostPlaceholder,
                    SupportPowerTargetKind.None,
                    300,
                    0,
                    0,
                    new[] { "tech_center" }),
                new SupportPowerDefinition(
                    "power_surge",
                    "Power Surge",
                    SupportPowerEffectKind.PowerSurgePlaceholder,
                    SupportPowerTargetKind.None,
                    300,
                    0,
                    0,
                    new[] { "advanced_power_plant" })
            };
        }
    }
}
