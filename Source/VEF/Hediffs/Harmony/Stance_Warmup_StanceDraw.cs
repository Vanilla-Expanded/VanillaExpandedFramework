using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VEF.Hediffs
{
    [HarmonyPatch(typeof(Stance_Warmup), "StanceDraw")]
    public static class VanillaExpandedFramework_Stance_Warmup_StanceDraw_Patch
    {
        public static void Postfix(Stance_Warmup __instance)
        {
            if (__instance.stanceTracker.pawn.health?.hediffSet?.hediffs != null)
            {
                foreach (var hediff in __instance.stanceTracker.pawn.health.hediffSet.hediffs)
                {
                    var comp = hediff.TryGetComp<HediffComp_Targeting>();
                    if (comp != null)
                    {
                        comp.DrawTargetingEffects(__instance.focusTarg);
                    }
                }
            }
        }
    }
}
