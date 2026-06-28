using ProjectAegisRTS.Core;
using ProjectAegisRTS.Simulation;

namespace ProjectAegisRTS.Demo
{
    public static class DemoWorldFactory
    {
        public static RtsWorld CreateMvpWorld()
        {
            var rules = DemoRules.CreateDefaultRules();
            var world = new RtsWorld(rules, new GridMap(32, 32));
            world.AddPlayer(1, "Aegis Test Player", 5000);
            world.CreateActor("fabrication_hub", 1, new Int2(4, 4));
            world.CreateActor("scout_rover", 1, new Int2(10, 4));
            return world;
        }
    }
}
