using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Art.Production
{
    public sealed class Stage29VisualDetailTag : MonoBehaviour
    {
        public string actorTypeId;
        public bool hasRealisticMaterialPass;
        public bool hasSilhouetteBreakup;
        public bool hasFineGridGrounding;
        public bool hasReadableTopProfile;
        public bool hasFrontSideRearCues;
        public bool preservesStage20Sockets;
        public bool preservesAnimationHooks;
        public bool questSafePrimitiveBudget;
        [TextArea(2, 5)] public string notes;

        public bool IsComplete()
        {
            return !string.IsNullOrEmpty(actorTypeId) &&
                hasRealisticMaterialPass &&
                hasSilhouetteBreakup &&
                hasFineGridGrounding &&
                hasReadableTopProfile &&
                hasFrontSideRearCues &&
                preservesStage20Sockets &&
                preservesAnimationHooks &&
                questSafePrimitiveBudget;
        }
    }
}
