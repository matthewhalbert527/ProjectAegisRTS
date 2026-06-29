using UnityEngine;

namespace ProjectAegisRTS.UnityClient.InputControls.XR
{
    public interface ILeftHandInputSource
    {
        bool IsAvailable { get; }
        bool TryGetRay(out Ray ray);
        bool GetMenuToggleDown();
        bool GetPrimarySelectDown();
        bool GetPrimarySelectHeld();
        bool GetPrimarySelectUp();
        bool GetSecondaryModifierHeld();
        bool GetCancelDown();
        bool GetCategoryNextDown();
        bool GetCategoryPreviousDown();
        float GetCategoryAxis();
        float GetItemAxis();
        bool GetCycleCandidateNextDown();
        bool GetCycleCandidatePreviousDown();
        bool GetLassoModifierHeld();
    }
}
