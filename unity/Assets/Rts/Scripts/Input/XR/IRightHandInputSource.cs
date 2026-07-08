using UnityEngine;

namespace ProjectAegisRTS.UnityClient.InputControls.XR
{
    public interface IRightHandInputSource
    {
        bool IsAvailable { get; }
        bool TryGetRay(out Ray ray);
        bool GetHudToggleDown();
        bool GetPrimaryCommandDown();
        bool GetPrimaryCommandHeld();
        bool GetPrimaryCommandUp();
        bool GetCancelDown();
        bool GetMoveModeDown();
        bool GetAttackModeDown();
        bool GetForceAttackModeDown();
        bool GetBoardManipulationHeld();
        bool GetBoardManipulationToggleDown();
        float GetRotateAxis();
        float GetZoomAxis();
        bool GetAlternateModifierHeld();
    }
}
