using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.UI.Common;
using ProjectAegisRTS.UnityClient.UI.XR.LeftHand;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Selection
{
    public sealed class LeftHandSelectionController : MonoBehaviour
    {
        readonly List<LeftHandSelectionCandidate> candidates = new List<LeftHandSelectionCandidate>();
        public RtsSimulationDriver driver;
        public BoardCoordinateMapper mapper;
        public LeftHandCommandRouter commandRouter;
        public RtsStatusLog statusLog;
        public float rayToleranceMeters = 0.85f;
        public int cellSearchRadius = 1;

        public List<LeftHandSelectionCandidate> Candidates { get { return candidates; } }
        public int CurrentCandidateIndex { get; private set; }
        public LeftHandSelectionCandidate CurrentCandidate
        {
            get
            {
                return candidates.Count == 0 || CurrentCandidateIndex < 0 || CurrentCandidateIndex >= candidates.Count ? null : candidates[CurrentCandidateIndex];
            }
        }

        public void Initialize(RtsSimulationDriver simulationDriver, BoardCoordinateMapper coordinateMapper, LeftHandCommandRouter router)
        {
            driver = simulationDriver;
            mapper = coordinateMapper;
            commandRouter = router;
        }

        public void RefreshCandidates(Ray ray, Int2? boardCell)
        {
            candidates.Clear();
            if (driver == null || mapper == null)
                return;

            candidates.AddRange(SelectionResolver.FindCandidates(driver.LatestSnapshot, driver, mapper, ray, boardCell, rayToleranceMeters, cellSearchRadius));
            if (CurrentCandidateIndex >= candidates.Count)
                CurrentCandidateIndex = 0;
            if (CurrentCandidateIndex < 0)
                CurrentCandidateIndex = 0;
        }

        public void RefreshCandidatesForCell(Int2 boardCell)
        {
            var origin = mapper != null ? mapper.CellToWorldCenter(boardCell) + Vector3.up * 10f : Vector3.up * 10f;
            RefreshCandidates(new Ray(origin, Vector3.down), boardCell);
        }

        public void CycleCandidate(int direction)
        {
            if (candidates.Count == 0)
                return;

            CurrentCandidateIndex = (CurrentCandidateIndex + (direction >= 0 ? 1 : -1) + candidates.Count) % candidates.Count;
        }

        public RtsCommandResult SelectCurrentCandidate(bool additive)
        {
            if (commandRouter == null)
                return RtsCommandResult.Fail("RouterMissing", "Left-hand command router is not available.");

            var candidate = CurrentCandidate;
            if (additive)
                return commandRouter.AddOrRemoveCandidate(candidate);
            return commandRouter.SelectCandidate(candidate);
        }

        public RtsCommandResult ClearSelection()
        {
            return commandRouter != null ? commandRouter.ClearSelection() : RtsCommandResult.Fail("RouterMissing", "Left-hand command router is not available.");
        }
    }
}
