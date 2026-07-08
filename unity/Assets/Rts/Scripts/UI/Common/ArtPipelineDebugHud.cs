using ProjectAegisRTS.UnityClient.Art;
using ProjectAegisRTS.UnityClient.Rendering;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.UI.Common
{
    public sealed class ArtPipelineDebugHud : MonoBehaviour
    {
        public ActorRenderSystem actorRenderSystem;
        public ActorVisualDefinitionLibrary definitionLibrary;
        public ConceptArtReferenceLibrary conceptLibrary;
        public ActorVisualPrefabResolver prefabResolver;
        public ArtPipelineShowcaseController showcaseController;
        public bool visible = true;
        public KeyCode toggleKey = KeyCode.F11;

        string validationText = string.Empty;

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
            GUILayout.BeginArea(new Rect(824f, 260f, 430f, 430f), GUI.skin.box);
            GUILayout.Label("ART PIPELINE DEBUG HUD (F11)");

            if (definitionLibrary == null)
            {
                GUILayout.Label("ActorVisualDefinitionLibrary: missing");
                GUILayout.EndArea();
                return;
            }

            definitionLibrary.EnsureInitialized();
            if (conceptLibrary != null)
                conceptLibrary.EnsureInitialized();

            GUILayout.Label("Definitions: " + definitionLibrary.DefinitionCount);
            GUILayout.Label("Concept refs: " + (conceptLibrary == null ? 0 : conceptLibrary.ReferenceCount));
            if (actorRenderSystem != null)
            {
                GUILayout.Label("Renderer definitions: " + actorRenderSystem.ActorVisualDefinitionCount);
                GUILayout.Label("Resolved prefabs: " + actorRenderSystem.ResolvedPrefabCount + "  fallback primitives: " + actorRenderSystem.FallbackPrimitiveCount);
                GUILayout.Label("Missing definitions: " + actorRenderSystem.MissingDefinitionCount);
            }

            ActorVisualDefinition definition = null;
            if (showcaseController != null)
                definition = showcaseController.SelectedDefinition;
            if (definition == null)
            {
                var all = definitionLibrary.GetAllDefinitions();
                if (all.Count > 0)
                    definition = all[0];
            }

            if (definition != null)
            {
                GUILayout.Space(6f);
                GUILayout.Label("Selected: " + definition.actorTypeId);
                GUILayout.Label("Name: " + definition.safeDisplayName);
                GUILayout.Label("Category: " + definition.category + "  Status: " + definition.productionStatus);
                GUILayout.Label("Concept: " + (definition.conceptReference == null ? "missing" : "assigned"));
                GUILayout.Label("Icon: " + (definition.icon == null ? "missing" : "assigned"));
                GUILayout.Label("Blockout: " + (definition.generatedBlockoutPrefab == null ? "missing" : "assigned"));
                GUILayout.Label("Production prefab: " + (definition.productionPrefab == null ? "missing" : "assigned"));
                GUILayout.Label("Best mode: " + definition.preferredPrefabMode + "  IP review: " + definition.ipReviewRequired);
                GUILayout.Label("Sockets: " + (definition.requiredSockets == null ? 0 : definition.requiredSockets.Count));
                GUILayout.Label("Summary: " + definition.GetValidationSummary());
            }

            GUILayout.Space(6f);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Prev"))
                WithShowcase(delegate(ArtPipelineShowcaseController showcase) { showcase.CycleSelected(-1); });
            if (GUILayout.Button("Next"))
                WithShowcase(delegate(ArtPipelineShowcaseController showcase) { showcase.CycleSelected(1); });
            if (GUILayout.Button("Spawn"))
                WithShowcase(delegate(ArtPipelineShowcaseController showcase) { showcase.SpawnSelectedPreview(new Vector3(0f, 0f, 25f)); });
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Concepts"))
                WithShowcase(delegate(ArtPipelineShowcaseController showcase) { showcase.ToggleConceptCards(); });
            if (GUILayout.Button("Sockets"))
                WithShowcase(delegate(ArtPipelineShowcaseController showcase) { showcase.ToggleSocketLabels(); });
            if (GUILayout.Button("Refresh"))
                WithShowcase(delegate(ArtPipelineShowcaseController showcase) { showcase.EnsureShowcase(); });
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Validate All"))
            {
                var errors = definitionLibrary.ValidateAllDefinitions();
                validationText = errors.Count == 0 ? "Validation passed." : "Validation issues: " + string.Join(" | ", errors.ToArray());
            }

            if (!string.IsNullOrEmpty(validationText))
                GUILayout.Label(validationText);

            GUILayout.EndArea();
        }

        void EnsureReferences()
        {
            if (actorRenderSystem == null)
                actorRenderSystem = Object.FindFirstObjectByType<ActorRenderSystem>();
            if (definitionLibrary == null)
                definitionLibrary = Object.FindFirstObjectByType<ActorVisualDefinitionLibrary>();
            if (conceptLibrary == null)
                conceptLibrary = Object.FindFirstObjectByType<ConceptArtReferenceLibrary>();
            if (prefabResolver == null)
                prefabResolver = Object.FindFirstObjectByType<ActorVisualPrefabResolver>();
            if (showcaseController == null)
                showcaseController = Object.FindFirstObjectByType<ArtPipelineShowcaseController>();
        }

        void WithShowcase(System.Action<ArtPipelineShowcaseController> action)
        {
            EnsureReferences();
            if (showcaseController != null && action != null)
                action(showcaseController);
        }
    }
}
