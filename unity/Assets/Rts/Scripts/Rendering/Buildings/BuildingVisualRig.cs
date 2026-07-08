using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Buildings
{
    public sealed class BuildingVisualRig : MonoBehaviour
    {
        public Transform visualRoot;
        public List<Transform> poweredLights = new List<Transform>();
        public Transform machinery;
        public Transform door;
        public Vector3 doorOpenLocalDirection = Vector3.up;
        public Transform productionIndicator;
        public Transform turbine;
        public List<Transform> extraTurbines = new List<Transform>();
        public Transform radarDish;
        public Transform craneArm;
        public Transform repairArmLeft;
        public Transform repairArmRight;
        public Transform dockPump;
        public Transform turretOrBarrel;
        public Transform warningLight;
        public Transform damageSmoke;

        public bool HasAnimatedParts
        {
            get
            {
                return (poweredLights != null && poweredLights.Count > 0)
                    || machinery != null
                    || door != null
                    || productionIndicator != null
                    || turbine != null
                    || (extraTurbines != null && extraTurbines.Count > 0)
                    || radarDish != null
                    || craneArm != null
                    || repairArmLeft != null
                    || repairArmRight != null
                    || dockPump != null
                    || turretOrBarrel != null
                    || warningLight != null
                    || damageSmoke != null;
            }
        }

        public BuildingPlaceholderPartFactory.PartSet CreatePartSet()
        {
            var set = new BuildingPlaceholderPartFactory.PartSet();
            set.Root = visualRoot == null ? gameObject : visualRoot.gameObject;

            if (poweredLights != null)
                for (var i = 0; i < poweredLights.Count; i++)
                    if (poweredLights[i] != null)
                        set.Lights.Add(poweredLights[i]);

            set.Machinery = machinery;
            set.Door = door;
            set.DoorOpenLocalDirection = doorOpenLocalDirection.sqrMagnitude < 0.001f ? Vector3.up : doorOpenLocalDirection.normalized;
            set.ProductionIndicator = productionIndicator;
            set.Turbine = turbine;
            if (extraTurbines != null)
                for (var i = 0; i < extraTurbines.Count; i++)
                    if (extraTurbines[i] != null)
                        set.ExtraTurbines.Add(extraTurbines[i]);
            set.RadarDish = radarDish;
            set.CraneArm = craneArm;
            set.RepairArmLeft = repairArmLeft;
            set.RepairArmRight = repairArmRight;
            set.DockPump = dockPump;
            set.TurretOrBarrel = turretOrBarrel;
            set.WarningLight = warningLight;
            set.DamageSmoke = damageSmoke;
            return set;
        }
    }
}
