using HarmonyLib;
using RimWorld;
using Verse;

namespace VFECore
{
    [HarmonyPatch(typeof(Pawn_RelationsTracker), "SetPregnancyApproach")]
    public static class Pawn_RelationsTracker_SetPregnancyApproach_Patch
    {
        public static void Postfix(Pawn ___pawn, Pawn partner)
        {
            ___pawn.relations.GetAdditionalPregnancyApproachData().partners.Remove(partner);
            partner.relations.GetAdditionalPregnancyApproachData().partners.Remove(___pawn);
        }
    }
}
