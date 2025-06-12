using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;
using VEF;

namespace VEF.Genes
{


    [HarmonyPatch(typeof(Pawn_SkillTracker))]
    [HarmonyPatch("Learn")]
    public static class VanillaExpandedFramework_Pawn_SkillTracker_Learn_Patch
    {
        [HarmonyPostfix]
        public static void GiveRecreation(Pawn ___pawn, SkillDef sDef, float xp)

        {
            if (xp > 0 && StaticCollectionsClass.skillRecreation_gene_pawns.ContainsKey(___pawn) && StaticCollectionsClass.skillRecreation_gene_pawns[___pawn] == sDef)
            {
                ___pawn.needs?.joy?.GainJoy(xp*0.001f, VEFDefOf.Gaming_Cerebral);
            }

        }
    }



   
}
