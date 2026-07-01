using ProjectAegisRTS.UnityClient.InputControls.Desktop;
using ProjectAegisRTS.UnityClient.UI.Desktop;
using ProjectAegisRTS.UnityClient.UI.XR.LeftHand;
using ProjectAegisRTS.UnityClient.UI.XR.RightHand;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.UI.Common
{
    public sealed class PlayerFacingUiModeController : MonoBehaviour
    {
        public bool pcPlayerFacingMode = true;
        public bool allowSimulatedXrMenusInPcMode;
        public DesktopRtsHudRoot desktopHud;
        public DebugHudVisibilityController debugVisibility;

        void Awake()
        {
            ApplyModeDefaults();
        }

        void Start()
        {
            ApplyModeDefaults();
        }

        public void ApplyModeDefaults()
        {
            if (desktopHud == null)
                desktopHud = FindAnyObjectByType<DesktopRtsHudRoot>();
            if (debugVisibility == null)
                debugVisibility = FindAnyObjectByType<DebugHudVisibilityController>();

            if (desktopHud != null)
            {
                desktopHud.showDebugOverlay = false;
                desktopHud.gameObject.SetActive(true);
            }

            if (debugVisibility != null)
                debugVisibility.ApplyPlayerFacingDefaults();

            if (pcPlayerFacingMode && !allowSimulatedXrMenusInPcMode)
                SetXrFallbackUiActive(false);
        }

        public bool AreXrBuildMenusHiddenForPc()
        {
            if (!pcPlayerFacingMode || allowSimulatedXrMenusInPcMode)
                return true;

            return AreLeftHandMenusClosed() &&
                !AnySceneComponentActive<LeftHandRadialMenuView>() &&
                !AnySceneComponentActive<LeftHandPlacementPanel>() &&
                !AnySceneComponentActive<LeftHandSelectionPanel>() &&
                !AnySceneComponentActive<LeftHandStatusHud>() &&
                !AnySceneComponentActive<RightHandCommandHud>() &&
                !AnySceneComponentActive<RightHandStatusPanel>() &&
                !AnyEnabledSceneBehaviour<DesktopLeftHandInputSource>() &&
                !AnyEnabledSceneBehaviour<DesktopRightHandInputSource>();
        }

        void SetXrFallbackUiActive(bool active)
        {
            SetSceneBehavioursEnabled<DesktopLeftHandInputSource>(active);
            SetSceneBehavioursEnabled<DesktopRightHandInputSource>(active);
            CloseLeftHandBuildMenus();
            SetSceneComponentsActive<LeftHandRadialMenuView>(active);
            SetSceneComponentsActive<LeftHandPlacementPanel>(active);
            SetSceneComponentsActive<LeftHandSelectionPanel>(active);
            SetSceneComponentsActive<LeftHandStatusHud>(active);
            SetSceneComponentsActive<RightHandCommandHud>(active);
            SetSceneComponentsActive<RightHandStatusPanel>(active);
        }

        static void CloseLeftHandBuildMenus()
        {
            var menus = Resources.FindObjectsOfTypeAll<LeftHandBuildMenuController>();
            for (var i = 0; i < menus.Length; i++)
                if (IsSceneComponent(menus[i]))
                    menus[i].CloseMenu();
        }

        static bool AreLeftHandMenusClosed()
        {
            var menus = Resources.FindObjectsOfTypeAll<LeftHandBuildMenuController>();
            for (var i = 0; i < menus.Length; i++)
                if (IsSceneComponent(menus[i]) && menus[i].IsOpen)
                    return false;
            return true;
        }

        static void SetSceneBehavioursEnabled<T>(bool enabled) where T : Behaviour
        {
            var behaviours = Resources.FindObjectsOfTypeAll<T>();
            for (var i = 0; i < behaviours.Length; i++)
                if (IsSceneComponent(behaviours[i]))
                    behaviours[i].enabled = enabled;
        }

        static void SetSceneComponentsActive<T>(bool active) where T : Component
        {
            var components = Resources.FindObjectsOfTypeAll<T>();
            for (var i = 0; i < components.Length; i++)
            {
                var component = components[i];
                if (!IsSceneComponent(component))
                    continue;

                component.gameObject.SetActive(active);
                var behaviour = component as Behaviour;
                if (behaviour != null)
                    behaviour.enabled = active;
            }
        }

        static bool AnySceneComponentActive<T>() where T : Component
        {
            var components = Resources.FindObjectsOfTypeAll<T>();
            for (var i = 0; i < components.Length; i++)
                if (IsSceneComponent(components[i]) && components[i].gameObject.activeInHierarchy)
                    return true;
            return false;
        }

        static bool AnyEnabledSceneBehaviour<T>() where T : Behaviour
        {
            var behaviours = Resources.FindObjectsOfTypeAll<T>();
            for (var i = 0; i < behaviours.Length; i++)
                if (IsSceneComponent(behaviours[i]) && behaviours[i].isActiveAndEnabled)
                    return true;
            return false;
        }

        static bool IsSceneComponent(Component component)
        {
            return component != null && component.gameObject != null && component.gameObject.scene.IsValid();
        }
    }
}
