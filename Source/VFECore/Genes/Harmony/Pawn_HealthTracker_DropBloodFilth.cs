using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using System.Linq;

namespace VanillaGenesExpanded
{
    [HarmonyPatch(typeof(Pawn_HealthTracker), "DropBloodFilth")]
    public static class VanillaGenesExpanded_Pawn_HealthTracker_DropBloodFilth_Patch
    {
        public static bool Prefix(Pawn_HealthTracker __instance,Pawn ___pawn)
        {

            if (ModLister.BiotechInstalled && ___pawn.RaceProps.Humanlike && ___pawn.genes != null)
            {
                if (___pawn.genes.GenesListForReading.Where(x => x.Active).Any(g => g.def.GetModExtension<GeneExtension>()?.customBloodThingDef != null))
                {
                    ThingDef blood = ___pawn.genes.GenesListForReading.Where(x => x.Active).First(g => g.def.GetModExtension<GeneExtension>()?.customBloodThingDef != null).def.GetModExtension<GeneExtension>()?.customBloodThingDef;

                    FilthMaker.TryMakeFilth(___pawn.PositionHeld, ___pawn.MapHeld, blood, ___pawn.LabelIndefinite());
                    return false;

                }
            }
            return true;


        }
    }
}
