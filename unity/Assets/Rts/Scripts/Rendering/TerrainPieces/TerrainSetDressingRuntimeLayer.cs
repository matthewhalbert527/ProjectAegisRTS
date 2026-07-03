using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.TerrainPieces
{
    public sealed class TerrainSetDressingRuntimeLayer : MonoBehaviour
    {
        public TerrainPieceLibrary pieceLibrary;
        public Stage32TerrainPieceMaterialLibrary materialLibrary;
        public TerrainSetDressingLibrary setDressingLibrary;
        public TerrainSetDressingProfile activeProfile;
        public TerrainPieceRuntimeResolver resolver;
        public TerrainSetDressingRenderer renderer;
        public bool initializeOnStart = true;
        public bool logStartup = true;

        void Start()
        {
            if (initializeOnStart)
                EnsureInitialized();
        }

        public void EnsureInitialized()
        {
            if (resolver == null)
                resolver = GetComponent<TerrainPieceRuntimeResolver>();
            if (resolver == null)
                resolver = gameObject.AddComponent<TerrainPieceRuntimeResolver>();

            if (renderer == null)
                renderer = GetComponent<TerrainSetDressingRenderer>();
            if (renderer == null)
                renderer = gameObject.AddComponent<TerrainSetDressingRenderer>();

            if (activeProfile == null && setDressingLibrary != null)
                activeProfile = setDressingLibrary.GetDefaultProfile();

            resolver.pieceLibrary = pieceLibrary;
            resolver.materialLibrary = materialLibrary;
            renderer.resolver = resolver;
            renderer.profile = activeProfile;
            renderer.RenderProfile();

            if (logStartup)
            {
                Debug.Log("[Stage32 Terrain] profile=" +
                    (activeProfile == null ? "none" : activeProfile.profileId) +
                    " renderedPieces=" + renderer.RenderedPieceCount +
                    " visualOnly=" + renderer.LastRenderWasVisualOnly);
            }
        }
    }
}
