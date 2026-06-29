using ProjectAegisRTS.UnityClient.Board;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.UI.XR
{
    public sealed class BoardPlacementHud : MonoBehaviour
    {
        public BoardPlacementController controller;
        public Transform recenterReference;
        Text readout;

        void Start()
        {
            BuildIfNeeded();
        }

        void Update()
        {
            BuildIfNeeded();
            Refresh();
        }

        public void Initialize(BoardPlacementController placementController, Transform reference)
        {
            controller = placementController;
            recenterReference = reference;
            BuildIfNeeded();
            Refresh();
        }

        void BuildIfNeeded()
        {
            if (readout != null)
                return;

            RtsUiFactory.Stretch(gameObject, Vector2.zero, Vector2.zero);
            RtsUiFactory.AddPanel(gameObject, new Color(0.04f, 0.05f, 0.07f, 0.86f));

            var layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(12, 12, 12, 12);
            layout.spacing = 6f;
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;

            readout = RtsUiFactory.CreateText(transform, "Stage 3 Placement Readout", string.Empty, 13, Color.white, TextAnchor.UpperLeft);
            readout.rectTransform.sizeDelta = new Vector2(360f, 190f);

            AddButton("Toggle Placement Mode", () => { if (controller != null) controller.TogglePlacementMode(); });
            AddButton("Confirm Placement", () => { if (controller != null) controller.ConfirmPlacement(); });
            AddButton("Cancel", () => { if (controller != null) controller.CancelPlacement(); });
            AddButton("Reset", () => { if (controller != null) controller.ResetPlacement(); });
            AddButton("Save", () => { if (controller != null) controller.Save(); });
            AddButton("Load", () => { if (controller != null) controller.Load(); });
            AddButton("Recenter", Recenter);
        }

        void AddButton(string text, UnityEngine.Events.UnityAction action)
        {
            var button = RtsUiFactory.CreateButton(transform, text, text);
            button.GetComponent<RectTransform>().sizeDelta = new Vector2(220f, 30f);
            button.onClick.AddListener(action);
        }

        void Refresh()
        {
            if (readout == null)
                return;

            if (controller == null)
            {
                readout.text = "Stage 3 board placement controller missing.";
                return;
            }

            readout.text =
                "Stage 3 Board Placement\n" +
                "Mode: " + (controller.IsPlacementModeActive ? "Placement" : "Normal") + "\n" +
                "Height: " + controller.CurrentHeight.ToString("0.00") + "\n" +
                "Yaw: " + controller.CurrentYaw.ToString("0.0") + "\n" +
                "Scale: " + controller.CurrentScale.ToString("0.00") + "\n" +
                "Meters/Cell: " + controller.CurrentMetersPerCell.ToString("0.00") + "\n" +
                "Status: " + controller.StatusText + "\n\n" +
                "Tab toggle, Enter confirm, Esc cancel\n" +
                "WASD/Arrows move, Q/E yaw\n" +
                "Z/X or PgUp/PgDn height\n" +
                "Shift/Ctrl + wheel scales";
        }

        void Recenter()
        {
            if (controller == null)
                return;

            var position = recenterReference != null ? recenterReference.position : Vector3.zero;
            controller.Recenter(new Vector3(position.x, controller.CurrentHeight, position.z));
        }
    }
}
