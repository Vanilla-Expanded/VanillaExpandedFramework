using Verse;
using RimWorld;
using HarmonyLib;

namespace VEF.Pawns
{
    [HarmonyPatch(typeof(PawnComponentsUtility), "CreateInitialComponents")]
    public static class VanillaExpandedFramework_PawnComponentsUtility_CreateInitialComponents_Patch
    {
        public static void Postfix(Pawn pawn)
        {
            if (pawn?.kindDef?.skills != null)
            {
                if (pawn.skills is null)
                {
                    pawn.skills = new Pawn_SkillTracker(pawn);
                }
                if (pawn.story is null)
                {
                    pawn.story = new Pawn_StoryTracker(pawn);
                }
                if (!pawn.RaceProps.Humanlike)
                {
                    // TODO: Fix
                    // NonPublicMethods.GenerateSkills(pawn);
                }
            }
        }
    }

}
