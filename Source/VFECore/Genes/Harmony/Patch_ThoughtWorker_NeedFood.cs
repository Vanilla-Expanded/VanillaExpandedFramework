using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VanillaGenesExpanded
{
    [HarmonyPatch]
    public static class CurrentSocialStateInternal_Patch
    {
        [HarmonyPatch(typeof(ThoughtWorker_NeedFood), "CurrentStateInternal")]
        [HarmonyPostfix]
        public static void CurrentStateInternal_Postfix(ref ThoughtState __result, Pawn p)
        {
            if (__result.Active && p.genes.GetActiveGeneExtensions().Any(x => x.doubleNegativeFoodThought))
            {
                int stage = __result.StageIndex * 2;
                if (stage == 0) stage = 1;

                if (stage > 6) // Don't go out of bounds
                {
                    stage = 6;
                }
                __result = ThoughtState.ActiveAtStage(stage);
            }
        }
    }
}