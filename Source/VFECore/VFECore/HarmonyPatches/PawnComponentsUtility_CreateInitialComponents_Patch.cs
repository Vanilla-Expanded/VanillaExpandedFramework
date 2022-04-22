using Verse;
using RimWorld;
using HarmonyLib;

namespace VFECore
{
    [HarmonyPatch(typeof(PawnComponentsUtility), "CreateInitialComponents")]
    public static class PawnComponentsUtility_CreateInitialComponents_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn)
        {
            if (pawn.skills is null && pawn.kindDef.skills != null)
            {
                pawn.skills = new Pawn_SkillTracker(pawn);
            }
        }
    }

}
