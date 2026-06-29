using UnityEngine;

namespace ProjectAegisRTS.UnityClient.InputControls.XR
{
    public sealed class SimulatedRightHandRig : MonoBehaviour
    {
        public Camera sceneCamera;
        public Transform rightHandRoot;
        public Transform rightRayOrigin;
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
                transform.position = cameraToUse.transform.position + cameraToUse.transform.forward * distanceFromCamera + cameraToUse.transform.right * 1.5f - Vector3.up * 0.6f;
                transform.rotation = Quaternion.LookRotation(cameraToUse.transform.forward, Vector3.up);
            }

            if (rayLine != null && rightRayOrigin != null)
            {
                rayLine.SetPosition(0, rightRayOrigin.position);
                rayLine.SetPosition(1, rightRayOrigin.position + rightRayOrigin.forward * 18f);
            }
        }

        public void EnsureRig()
        {
            rightHandRoot = EnsureChild(transform, "Simulated Right Hand");
            rightRayOrigin = EnsureChild(rightHandRoot, "Right Hand Ray Origin");
            uiAnchor = EnsureChild(rightHandRoot, "Right Wrist UI Anchor");

            rightHandRoot.localPosition = Vector3.zero;
            rightHandRoot.localRotation = Quaternion.identity;
            rightRayOrigin.localPosition = new Vector3(0f, 0f, 0.1f);
            rightRayOrigin.localRotation = Quaternion.identity;
            uiAnchor.localPosition = new Vector3(-0.16f, 0.08f, 0.22f);
            uiAnchor.localRotation = Quaternion.Euler(18f, 18f, 0f);

            if (controllerBody == null)
            {
                var existing = rightHandRoot.Find("Right Hand Controller Body");
                controllerBody = existing != null ? existing.gameObject : GameObject.CreatePrimitive(PrimitiveType.Cube);
                controllerBody.name = "Right Hand Controller Body";
                controllerBody.transform.SetParent(rightHandRoot, false);
                controllerBody.transform.localPosition = Vector3.zero;
                controllerBody.transform.localRotation = Quaternion.identity;
                controllerBody.transform.localScale = new Vector3(0.18f, 0.12f, 0.32f);
                var collider = controllerBody.GetComponent<Collider>();
                if (collider != null)
                    DestroyUnityObject(collider);
            }

            if (rayLine == null)
            {
                var rayObject = new GameObject("Right Hand Ray");
                rayObject.transform.SetParent(rightRayOrigin, false);
                rayLine = rayObject.AddComponent<LineRenderer>();
                rayLine.useWorldSpace = true;
                rayLine.positionCount = 2;
                rayLine.widthMultiplier = 0.025f;
                rayLine.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
                rayLine.startColor = new Color(1f, 0.72f, 0.24f, 0.9f);
                rayLine.endColor = new Color(1f, 0.72f, 0.24f, 0.05f);
            }
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

        static void DestroyUnityObject(Object target)
        {
            if (Application.isPlaying)
                Destroy(target);
            else
                DestroyImmediate(target);
        }
    }
}
