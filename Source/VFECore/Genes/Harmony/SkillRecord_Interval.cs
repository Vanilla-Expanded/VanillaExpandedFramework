using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace VanillaGenesExpanded
{
    [HarmonyPatch(typeof(SkillRecord), "Interval")]
    public static class VanillaGenesExpanded_SkillRecord_Interval_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn ___pawn, SkillRecord __instance)
        {
            
            if (StaticCollectionsClass.noSkillLoss_gene_pawns.ContainsKey(___pawn) && StaticCollectionsClass.noSkillLoss_gene_pawns[___pawn]== __instance.def)
            {
                return false;
            }
            return true;

        }


    }
}
