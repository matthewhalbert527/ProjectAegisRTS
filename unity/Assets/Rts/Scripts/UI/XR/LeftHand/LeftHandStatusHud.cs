using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Selection;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.UI.XR.LeftHand
{
    public sealed class LeftHandStatusHud : MonoBehaviour
    {
        public RtsSimulationDriver driver;
        public Stage4ModeCoordinator modeCoordinator;
        public LeftHandBuildMenuController buildMenu;
        public LeftHandSelectionController selectionController;
        public LeftHandCommandRouter commandRouter;
        public bool visible = true;
        Text readoutText;

        void Start()
        {
            BuildIfNeeded();
        }

        void Update()
        {
            BuildIfNeeded();
            gameObject.SetActive(visible);
            if (!visible)
                return;

            var mode = modeCoordinator == null ? LeftHandCommandMode.Disabled : modeCoordinator.CurrentMode;
            var category = buildMenu == null ? "-" : buildMenu.ActiveCategory.ToString();
            var item = buildMenu == null || string.IsNullOrEmpty(buildMenu.SelectedActorTypeId) ? "none" : buildMenu.SelectedActorTypeId;
            var hovered = driver == null ? "none" : driver.HoveredCellText();
            var selected = driver == null ? "none" : driver.SelectedActorIdsText();
            var input = modeCoordinator == null ? "none" : modeCoordinator.ActiveInputSourceName;
            var result = commandRouter == null || string.IsNullOrEmpty(commandRouter.LastCommandResult) ? "none" : commandRouter.LastCommandResult;
            readoutText.text = "Mode: " + mode + "\nCategory: " + category + "\nBuild: " + item + "\nHovered: " + hovered + "\nSelected: " + selected + "\nInput: " + input + "\nLast: " + result;
        }

        public void Toggle()
        {
            visible = !visible;
            gameObject.SetActive(visible);
        }

        void BuildIfNeeded()
        {
            if (readoutText != null)
                return;

            RtsUiFactory.Stretch(gameObject, Vector2.zero, Vector2.zero);
            RtsUiFactory.AddPanel(gameObject, new Color(0.02f, 0.025f, 0.03f, 0.80f));
            readoutText = RtsUiFactory.CreateText(transform, "Stage4 Status Text", "Stage 4 status.", 12, Color.white, TextAnchor.UpperLeft);
            readoutText.rectTransform.offsetMin = new Vector2(8f, 8f);
            readoutText.rectTransform.offsetMax = new Vector2(-8f, -8f);
        }
    }
}
