using UnityEngine;

namespace ProjectAegisRTS.UnityClient.InputControls.XR
{
    public sealed class Stage3XrRigPlaceholder : MonoBehaviour
    {
        public Camera fallbackCamera;
        public Transform rigRoot;
        public Transform head;
        public Transform leftController;
        public Transform rightController;
        public Transform leftRay;
        public Transform rightRay;

        void Awake()
        {
            EnsureRig();
        }

        void Start()
        {
            EnsureRig();
        }

        public void EnsureRig()
        {
            rigRoot = EnsureChild(transform, "XR Rig Root");
            head = EnsureChild(rigRoot, "Head Camera");
            leftController = EnsureChild(rigRoot, "LeftController Placeholder");
            rightController = EnsureChild(rigRoot, "RightController Placeholder");
            leftRay = EnsureChild(leftController, "LeftRay Placeholder");
            rightRay = EnsureChild(rightController, "RightRay Placeholder");

            rigRoot.localPosition = Vector3.zero;
            rigRoot.localRotation = Quaternion.identity;
            head.localPosition = new Vector3(0f, 1.6f, -1.2f);
            head.localRotation = Quaternion.Euler(25f, 0f, 0f);
            leftController.localPosition = new Vector3(-0.25f, 1.2f, -0.45f);
            rightController.localPosition = new Vector3(0.25f, 1.2f, -0.45f);
            leftRay.localRotation = Quaternion.identity;
            rightRay.localRotation = Quaternion.identity;

            if (fallbackCamera == null)
                fallbackCamera = Camera.main;
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
