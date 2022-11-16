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

            if (___pawn?.RaceProps.Humanlike==true)
            {
                if (StaticCollectionsClass.bloodtype_gene_pawns.ContainsKey(___pawn))
                {
                    ThingDef blood = StaticCollectionsClass.bloodtype_gene_pawns[___pawn];
                    FilthMaker.TryMakeFilth(___pawn.PositionHeld, ___pawn.MapHeld, blood, ___pawn.LabelIndefinite());
                    return false;
                }
               
            }
            return true;


        }
    }
}
