using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;
using VFEMech;

namespace VFECore
{
    [HarmonyPatch(typeof(Stance_Warmup), "StanceDraw")]
    public static class Stance_WarmupStanceDraw_Patch
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
                        float statValue = __instance.stanceTracker.pawn.GetStatValue(StatDefOf.AimingDelayFactor);
                        int ticks = (__instance.verb.verbProps.warmupTime * statValue).SecondsToTicks();
                        var progress = (float)__instance.ticksLeft / (float)ticks;
                        comp.DrawTargetingEffects(__instance.focusTarg, progress);
                    }
                }
            }
        }
    }
}
