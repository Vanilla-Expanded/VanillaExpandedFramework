using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VEF.Storyteller
{
    [HarmonyPatch(typeof(Site), nameof(Site.ShouldRemoveMapNow))]
    public static class Site_ShouldRemoveMapNow_Patch
    {
        public static void Postfix(Site __instance, ref bool __result, ref bool alsoRemoveWorldObject)
        {
            if (__result && alsoRemoveWorldObject && __instance.parts != null)
            {
                foreach (var quest in Find.QuestManager.QuestsListForReading)
                {
                    if (quest.State == QuestState.Ongoing)
                    {
                        foreach (var part in quest.PartsListForReading)
                        {
                            if (part is QuestPart_KeepSite keepSite && keepSite.mapParent == __instance)
                            {
                                alsoRemoveWorldObject = false;
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}
