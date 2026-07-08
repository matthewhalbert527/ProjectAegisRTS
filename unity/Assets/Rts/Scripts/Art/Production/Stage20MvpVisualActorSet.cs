namespace ProjectAegisRTS.UnityClient.Art.Production
{
    public static class Stage20MvpVisualActorSet
    {
        public static readonly string[] ActorTypeIds =
        {
            "fabrication_hub",
            "power_plant",
            "refinery",
            "barracks",
            "war_factory",
            "gun_tower",
            "rifle_infantry",
            "light_tank",
            "harvester"
        };

        public static bool Contains(string actorTypeId)
        {
            if (string.IsNullOrEmpty(actorTypeId))
                return false;

            for (var i = 0; i < ActorTypeIds.Length; i++)
                if (ActorTypeIds[i] == actorTypeId)
                    return true;
            return false;
        }
    }
}
