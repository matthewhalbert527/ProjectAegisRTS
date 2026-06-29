using UnityEngine;

namespace ProjectAegisRTS.UnityClient.InputControls.XR
{
    public sealed class SimulatedLeftHandRig : MonoBehaviour
    {
        public Camera sceneCamera;
        public Transform leftHandRoot;
        public Transform leftRayOrigin;
        public Transform uiAnchor;
        public LineRenderer rayLine;
        public float distanceFromCamera = 4.5f;
        GameObject controllerBody;

        void Awake()
        {
            EnsureRig();
        }

        void Start()
        {
            EnsureRig();
        }

        void Update()
        {
            EnsureRig();
            var cameraToUse = sceneCamera != null ? sceneCamera : Camera.main;
            if (cameraToUse != null)
            {
                transform.position = cameraToUse.transform.position + cameraToUse.transform.forward * distanceFromCamera - cameraToUse.transform.right * 1.5f - Vector3.up * 0.6f;
                transform.rotation = Quaternion.LookRotation(cameraToUse.transform.forward, Vector3.up);
            }

            if (rayLine != null && leftRayOrigin != null)
            {
                rayLine.SetPosition(0, leftRayOrigin.position);
                rayLine.SetPosition(1, leftRayOrigin.position + leftRayOrigin.forward * 18f);
            }
        }

        public void EnsureRig()
        {
            leftHandRoot = EnsureChild(transform, "Simulated Left Hand");
            leftRayOrigin = EnsureChild(leftHandRoot, "Left Hand Ray Origin");
            uiAnchor = EnsureChild(leftHandRoot, "Left Wrist UI Anchor");

            leftHandRoot.localPosition = Vector3.zero;
            leftHandRoot.localRotation = Quaternion.identity;
            leftRayOrigin.localPosition = new Vector3(0f, 0f, 0.1f);
            leftRayOrigin.localRotation = Quaternion.identity;
            uiAnchor.localPosition = new Vector3(0.16f, 0.08f, 0.22f);
            uiAnchor.localRotation = Quaternion.Euler(18f, -18f, 0f);

            if (controllerBody == null)
            {
                var existing = leftHandRoot.Find("Left Hand Controller Body");
                controllerBody = existing != null ? existing.gameObject : GameObject.CreatePrimitive(PrimitiveType.Cube);
                controllerBody.name = "Left Hand Controller Body";
                controllerBody.transform.SetParent(leftHandRoot, false);
                controllerBody.transform.localPosition = Vector3.zero;
                controllerBody.transform.localRotation = Quaternion.identity;
                controllerBody.transform.localScale = new Vector3(0.18f, 0.12f, 0.32f);
                var collider = controllerBody.GetComponent<Collider>();
                if (collider != null)
                    DestroyUnityObject(collider);
            }

            if (rayLine == null)
            {
                var rayObject = new GameObject("Left Hand Ray");
                rayObject.transform.SetParent(leftRayOrigin, false);
                rayLine = rayObject.AddComponent<LineRenderer>();
                rayLine.useWorldSpace = true;
                rayLine.positionCount = 2;
                rayLine.widthMultiplier = 0.025f;
                rayLine.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
                rayLine.startColor = new Color(0.3f, 0.85f, 1f, 0.85f);
                rayLine.endColor = new Color(0.3f, 0.85f, 1f, 0.05f);
            }
        }

        static void DestroyUnityObject(Object target)
        {
            if (Application.isPlaying)
                Destroy(target);
            else
                DestroyImmediate(target);
        }

        static Transform EnsureChild(Transform parent, string childName)
        {
            var existing = parent.Find(childName);
            if (existing != null)
                return existing;

            var obj = new GameObject(childName);
            obj.transform.SetParent(parent, false);
            return obj.transform;
        }
    }
}
