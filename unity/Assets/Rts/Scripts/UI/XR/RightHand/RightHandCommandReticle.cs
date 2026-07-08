using ProjectAegisRTS.Core;
using ProjectAegisRTS.UnityClient.Rendering;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.UI.XR.RightHand
{
    public sealed class RightHandCommandReticle : MonoBehaviour
    {
        public CommandPreviewRenderer previewRenderer;
        public RightHandCommandMode currentMode;
        public Int2 hoveredCell;
        public bool hasHoveredCell;

        public void ShowHover(Int2 cell, RightHandCommandMode mode)
        {
            hoveredCell = cell;
            hasHoveredCell = true;
            currentMode = mode;

            if (previewRenderer == null)
                return;

            if (mode == RightHandCommandMode.Attack || mode == RightHandCommandMode.ForceAttack)
                previewRenderer.ShowAttackTarget(cell);
            else if (mode == RightHandCommandMode.Disabled)
                previewRenderer.ShowInvalidTarget(cell);
            else
                previewRenderer.ShowMoveTarget(cell);
        }

        public void Clear()
        {
            hasHoveredCell = false;
            if (previewRenderer != null)
                previewRenderer.ClearPreview();
        }
    }
}
