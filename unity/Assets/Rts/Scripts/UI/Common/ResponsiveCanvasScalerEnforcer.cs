using UnityEngine;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.UI.Common
{
    public sealed class ResponsiveCanvasScalerEnforcer : MonoBehaviour
    {
        public Vector2 referenceResolution = new Vector2(1920f, 1080f);
        [Range(0f, 1f)]
        public float matchWidthOrHeight = 0.5f;
        public bool enforceOnAwake = true;
        public bool enforceOnStart = true;
        public bool includeChildCanvases;
        public bool logAdjustments = true;

        void Awake()
        {
            if (enforceOnAwake)
                Enforce();
        }

        void Start()
        {
            if (enforceOnStart)
                Enforce();
        }

        public void Enforce()
        {
            var canvas = GetComponent<Canvas>();
            if (canvas != null)
                EnforceCanvas(canvas, referenceResolution, matchWidthOrHeight, logAdjustments);

            if (!includeChildCanvases)
                return;

            var childCanvases = GetComponentsInChildren<Canvas>(true);
            for (var i = 0; i < childCanvases.Length; i++)
                if (childCanvases[i] != null && childCanvases[i] != canvas)
                    EnforceCanvas(childCanvases[i], referenceResolution, matchWidthOrHeight, logAdjustments);
        }

        public static CanvasScaler EnforceCanvas(Canvas canvas, Vector2 referenceResolution, float matchWidthOrHeight, bool logAdjustments)
        {
            if (canvas == null)
                return null;

            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
                scaler = canvas.gameObject.AddComponent<CanvasScaler>();

            var changed = scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize ||
                scaler.referenceResolution != referenceResolution ||
                Mathf.Abs(scaler.matchWidthOrHeight - matchWidthOrHeight) > 0.001f;

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = referenceResolution;
            scaler.matchWidthOrHeight = matchWidthOrHeight;

            if (changed && logAdjustments)
                Debug.Log("[Stage21.5 Display] ResponsiveCanvasScalerEnforcer adjusted " + canvas.name + " to " + referenceResolution.x + "x" + referenceResolution.y + " match=" + matchWidthOrHeight + ".");

            return scaler;
        }
    }
}
