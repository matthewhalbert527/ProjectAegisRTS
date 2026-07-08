using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Art.Production
{
    [DisallowMultipleComponent]
    public sealed class ProjectAegisHarvesterVisualRig : MonoBehaviour
    {
        [Header("Team Color")]
        public Color teamColor = Color.white;
        public Renderer[] teamRenderers;

        [Header("Animation Hooks")]
        public Transform cutterDrumRoot;
        public Transform conveyorRoot;
        public Transform trackLeft;
        public Transform trackRight;
        public Transform oreCargoAnchor;
        public Transform machineGunRoot;
        public float cutterSpinDegreesPerSecond = 160f;
        public float conveyorScrollSpeed = 0.55f;
        public float trackScrollSpeed = 1.0f;
        public float machineGunIdleSweepDegrees = 6f;
        [Range(0f, 1f)] public float oreCargoFill01;

        MaterialPropertyBlock block;
        Quaternion machineGunInitialRotation = Quaternion.identity;
        Color appliedTeamColor;
        bool hasMachineGunInitialRotation;
        bool hasAppliedTeamColor;

        public void ApplyTeamColor(Color color)
        {
            teamColor = color;
            if (teamRenderers == null)
                return;

            if (hasAppliedTeamColor && appliedTeamColor == color)
                return;

            appliedTeamColor = color;
            hasAppliedTeamColor = true;
            block ??= new MaterialPropertyBlock();

            for (var i = 0; i < teamRenderers.Length; i++)
            {
                var renderer = teamRenderers[i];
                if (renderer == null)
                    continue;

                renderer.GetPropertyBlock(block);
                block.SetColor("_BaseColor", color);
                block.SetColor("_Color", color);
                renderer.SetPropertyBlock(block);
            }
        }

        public void SetCargoFill(float fill01)
        {
            oreCargoFill01 = Mathf.Clamp01(fill01);
            if (oreCargoAnchor != null)
                oreCargoAnchor.localScale = new Vector3(1f, Mathf.Lerp(0.15f, 1f, oreCargoFill01), 1f);
        }

        void Update()
        {
            var dt = Time.deltaTime;
            if (cutterDrumRoot != null)
                cutterDrumRoot.Rotate(Vector3.right, cutterSpinDegreesPerSecond * dt, Space.Self);

            if (machineGunRoot == null)
                return;

            if (!hasMachineGunInitialRotation)
            {
                machineGunInitialRotation = machineGunRoot.localRotation;
                hasMachineGunInitialRotation = true;
            }

            var sweep = Mathf.Sin(Time.time * 0.8f) * machineGunIdleSweepDegrees;
            machineGunRoot.localRotation = machineGunInitialRotation * Quaternion.Euler(0f, sweep, 0f);
        }
    }
}
