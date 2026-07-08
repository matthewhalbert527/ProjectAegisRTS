using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Art.Production
{
    public sealed class ProjectAegisLightTankVisualRig : MonoBehaviour
    {
        public Transform turretRoot;
        public Transform barrelRoot;
        public Transform trackLeft;
        public Transform trackRight;
        public Transform muzzlePrimary;
        public Renderer[] teamColorRenderers;
        public float trackScrollSpeed = 2.4f;
        public float recoilDistanceMeters = 0.08f;
        [Range(0f, 1f)] public float teamTintStrength = 0.45f;

        Vector3 barrelRestLocalPosition;
        float trackPhase;
        MaterialPropertyBlock teamTintBlock;

        void Awake()
        {
            if (barrelRoot != null)
                barrelRestLocalPosition = barrelRoot.localPosition;
        }

        public void ApplyTeamColor(Color color)
        {
            if (teamColorRenderers == null)
                return;

            ProjectAegisTeamColorMaterialUtility.ApplyTeamTint(teamColorRenderers, color, ref teamTintBlock, teamTintStrength);
        }

        public void SetTurretYaw(float yawDegrees)
        {
            if (turretRoot != null)
                turretRoot.localRotation = Quaternion.Euler(0f, yawDegrees, 0f);
        }

        public void SetBarrelPitch(float pitchDegrees)
        {
            if (barrelRoot != null)
                barrelRoot.localRotation = Quaternion.Euler(pitchDegrees, 0f, 0f);
        }

        public void PlayRecoil(float normalized)
        {
            if (barrelRoot == null)
                return;

            normalized = Mathf.Clamp01(normalized);
            barrelRoot.localPosition = barrelRestLocalPosition + new Vector3(0f, 0f, -recoilDistanceMeters * normalized);
        }

        public void SetTrackMotion(float signedSpeed)
        {
            trackPhase += signedSpeed * trackScrollSpeed * Time.deltaTime;
            ApplyTrackPhase(trackLeft, trackPhase);
            ApplyTrackPhase(trackRight, trackPhase);
        }

        static void ApplyTrackPhase(Transform root, float phase)
        {
            if (root == null)
                return;

            var renderers = root.GetComponentsInChildren<Renderer>(true);
            for (var i = 0; i < renderers.Length; i++)
            {
                var block = new MaterialPropertyBlock();
                renderers[i].GetPropertyBlock(block);
                block.SetFloat("_TrackPhase", phase);
                renderers[i].SetPropertyBlock(block);
            }
        }
    }
}
