using ProjectAegisRTS.UnityClient.Board;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.UI.XR.RightHand
{
    public sealed class RightHandStatusPanel : MonoBehaviour
    {
        public Stage5DualHandModeCoordinator coordinator;
        public RightHandCommandRouter commandRouter;
        public BoardPlacementController boardPlacement;
        Text readoutText;

        void Start()
        {
            BuildIfNeeded();
        }

        void Update()
        {
            Refresh();
        }

        public void Refresh()
        {
            BuildIfNeeded();
            var mode = coordinator == null ? RightHandCommandMode.Disabled : coordinator.CurrentMode;
            var board = boardPlacement != null && boardPlacement.IsPlacementModeActive ? "board placement active" : "board placement inactive";
            var result = commandRouter == null || string.IsNullOrEmpty(commandRouter.LastCommandResult) ? "none" : commandRouter.LastCommandResult;
            readoutText.text = "Mode: " + mode + "\nBoard: " + board + "\nSpace/MMB: manipulate\nQ/E rotate  wheel zoom\n" + result;
        }

        void BuildIfNeeded()
        {
            if (readoutText != null)
                return;

            RtsUiFactory.Stretch(gameObject, Vector2.zero, Vector2.zero);
            RtsUiFactory.AddPanel(gameObject, new Color(0.05f, 0.045f, 0.035f, 0.78f));
            readoutText = RtsUiFactory.CreateText(transform, "Right Hand Status Text", "Right-hand status ready.", 11, Color.white, TextAnchor.UpperLeft);
            readoutText.rectTransform.offsetMin = new Vector2(8f, 8f);
            readoutText.rectTransform.offsetMax = new Vector2(-8f, -8f);
        }
    }
}
