using System.Collections;
using ProjectAegisRTS.UnityClient.CoreBridge;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.MapEditor
{
    public sealed class AegisMapDocumentPlayModeInitializer : MonoBehaviour
    {
        public RtsSimulationDriver driver;
        public bool startMatchOnLoad = true;
        public bool selectFirstCombatGroupOnLoad = true;
        public bool revealScenarioMapOnLoad = true;

        IEnumerator Start()
        {
            yield return null;

            if (driver == null)
                driver = FindAnyObjectByType<RtsSimulationDriver>();
            if (driver == null)
                yield break;

            if (revealScenarioMapOnLoad)
                driver.TryRevealScenarioMap();
            if (startMatchOnLoad)
                driver.TryStartMatch();
            if (selectFirstCombatGroupOnLoad)
                driver.TrySelectOwnedCombatGroup();
        }
    }
}
