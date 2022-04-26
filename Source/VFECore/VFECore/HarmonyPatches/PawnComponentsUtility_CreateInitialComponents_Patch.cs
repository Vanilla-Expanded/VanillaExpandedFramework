using Verse;
using RimWorld;
using HarmonyLib;

namespace VFECore
{
    [HarmonyPatch(typeof(PawnComponentsUtility), "CreateInitialComponents")]
    public static class PawnComponentsUtility_CreateInitialComponents_Patch
    {
        public static void Postfix(Pawn pawn)
        {
            if (pawn.kindDef.skills != null)
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
                    Log.Message(pawn.skills.GetSkill(SkillDefOf.Shooting).Level + " - " + pawn.skills.GetSkill(SkillDefOf.Shooting).LevelDescriptor);
                    NonPublicMethods.GenerateSkills(pawn);
                    Log.Message(pawn.skills.GetSkill(SkillDefOf.Shooting).Level + " - " + pawn.skills.GetSkill(SkillDefOf.Shooting).LevelDescriptor);
                }
            }
        }
    }

}
