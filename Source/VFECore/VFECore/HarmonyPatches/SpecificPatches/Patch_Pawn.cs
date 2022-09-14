using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VFECore
{

    public static class Patch_Pawn
    {

        [HarmonyPatch(typeof(Pawn), nameof(Pawn.SetFaction))]
        public static class SetFaction
        {

            public static void Postfix(Pawn __instance, Faction newFaction)
            {
                // Re-resolve pack animal graphics
                if (__instance.RaceProps.packAnimal)
                {
                    __instance.Drawer?.renderer?.graphics?.ResolveAllGraphics();
                }
            }

        }
    }

    [HarmonyPatch(typeof(Pawn_InteractionsTracker), "TryInteractWith")]
    public static class TryInteractWith_Patch
    {
        private static Dictionary<Hediff, HediffComp_Spreadable> cachedComps = new Dictionary<Hediff, HediffComp_Spreadable>();
        private static bool TryGetCachedSpreadableComp(this Hediff hediff, out HediffComp_Spreadable comp)
        {
            if (!cachedComps.TryGetValue(hediff, out comp))
            {
                cachedComps[hediff] = comp = hediff.TryGetComp<HediffComp_Spreadable>();
            }
            return comp != null;
        }
        public static void Postfix(bool __result, Pawn ___pawn, Pawn recipient)
        {
            if (__result)
            {
                if (___pawn.health?.hediffSet?.hediffs != null)
                {
                    foreach (var hediff in ___pawn.health.hediffSet.hediffs)
                    {
                        if (hediff.TryGetCachedSpreadableComp(out var comp))
                        {
                            if (Rand.Chance(comp.Props.socialInteractionTransmissionChance))
                            {
                                comp.TrySpreadDiseaseOn(recipient);
                            }
                        }
                    }
                }

                if (recipient.health?.hediffSet?.hediffs != null)
                {
                    foreach (var hediff in recipient.health.hediffSet.hediffs)
                    {
                        if (hediff.TryGetCachedSpreadableComp(out var comp))
                        {
                            if (Rand.Chance(comp.Props.socialInteractionTransmissionChance))
                            {
                                comp.TrySpreadDiseaseOn(___pawn);
                            }
                        }
                    }
                }
            }
        }
    }
}
