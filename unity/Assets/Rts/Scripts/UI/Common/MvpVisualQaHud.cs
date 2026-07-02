using ProjectAegisRTS.UnityClient.Art.Production;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.UI.Common
{
    public sealed class MvpVisualQaHud : MonoBehaviour
    {
        public Stage21MvpVisualQaShowcaseController showcaseController;
        public bool visible = true;
        public KeyCode toggleKey = KeyCode.F9;

        void Update()
        {
            if (Input.GetKeyDown(toggleKey))
                visible = !visible;
            if (Input.GetKeyDown(KeyCode.LeftBracket))
                WithShowcase(delegate(Stage21MvpVisualQaShowcaseController showcase) { showcase.CycleSelected(-1); });
            if (Input.GetKeyDown(KeyCode.RightBracket))
                WithShowcase(delegate(Stage21MvpVisualQaShowcaseController showcase) { showcase.CycleSelected(1); });
        }

        void OnGUI()
        {
            if (!visible)
                return;

            EnsureReferences();
            GUILayout.BeginArea(new Rect(24f, 24f, 470f, 340f), GUI.skin.box);
            GUILayout.Label("STAGE 21 MVP VISUAL QA (F9)");
            GUILayout.Label("Displayed MVP actors: " + (showcaseController == null ? 0 : showcaseController.DisplayedActorCount));
            GUILayout.Label("QA pass/warn/fail: " + (showcaseController == null ? "0/0/0" : showcaseController.PassCount + "/" + showcaseController.WarningCount + "/" + showcaseController.FailCount));

            var selected = showcaseController == null ? null : showcaseController.SelectedReport;
            if (selected != null)
            {
                GUILayout.Space(6f);
                GUILayout.Label("Selected: " + selected.actorTypeId);
                GUILayout.Label("Status: " + selected.overallStatus + "  Import: " + selected.artistImportStatus);
                GUILayout.Label("Bounds: " + selected.localBoundsSize.ToString("F2"));
                GUILayout.Label("Sockets: " + selected.socketCount + " present, " + selected.requiredSocketCount + " definition-required");
                GUILayout.Label("Meshes/materials: " + selected.meshObjectCount + "/" + selected.materialCount);
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Prev"))
                WithShowcase(delegate(Stage21MvpVisualQaShowcaseController showcase) { showcase.CycleSelected(-1); });
            if (GUILayout.Button("Next"))
                WithShowcase(delegate(Stage21MvpVisualQaShowcaseController showcase) { showcase.CycleSelected(1); });
            if (GUILayout.Button("Labels"))
                WithShowcase(delegate(Stage21MvpVisualQaShowcaseController showcase) { showcase.ToggleLabels(); });
            if (GUILayout.Button("Sockets"))
                WithShowcase(delegate(Stage21MvpVisualQaShowcaseController showcase) { showcase.ToggleSocketMarkers(); });
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Refresh QA"))
                WithShowcase(delegate(Stage21MvpVisualQaShowcaseController showcase) { showcase.EnsureShowcase(); });

            GUILayout.EndArea();
        }

        void EnsureReferences()
        {
            if (showcaseController == null)
                showcaseController = Object.FindFirstObjectByType<Stage21MvpVisualQaShowcaseController>();
        }

        void WithShowcase(System.Action<Stage21MvpVisualQaShowcaseController> action)
        {
            EnsureReferences();
            if (showcaseController != null && action != null)
                action(showcaseController);
        }
    }
}
