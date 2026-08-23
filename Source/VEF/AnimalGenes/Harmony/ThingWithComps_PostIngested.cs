using HarmonyLib;
using Verse;

namespace VEF.AnimalGenes
{

    [HarmonyPatch(typeof(ThingWithComps), "PostIngested")]
    public static class VEF_AnimalGenes_ThingWithComps_PostIngested_Patch
    {
        public static void Postfix(ThingWithComps __instance)
        {
            WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes.Remove(__instance);
        }
    }


}