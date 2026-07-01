using System;

namespace ProjectAegisRTS.UnityClient.Art.Production
{
    [Flags]
    public enum ProductionVisualViewCoverage
    {
        None = 0,
        Top = 1 << 0,
        Front = 1 << 1,
        Back = 1 << 2,
        Left = 1 << 3,
        Right = 1 << 4,
        Roof = 1 << 5,
        AllAround = Top | Front | Back | Left | Right | Roof
    }
}
