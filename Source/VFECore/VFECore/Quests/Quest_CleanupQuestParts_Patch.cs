using HarmonyLib;
using RimWorld;

namespace VFECore
{
    [HarmonyPatch(typeof(Quest), "CleanupQuestParts")]
    public static class Quest_CleanupQuestParts_Patch
    {
        public static void Prefix(Quest __instance, QuestEndOutcome ___endOutcome)
        {
            var extension = __instance.root.GetModExtension<QuestChainExtension>();
            if (extension != null)
            {
                if (___endOutcome == QuestEndOutcome.Success || ___endOutcome == QuestEndOutcome.Fail)
                {
                    GameComponent_QuestChains.Instance.QuestCompleted(__instance, ___endOutcome);
                }
                else if (__instance.State == QuestState.EndedOfferExpired)
                {
                    GameComponent_QuestChains.Instance.QuestExpired(__instance);
                }
            }
        }
    }
}