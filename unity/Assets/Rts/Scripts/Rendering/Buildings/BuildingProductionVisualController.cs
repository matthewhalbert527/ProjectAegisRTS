using System;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Buildings
{
    public sealed class BuildingProductionVisualController : MonoBehaviour
    {
        BuildingPlaceholderPartFactory.PartSet parts;
        BuildingVisualProfile profile;

        public bool IsProducing { get; private set; }
        public float ProductionProgress01 { get; private set; }
        public float ProductionPulsePhase { get; private set; }

        public event Action OnProductionStarted;
        public event Action<float> OnProductionProgress;
        public event Action OnProductionCompleteVisual;

        bool wasProducing;

        public void Initialize(BuildingPlaceholderPartFactory.PartSet partSet, BuildingVisualProfile activeProfile)
        {
            parts = partSet;
            profile = activeProfile;
        }

        public void TickVisual(float deltaTime, bool isProducing, float progress01)
        {
            IsProducing = isProducing;
            ProductionProgress01 = Mathf.Clamp01(progress01);
            ProductionPulsePhase = Mathf.Repeat(ProductionPulsePhase + deltaTime * Mathf.Max(0.1f, profile == null ? 3f : profile.productionPulseSpeed), 1f);

            if (IsProducing && !wasProducing && OnProductionStarted != null)
                OnProductionStarted();
            if (IsProducing && OnProductionProgress != null)
                OnProductionProgress(ProductionProgress01);
            if (!IsProducing && wasProducing && OnProductionCompleteVisual != null)
                OnProductionCompleteVisual();
            wasProducing = IsProducing;

            if (parts == null || parts.ProductionIndicator == null)
                return;

            parts.ProductionIndicator.gameObject.SetActive(IsProducing);
            if (IsProducing)
            {
                var pulse = 0.85f + Mathf.Sin(ProductionPulsePhase * Mathf.PI * 2f) * 0.2f;
                parts.ProductionIndicator.localScale = Vector3.one * Mathf.Lerp(0.18f, 0.32f, Mathf.Clamp01(pulse));
            }
        }
    }
}
