using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.AnimalGenes
{

    [HarmonyPatch(typeof(StatWorker))]
    [HarmonyPatch("GetOffsetsAndFactorsExplanation")]
    public class VEF_AnimalGenes_StatWorker_GetOffsetsAndFactorsExplanation_Patch
    {
        public static string whitespace = "";

        [HarmonyPostfix]
        public static void ApplyStatModifiersExplanations(StatDef ___stat, StatRequest req, StringBuilder sb, StatWorker __instance)
        {
            Pawn pawn = req.Thing as Pawn;
            if (pawn != null)
            {
                if (WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes.ContainsKey(pawn))
                {

                    CompAnimalGenes comp = WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes[pawn];
                    if (comp != null)
                    {
                        bool flag2 = false;
                        List<AnimalGeneDef> genesListForReading = comp.genes;
                        for (int num4 = 0; num4 < genesListForReading.Count; num4++)
                        {

                            float statOffsetFromList3 = genesListForReading[num4].statOffsets.GetStatOffsetFromList(___stat);
                            if (statOffsetFromList3 != 0f)
                            {
                                if (!flag2)
                                {
                                    sb.AppendLine(whitespace + "VRE_StatsReport_Genes".Translate());
                                    flag2 = true;
                                }
                                sb.AppendLine(whitespace + "    " + genesListForReading[num4].LabelCap + ": " + __instance.ValueToString(statOffsetFromList3, finalized: false, ToStringNumberSense.Offset));
                            }
                            float statFactorFromList3 = genesListForReading[num4].statFactors.GetStatFactorFromList(___stat);
                            if (statFactorFromList3 != 1f)
                            {
                                if (!flag2)
                                {
                                    sb.AppendLine(whitespace + "VRE_StatsReport_Genes".Translate());
                                    flag2 = true;
                                }
                                sb.AppendLine(whitespace + "    " + genesListForReading[num4].LabelCap + ": " + __instance.ValueToString(statFactorFromList3, finalized: false, ToStringNumberSense.Factor));
                            }

                        }
                    }




                }

            }

        }
    }
}