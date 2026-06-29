using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.UI.XR.RightHand
{
    public sealed class RightHandCommandHud : MonoBehaviour
    {
        public RtsSimulationDriver driver;
        public Stage5DualHandModeCoordinator coordinator;
        public RightHandCommandRouter commandRouter;
        public bool visible = true;
        Text readoutText;

        void Start()
        {
            BuildIfNeeded();
        }

        void Update()
        {
            Refresh();
        }

        public void Toggle()
        {
            visible = !visible;
            gameObject.SetActive(visible);
        }

        public void Refresh()
        {
            BuildIfNeeded();
            gameObject.SetActive(visible);
            if (!visible)
                return;

            var mode = coordinator == null ? RightHandCommandMode.Disabled : coordinator.CurrentMode;
            var hovered = driver == null ? "none" : driver.HoveredCellText();
            var selected = driver == null ? "none" : driver.SelectedActorIdsText();
            var source = coordinator == null ? "none" : coordinator.ActiveInputSourceName;
            var result = commandRouter == null || string.IsNullOrEmpty(commandRouter.LastCommandResult) ? "none" : commandRouter.LastCommandResult;
            readoutText.text = "RIGHT HAND COMMAND\nMode: " + mode + "\nHovered: " + hovered + "\nSelected: " + selected + "\nInput: " + source + "\nLast: " + result + "\nM move  A attack  F force  RMB confirm";
        }

        void BuildIfNeeded()
        {
            if (readoutText != null)
                return;

            RtsUiFactory.Stretch(gameObject, Vector2.zero, Vector2.zero);
            RtsUiFactory.AddPanel(gameObject, new Color(0.05f, 0.045f, 0.035f, 0.86f));
            readoutText = RtsUiFactory.CreateText(transform, "Right Hand Command Text", "Right-hand command ready.", 12, Color.white, TextAnchor.UpperLeft);
            readoutText.rectTransform.offsetMin = new Vector2(8f, 8f);
            readoutText.rectTransform.offsetMax = new Vector2(-8f, -8f);
        }
    }
}
