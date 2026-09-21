using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VEF.AnimalGenes
{

    [HarmonyPatch(typeof(GeneUtility), nameof(GeneUtility.SterileGenes))]
    public static class VEF_AnimalGenes_GeneUtility_SterileGenes_Patch
    {
        public static void Postfix(ref bool __result, Pawn pawn)
        {
            if (!__result && WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes.ContainsKey(pawn))
            {
                CompAnimalGenes comp = WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes[pawn];
                if (comp != null)
                {
                    List<AnimalGeneDef> genesListForReading = comp.genes;
                    for (int i = 0; i < genesListForReading.Count; i++)
                    {
                        if (genesListForReading[i]?.makeAnimalSterile == true)
                        {
                            __result = true;
                            break;
                        }

                    }
                }
            }

        }
    }
}