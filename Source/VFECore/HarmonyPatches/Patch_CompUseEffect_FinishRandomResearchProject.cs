using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VFECore
{

    public static class Patch_CompUseEffect_FinishRandomResearchProject
    {

        [HarmonyPatch(typeof(CompUseEffect_FinishRandomResearchProject), nameof(CompUseEffect_FinishRandomResearchProject.DoEffect))]
        public static class DoEffect
        {

            public static void Postfix(CompUseEffect_FinishRandomResearchProject __instance, Pawn usedBy)
            {
                // If there's no active research project and all storyteller-permitted research projects have been finished, finish a random project instead (favouring lower tech levels)
                if (Find.ResearchManager.currentProj == null && CustomStorytellerUtility.AllowedResearchProjectDefs().All(r => r.IsFinished) && CustomStorytellerUtility.TryGetRandomUnfinishedResearchProject(out ResearchProjectDef research))
                {
                    NonPublicMethods.CompUseEffect_FinishRandomResearchProject_FinishInstantly(__instance, research, usedBy);
                }
            }

        }

        [HarmonyPatch(typeof(CompUseEffect_FinishRandomResearchProject), nameof(CompUseEffect_FinishRandomResearchProject.CanBeUsedBy))]
        public static class CanBeUsedBy
        {

            public static void Postfix(CompUseEffect_FinishRandomResearchProject __instance, Pawn p, ref string failReason, ref bool __result)
            {
                if (Find.ResearchManager.currentProj == null && CustomStorytellerUtility.AllowedResearchProjectDefs().All(r => r.IsFinished) && CustomStorytellerUtility.TryGetRandomUnfinishedResearchProject(out ResearchProjectDef research))
                {
                    failReason = null;
                    __result = true;
                }
            }

        }

    }

}
