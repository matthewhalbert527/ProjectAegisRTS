using ProjectAegisRTS.UnityClient.Art;
using ProjectAegisRTS.UnityClient.Art.Production;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.UI.Common
{
    public sealed class ProductionVisualDebugHud : MonoBehaviour
    {
        public ActorVisualDefinitionLibrary definitionLibrary;
        public ProductionVisualStandardLibrary standardLibrary;
        public Stage20MvpVisualShowcaseController showcaseController;
        public bool visible;
        public KeyCode toggleKey = KeyCode.F10;

        void Update()
        {
            if (Input.GetKeyDown(toggleKey))
                visible = !visible;
            EnsureReferences();
        }

        void OnGUI()
        {
            if (!visible)
                return;

            EnsureReferences();
            GUILayout.BeginArea(new Rect(24f, 84f, 420f, 320f), GUI.skin.box);
            GUILayout.Label("STAGE 20 PRODUCTION VISUALS (F10)");
            GUILayout.Label("MVP proxies: " + (showcaseController == null ? 0 : showcaseController.MvpProxyCount));
            GUILayout.Label("Missing proxies: " + (showcaseController == null ? 0 : showcaseController.MissingProxyCount));
            GUILayout.Label("Sockets ok: " + (showcaseController == null ? 0 : showcaseController.SocketValidatedCount));
            GUILayout.Label("Coverage ok: " + (showcaseController == null ? 0 : showcaseController.ViewCoverageValidatedCount));
            GUILayout.Label("Standards: " + (standardLibrary == null ? 0 : standardLibrary.StandardCount));

            var selected = showcaseController == null ? null : showcaseController.SelectedDefinition;
            if (selected != null)
            {
                GUILayout.Space(6f);
                GUILayout.Label("Selected: " + selected.actorTypeId);
                GUILayout.Label("Mode: " + selected.preferredPrefabMode + "  Status: " + selected.productionStatus);
                GUILayout.Label("Footprint: " + selected.footprintWidth + "x" + selected.footprintHeight);
                GUILayout.Label("Production prefab: " + (selected.productionPrefab == null ? "missing" : selected.productionPrefab.name));
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Prev"))
                WithShowcase(delegate(Stage20MvpVisualShowcaseController showcase) { showcase.CycleSelected(-1); });
            if (GUILayout.Button("Next"))
                WithShowcase(delegate(Stage20MvpVisualShowcaseController showcase) { showcase.CycleSelected(1); });
            if (GUILayout.Button("Labels"))
                WithShowcase(delegate(Stage20MvpVisualShowcaseController showcase) { showcase.ToggleLabels(); });
            if (GUILayout.Button("Sockets"))
                WithShowcase(delegate(Stage20MvpVisualShowcaseController showcase) { showcase.ToggleSocketLabels(); });
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Refresh Showcase"))
                WithShowcase(delegate(Stage20MvpVisualShowcaseController showcase) { showcase.EnsureShowcase(); });

            GUILayout.EndArea();
        }

        void EnsureReferences()
        {
            if (definitionLibrary == null)
                definitionLibrary = Object.FindFirstObjectByType<ActorVisualDefinitionLibrary>();
            if (standardLibrary == null)
                standardLibrary = Object.FindFirstObjectByType<ProductionVisualStandardLibrary>();
            if (showcaseController == null)
                showcaseController = Object.FindFirstObjectByType<Stage20MvpVisualShowcaseController>();
        }

        void WithShowcase(System.Action<Stage20MvpVisualShowcaseController> action)
        {
            EnsureReferences();
            if (showcaseController != null && action != null)
                action(showcaseController);
        }
    }
}
