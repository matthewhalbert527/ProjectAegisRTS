using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Selection;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.UI.XR.LeftHand
{
    public sealed class LeftHandSelectionPanel : MonoBehaviour
    {
        public RtsSimulationDriver driver;
        public LeftHandSelectionController selectionController;
        Text readoutText;

        void Start()
        {
            BuildIfNeeded();
        }

        void Update()
        {
            Refresh();
        }

        public void CycleCandidate(int direction)
        {
            if (selectionController != null)
                selectionController.CycleCandidate(direction);
        }

        public void Refresh()
        {
            BuildIfNeeded();
            var selected = driver == null ? "none" : driver.SelectedActorIdsText();
            var candidate = selectionController == null || selectionController.CurrentCandidate == null ? "none" : selectionController.CurrentCandidate.ToString();
            var candidates = selectionController == null ? new List<LeftHandSelectionCandidate>() : selectionController.Candidates;
            var lines = "Selected: " + selected + "\nCandidate: " + candidate + "\nCandidates: " + candidates.Count + "\n";
            for (var i = 0; i < candidates.Count && i < 5; i++)
                lines += (i == selectionController.CurrentCandidateIndex ? "> " : "  ") + candidates[i] + "\n";
            lines += "Tab cycle, Ctrl+click additive, L drag lasso";
            readoutText.text = lines;
        }

        void BuildIfNeeded()
        {
            if (readoutText != null)
                return;

            RtsUiFactory.Stretch(gameObject, Vector2.zero, Vector2.zero);
            RtsUiFactory.AddPanel(gameObject, new Color(0.04f, 0.05f, 0.07f, 0.84f));
            readoutText = RtsUiFactory.CreateText(transform, "Selection Readout", "Selection ready.", 12, Color.white, TextAnchor.UpperLeft);
            readoutText.rectTransform.offsetMin = new Vector2(8f, 8f);
            readoutText.rectTransform.offsetMax = new Vector2(-8f, -8f);
        }
    }
}
