using System.Collections.Generic;
using ProjectAegisRTS.Core;

namespace ProjectAegisRTS.Air
{
    public sealed class AircraftState
    {
        public int ActorId { get; private set; }
        public int HomeAirfieldActorId { get; set; }
        public int DockedAirfieldActorId { get; set; }
        public int AssignedPadIndex { get; set; }
        public int AltitudeSubCells { get; set; }
        public int FuelTicksRemaining { get; set; }
        public int RearmProgressTicks { get; set; }
        public bool IsAirborne { get; set; }

        public AircraftState(int actorId, int altitudeSubCells, int fuelTicks)
        {
            ActorId = actorId;
            HomeAirfieldActorId = 0;
            DockedAirfieldActorId = 0;
            AssignedPadIndex = -1;
            AltitudeSubCells = altitudeSubCells;
            FuelTicksRemaining = fuelTicks;
            RearmProgressTicks = 0;
            IsAirborne = false;
        }
    }

    public sealed class AirfieldPadState
    {
        public int PadIndex { get; private set; }
        public Int2 Cell { get; private set; }
        public int OccupiedAircraftActorId { get; set; }

        public AirfieldPadState(int padIndex, Int2 cell)
        {
            PadIndex = padIndex;
            Cell = cell;
            OccupiedAircraftActorId = 0;
        }
    }

    public sealed class AirfieldState
    {
        readonly List<AirfieldPadState> pads;

        public int ActorId { get; private set; }
        public IReadOnlyList<AirfieldPadState> Pads { get { return pads; } }

        public AirfieldState(int actorId, IReadOnlyList<Int2> padCells)
        {
            ActorId = actorId;
            pads = new List<AirfieldPadState>();
            if (padCells != null)
                for (var i = 0; i < padCells.Count; i++)
                    pads.Add(new AirfieldPadState(i, padCells[i]));
        }

        public bool TryReservePad(int aircraftActorId, out AirfieldPadState pad)
        {
            for (var i = 0; i < pads.Count; i++)
            {
                if (pads[i].OccupiedAircraftActorId == aircraftActorId)
                {
                    pad = pads[i];
                    return true;
                }

                if (pads[i].OccupiedAircraftActorId == 0)
                {
                    pads[i].OccupiedAircraftActorId = aircraftActorId;
                    pad = pads[i];
                    return true;
                }
            }

            pad = null;
            return false;
        }

        public void ReleaseAircraft(int aircraftActorId)
        {
            for (var i = 0; i < pads.Count; i++)
                if (pads[i].OccupiedAircraftActorId == aircraftActorId)
                    pads[i].OccupiedAircraftActorId = 0;
        }
    }
}
