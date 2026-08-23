using HarmonyLib;
using Verse;

namespace VEF.AnimalGenes
{

    [HarmonyPatch(typeof(Corpse), nameof(Corpse.SpawnSetup))]
    public static class VEF_AnimalGenes_Corpse_SpawnSetup_Patch
    {
        public static void Postfix(Corpse __instance, bool respawningAfterLoad)
        {
            Pawn pawn = __instance.InnerPawn;

            if (pawn == null)
                return;

            CompAnimalGenes comp = pawn.TryGetComp<CompAnimalGenes>();

            if (comp != null)
            {
                WorldComponent_AnimalGenes.Instance.AddAnimalComp(pawn, comp);
            }
        }
    }
}