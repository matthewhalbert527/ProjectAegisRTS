using System;
using System.Reflection;
using ProjectAegisRTS.UnityClient.Board;
using ProjectAegisRTS.UnityClient.CoreBridge;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.UI.Common
{
    public sealed class DebugHudVisibilityController : MonoBehaviour
    {
        static readonly string[] HiddenByDefaultTypeNames =
        {
            "RtsDebugHud",
            "EconomyDebugHud",
            "MovementDebugHud",
            "BuildingAnimationDebugHud",
            "ArtPipelineDebugHud",
            "CombatDebugHud",
            "AiDebugHud",
            "FogDebugHud",
            "MapValidationDebugHud",
            "FeedbackDebugHud",
            "RenderStatsHud",
            "IntegratedSystemsStatusHud",
            "ConceptArtCardView",
            "RightHandCommandHud",
            "RightHandStatusPanel",
            "LeftHandStatusHud",
            "LeftHandSelectionPanel",
            "LeftHandRadialMenuView",
            "RtsStatusLog"
        };

        static readonly string[] DeactivateWhenHiddenTypeNames =
        {
            "RightHandCommandHud",
            "RightHandStatusPanel",
            "LeftHandStatusHud",
            "LeftHandSelectionPanel",
            "LeftHandRadialMenuView",
            "RtsStatusLog"
        };

        static readonly string[] PlacementPanelTypeNames =
        {
            "BoardPlacementHud",
            "PlacementModePanel",
            "LeftHandPlacementPanel"
        };

        static readonly string[] PlayerHudTypeNames =
        {
            "MatchObjectiveHud",
            "PlayerObjectiveHud",
            "PlayerPromptHud",
            "MatchResultHud"
        };

        public bool showDebugPanelsByDefault;
        public bool hideDebugPanelsOnStart = true;
        public bool keepPlacementPanelsSynced = true;

        void Awake()
        {
            ApplyPlayerFacingDefaults();
        }

        void Start()
        {
            ApplyPlayerFacingDefaults();
        }

        void Update()
        {
            if (keepPlacementPanelsSynced)
                SetPlacementPanelsVisible(IsPlacementActive() || showDebugPanelsByDefault);
        }

        public void ApplyPlayerFacingDefaults()
        {
            if (hideDebugPanelsOnStart)
                SetKnownDebugPanelsVisible(showDebugPanelsByDefault);

            var objectiveHud = FindAnyObjectByType<MatchObjectiveHud>();
            if (objectiveHud != null)
            {
                objectiveHud.visible = true;
                objectiveHud.showDebugActions = showDebugPanelsByDefault;
            }

            SetPlayerHudsVisible(true);
            var controlsOverlay = FindAnyObjectByType<PlayerControlsOverlay>();
            if (controlsOverlay != null)
                controlsOverlay.visible = false;

            if (keepPlacementPanelsSynced)
                SetPlacementPanelsVisible(IsPlacementActive() || showDebugPanelsByDefault);
        }

        public void SetKnownDebugPanelsVisible(bool visible)
        {
            var behaviours = Resources.FindObjectsOfTypeAll<MonoBehaviour>();
            for (var i = 0; i < behaviours.Length; i++)
            {
                var behaviour = behaviours[i];
                if (!IsSceneBehaviour(behaviour))
                    continue;

                var type = behaviour.GetType();
                if (!Contains(HiddenByDefaultTypeNames, type.Name))
                    continue;

                SetBoolFieldOrProperty(behaviour, type, "visible", visible);
                if (Contains(DeactivateWhenHiddenTypeNames, type.Name))
                    behaviour.gameObject.SetActive(visible);
            }
        }

        void SetPlacementPanelsVisible(bool visible)
        {
            var behaviours = Resources.FindObjectsOfTypeAll<MonoBehaviour>();
            for (var i = 0; i < behaviours.Length; i++)
            {
                var behaviour = behaviours[i];
                if (!IsSceneBehaviour(behaviour))
                    continue;

                if (Contains(PlacementPanelTypeNames, behaviour.GetType().Name))
                    behaviour.gameObject.SetActive(visible);
            }
        }

        void SetPlayerHudsVisible(bool visible)
        {
            var behaviours = Resources.FindObjectsOfTypeAll<MonoBehaviour>();
            for (var i = 0; i < behaviours.Length; i++)
            {
                var behaviour = behaviours[i];
                if (!IsSceneBehaviour(behaviour))
                    continue;

                if (Contains(PlayerHudTypeNames, behaviour.GetType().Name))
                    SetBoolFieldOrProperty(behaviour, behaviour.GetType(), "visible", visible);
            }
        }

        public bool AreDebugPanelsHiddenByDefault()
        {
            var behaviours = Resources.FindObjectsOfTypeAll<MonoBehaviour>();
            for (var i = 0; i < behaviours.Length; i++)
            {
                var behaviour = behaviours[i];
                if (!IsSceneBehaviour(behaviour))
                    continue;

                var typeName = behaviour.GetType().Name;
                if (!Contains(HiddenByDefaultTypeNames, typeName))
                    continue;

                bool visible;
                if (TryGetBoolFieldOrProperty(behaviour, behaviour.GetType(), "visible", out visible) && visible)
                    return false;
                if (Contains(DeactivateWhenHiddenTypeNames, typeName) && behaviour.gameObject.activeInHierarchy)
                    return false;
            }

            return true;
        }

        public bool IsPlacementUiHiddenByDefault()
        {
            if (IsPlacementActive())
                return false;

            var behaviours = Resources.FindObjectsOfTypeAll<MonoBehaviour>();
            for (var i = 0; i < behaviours.Length; i++)
            {
                var behaviour = behaviours[i];
                if (!IsSceneBehaviour(behaviour))
                    continue;

                if (Contains(PlacementPanelTypeNames, behaviour.GetType().Name) && behaviour.gameObject.activeInHierarchy)
                    return false;
            }

            return true;
        }

        public bool IsPlayerHudVisible()
        {
            return IsHudVisible<MatchObjectiveHud>() &&
                IsHudVisible<PlayerObjectiveHud>() &&
                IsHudVisible<PlayerPromptHud>() &&
                IsHudVisible<MatchResultHud>();
        }

        static bool IsPlacementActive()
        {
            var driver = FindAnyObjectByType<RtsSimulationDriver>();
            if (driver != null && driver.HasPlacementMode)
                return true;

            var boardPlacement = FindAnyObjectByType<BoardPlacementController>();
            return boardPlacement != null && boardPlacement.IsPlacementModeActive;
        }

        static bool IsSceneBehaviour(MonoBehaviour behaviour)
        {
            return behaviour != null && behaviour.gameObject != null && behaviour.gameObject.scene.IsValid();
        }

        static bool Contains(string[] values, string candidate)
        {
            for (var i = 0; i < values.Length; i++)
                if (string.Equals(values[i], candidate, StringComparison.Ordinal))
                    return true;
            return false;
        }

        static void SetBoolFieldOrProperty(object target, Type type, string name, bool value)
        {
            var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null && field.FieldType == typeof(bool))
            {
                field.SetValue(target, value);
                return;
            }

            var property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null && property.PropertyType == typeof(bool) && property.CanWrite)
                property.SetValue(target, value, null);
        }

        static bool TryGetBoolFieldOrProperty(object target, Type type, string name, out bool value)
        {
            var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null && field.FieldType == typeof(bool))
            {
                value = (bool)field.GetValue(target);
                return true;
            }

            var property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null && property.PropertyType == typeof(bool) && property.CanRead)
            {
                value = (bool)property.GetValue(target, null);
                return true;
            }

            value = false;
            return false;
        }

        static bool IsHudVisible<T>() where T : MonoBehaviour
        {
            var hud = FindAnyObjectByType<T>();
            if (hud == null || !hud.isActiveAndEnabled)
                return false;

            bool visible;
            return TryGetBoolFieldOrProperty(hud, hud.GetType(), "visible", out visible) && visible;
        }
    }
}
