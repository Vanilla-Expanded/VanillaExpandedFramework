using HarmonyLib;
using RimWorld;

namespace VEF.Storyteller
{
    [HarmonyPatch(typeof(ResearchManager), "FinishProject")]
    public static class VanillaExpandedFramework_ResearchManager_FinishProject_Patch
    {
        public static void Postfix()
        {
            GameComponent_QuestChains.Instance.TryScheduleQuests();
        }
    }
}
