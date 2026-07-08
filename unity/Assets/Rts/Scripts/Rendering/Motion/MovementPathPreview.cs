using ProjectAegisRTS.Core;
using ProjectAegisRTS.UnityClient.CoreBridge;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Motion
{
    public sealed class MovementPathPreview : MonoBehaviour
    {
        public BoardCoordinateMapper mapper;
        public Color lineColor = new Color(0.35f, 0.9f, 1f, 0.85f);
        LineRenderer line;
        GameObject startMarker;
        GameObject targetMarker;
        Material material;

        public bool HasPreview { get; private set; }

        public void Initialize(BoardCoordinateMapper coordinateMapper)
        {
            mapper = coordinateMapper;
            EnsureVisuals();
            ClearPreview();
        }

        public void SetPreview(Int2 startCell, Int2 targetCell)
        {
            if (mapper == null)
                mapper = Object.FindFirstObjectByType<BoardCoordinateMapper>();
            if (mapper == null)
                return;

            SetPreviewWorld(mapper.CellToWorldCenter(startCell), mapper.CellToWorldCenter(targetCell));
        }

        public void SetPreviewWorld(Vector3 startWorld, Vector3 targetWorld)
        {
            EnsureVisuals();
            HasPreview = true;
            line.gameObject.SetActive(true);
            startMarker.SetActive(true);
            targetMarker.SetActive(true);

            var start = startWorld + Vector3.up * 0.12f;
            var target = targetWorld + Vector3.up * 0.12f;
            line.SetPosition(0, start);
            line.SetPosition(1, target);
            startMarker.transform.position = start;
            targetMarker.transform.position = target;
        }

        public void ClearPreview()
        {
            HasPreview = false;
            if (line != null)
                line.gameObject.SetActive(false);
            if (startMarker != null)
                startMarker.SetActive(false);
            if (targetMarker != null)
                targetMarker.SetActive(false);
        }

        void EnsureVisuals()
        {
            if (material == null)
            {
                material = new Material(Shader.Find("Sprites/Default"));
                material.color = lineColor;
            }

            if (line == null)
            {
                var lineObject = new GameObject("Movement Path Preview Line");
                lineObject.transform.SetParent(transform, false);
                line = lineObject.AddComponent<LineRenderer>();
                line.useWorldSpace = true;
                line.positionCount = 2;
                line.widthMultiplier = 0.045f;
                line.sharedMaterial = material;
            }

            if (startMarker == null)
                startMarker = CreateMarker("Movement Path Start Marker", new Color(0.35f, 0.9f, 1f, 0.8f));
            if (targetMarker == null)
                targetMarker = CreateMarker("Movement Path Target Marker", new Color(0.1f, 1f, 0.45f, 0.9f));
        }

        GameObject CreateMarker(string markerName, Color color)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = markerName;
            marker.transform.SetParent(transform, false);
            marker.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
            var renderer = marker.GetComponent<Renderer>();
            if (renderer != null)
            {
                var markerMaterial = new Material(Shader.Find("Sprites/Default"));
                markerMaterial.color = color;
                renderer.sharedMaterial = markerMaterial;
            }

            var collider = marker.GetComponent<Collider>();
            if (collider != null)
                DestroyUnityObject(collider);
            return marker;
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
