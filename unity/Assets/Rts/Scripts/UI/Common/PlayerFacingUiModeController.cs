using ProjectAegisRTS.UnityClient.InputControls.Desktop;
using ProjectAegisRTS.UnityClient.UI.Desktop;
using ProjectAegisRTS.UnityClient.UI.XR.LeftHand;
using ProjectAegisRTS.UnityClient.UI.XR.RightHand;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.UI.Common
{
    public enum PlayerFacingUiMode
    {
        PCDesktop,
        QuestXR,
        DebugHybrid
    }

    public sealed class PlayerFacingUiModeController : MonoBehaviour
    {
        public PlayerFacingUiMode uiMode = PlayerFacingUiMode.PCDesktop;
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
            var mode = GetEffectiveMode();
            pcPlayerFacingMode = mode == PlayerFacingUiMode.PCDesktop;

            if (desktopHud == null)
                desktopHud = FindAnyObjectByType<DesktopRtsHudRoot>();
            if (debugVisibility == null)
                debugVisibility = FindAnyObjectByType<DebugHudVisibilityController>();

            if (desktopHud != null)
            {
                desktopHud.showDebugOverlay = false;
                desktopHud.gameObject.SetActive(mode == PlayerFacingUiMode.PCDesktop || mode == PlayerFacingUiMode.DebugHybrid);
                if (desktopHud.gameObject.activeInHierarchy)
                    desktopHud.Initialize();
            }

            if (debugVisibility != null)
                debugVisibility.ApplyPlayerFacingDefaults();

            if (mode == PlayerFacingUiMode.PCDesktop)
                SetXrFallbackUiActive(allowSimulatedXrMenusInPcMode);
            else
                SetXrFallbackUiActive(true);
        }

        public PlayerFacingUiMode GetEffectiveMode()
        {
            if (uiMode == PlayerFacingUiMode.PCDesktop && !pcPlayerFacingMode)
                return PlayerFacingUiMode.QuestXR;
            return uiMode;
        }

        public bool IsPcDesktopMode()
        {
            return GetEffectiveMode() == PlayerFacingUiMode.PCDesktop;
        }

        public bool IsQuestXrMode()
        {
            return GetEffectiveMode() == PlayerFacingUiMode.QuestXR;
        }

        public bool IsDebugHybridMode()
        {
            return GetEffectiveMode() == PlayerFacingUiMode.DebugHybrid;
        }

        public bool IsPcSidebarVisibleForDesktop()
        {
            var mode = GetEffectiveMode();
            if (mode != PlayerFacingUiMode.PCDesktop && mode != PlayerFacingUiMode.DebugHybrid)
                return false;

            if (desktopHud == null)
                desktopHud = FindAnyObjectByType<DesktopRtsHudRoot>();
            if (desktopHud == null || !desktopHud.gameObject.activeInHierarchy)
                return false;

            if (desktopHud.cncSidebarLayout == null)
                desktopHud.Initialize();
            var layout = desktopHud.cncSidebarLayout;
            return layout != null &&
                layout.rightSidebarRoot != null &&
                layout.rightSidebarRoot.gameObject.activeInHierarchy &&
                layout.AreProductionPanelsInRightSidebar() &&
                layout.IsRightSidebarDockedToScreenEdge() &&
                layout.IsMinimapAboveProductionGrid();
        }

        public bool IsPcSidebarHiddenForQuest()
        {
            if (GetEffectiveMode() != PlayerFacingUiMode.QuestXR)
                return true;

            if (desktopHud == null)
                desktopHud = FindAnyObjectByType<DesktopRtsHudRoot>();
            return desktopHud == null || !desktopHud.gameObject.activeInHierarchy;
        }

        public bool AreQuestLeftHandControlsAvailable()
        {
            return HasSceneComponent<LeftHandBuildMenuController>() &&
                HasSceneComponent<LeftHandRadialMenuView>() &&
                HasSceneComponent<LeftHandPlacementPanel>() &&
                HasSceneComponent<LeftHandSelectionPanel>() &&
                HasSceneComponent<LeftHandStatusHud>() &&
                HasSceneComponent<LeftHandCommandRouter>();
        }

        public bool AreQuestRightHandControlsAvailable()
        {
            return HasSceneComponent<RightHandCommandRouter>() &&
                HasSceneComponent<RightHandCommandHud>() &&
                HasSceneComponent<RightHandStatusPanel>();
        }

        public bool AreQuestControlsActiveForQuest()
        {
            var mode = GetEffectiveMode();
            if (mode != PlayerFacingUiMode.QuestXR && mode != PlayerFacingUiMode.DebugHybrid)
                return true;

            return AnySceneComponentActive<LeftHandRadialMenuView>() &&
                AnySceneComponentActive<LeftHandPlacementPanel>() &&
                AnySceneComponentActive<LeftHandSelectionPanel>() &&
                AnySceneComponentActive<LeftHandStatusHud>() &&
                AnySceneComponentActive<RightHandCommandHud>() &&
                AnySceneComponentActive<RightHandStatusPanel>() &&
                AnyEnabledSceneBehaviour<DesktopLeftHandInputSource>() &&
                AnyEnabledSceneBehaviour<DesktopRightHandInputSource>();
        }

        public bool WindowsPlayerDefaultsToPcDesktop()
        {
            return uiMode == PlayerFacingUiMode.PCDesktop && pcPlayerFacingMode;
        }

        public bool AreXrBuildMenusHiddenForPc()
        {
            if (GetEffectiveMode() != PlayerFacingUiMode.PCDesktop || allowSimulatedXrMenusInPcMode)
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

        [ContextMenu("Apply PC Desktop Mode")]
        public void ApplyPcDesktopMode()
        {
            uiMode = PlayerFacingUiMode.PCDesktop;
            pcPlayerFacingMode = true;
            ApplyModeDefaults();
        }

        [ContextMenu("Apply Quest XR Mode")]
        public void ApplyQuestXrMode()
        {
            uiMode = PlayerFacingUiMode.QuestXR;
            pcPlayerFacingMode = false;
            ApplyModeDefaults();
        }

        [ContextMenu("Apply Debug Hybrid Mode")]
        public void ApplyDebugHybridMode()
        {
            uiMode = PlayerFacingUiMode.DebugHybrid;
            pcPlayerFacingMode = false;
            ApplyModeDefaults();
        }

        void SetXrFallbackUiActive(bool active)
        {
            SetSceneBehavioursEnabled<DesktopLeftHandInputSource>(active);
            SetSceneBehavioursEnabled<DesktopRightHandInputSource>(active);
            if (!active)
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

        static bool HasSceneComponent<T>() where T : Component
        {
            var components = Resources.FindObjectsOfTypeAll<T>();
            for (var i = 0; i < components.Length; i++)
                if (IsSceneComponent(components[i]))
                    return true;
            return false;
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
