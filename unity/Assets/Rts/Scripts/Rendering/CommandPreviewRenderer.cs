using ProjectAegisRTS.Core;
using ProjectAegisRTS.UnityClient.CoreBridge;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering
{
    public sealed class CommandPreviewRenderer : MonoBehaviour
    {
        public BoardCoordinateMapper mapper;
        GameObject marker;
        Material moveMaterial;
        Material attackMaterial;
        Material invalidMaterial;

        public bool HasPreview { get; private set; }
        public Int2 PreviewCell { get; private set; }
        public string PreviewKind { get; private set; }

        public void Initialize(BoardCoordinateMapper coordinateMapper)
        {
            mapper = coordinateMapper;
            EnsureMaterials();
            EnsureMarker();
            ClearPreview();
        }

        public void ShowMoveTarget(Int2 cell)
        {
            Show(cell, "move", moveMaterial);
        }

        public void ShowAttackTarget(Int2 cell)
        {
            Show(cell, "attack", attackMaterial);
        }

        public void ShowInvalidTarget(Int2 cell)
        {
            Show(cell, "invalid", invalidMaterial);
        }

        public void ClearPreview()
        {
            HasPreview = false;
            PreviewKind = string.Empty;
            if (marker != null)
                marker.SetActive(false);
        }

        void Show(Int2 cell, string kind, Material material)
        {
            EnsureMaterials();
            EnsureMarker();
            if (mapper == null)
                return;

            HasPreview = true;
            PreviewCell = cell;
            PreviewKind = kind;
            marker.SetActive(true);
            marker.transform.position = mapper.CellToWorldCenter(cell) + Vector3.up * 0.09f;
            var renderer = marker.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = material;
        }

        void EnsureMarker()
        {
            if (marker != null)
                return;

            marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = "Right Hand Command Preview Marker";
            marker.transform.SetParent(transform, false);
            marker.transform.localScale = new Vector3(0.72f, 0.035f, 0.72f);
            var collider = marker.GetComponent<Collider>();
            if (collider != null)
                DestroyUnityObject(collider);
        }

        void EnsureMaterials()
        {
            if (moveMaterial != null)
                return;

            moveMaterial = CreateMaterial(new Color(0.2f, 0.85f, 0.45f, 0.88f));
            attackMaterial = CreateMaterial(new Color(1f, 0.35f, 0.18f, 0.88f));
            invalidMaterial = CreateMaterial(new Color(1f, 0.1f, 0.18f, 0.88f));
        }

        static Material CreateMaterial(Color color)
        {
            var material = new Material(Shader.Find("Sprites/Default"));
            material.color = color;
            return material;
        }

        static void DestroyUnityObject(Object target)
        {
            if (Application.isPlaying)
                Destroy(target);
            else
                DestroyImmediate(target);
        }
    }
}
