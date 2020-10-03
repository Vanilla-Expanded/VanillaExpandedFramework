using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace VanillaStorytellersExpanded
{

    [HarmonyPatch(typeof(SettlementDefeatUtility))]
    [HarmonyPatch("IsDefeated")]
    public static class Patch_IsDefeated
    {
        public static void Postfix(Map map, Faction faction, bool __result)
        {
            if (__result == true)
            {
                var options = Find.Storyteller.def.GetModExtension<StorytellerDefExtension>();
                if (options != null && options.raidRestlessness != null && faction.HostileTo(Faction.OfPlayer) 
                    && map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer).Count > 0)
                {
                    var comp = Current.Game.GetComponent<StorytellerWatcher>();
                    if (comp != null)
                    {
                        comp.lastRaidExpansionTicks = Find.TickManager.TicksGame;
                        //Log.Message("IsDefeated patch, story watcher is updated, the player defeated enemy base", true);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Quest))]
    [HarmonyPatch("End")]
    public static class Patch_End
    {
        public static void Prefix(Quest __instance, QuestEndOutcome outcome, bool sendLetter = true)
        {
            var options = Find.Storyteller.def.GetModExtension<StorytellerDefExtension>();
            if (options != null && options.raidRestlessness != null && HasMapNode(__instance.root.root))
            {
                //Log.Message("__instance.State: " + __instance.State, true);
                //Log.Message("outcome: " + outcome, true);
                var comp = Current.Game.GetComponent<StorytellerWatcher>();
                if (comp != null)
                {
                    if (outcome == QuestEndOutcome.Success || __instance.State == QuestState.EndedSuccess)
                    {
                        comp.lastRaidExpansionTicks = Find.TickManager.TicksGame;
                    }
                }

            }
        }

        public static bool HasMapNode(QuestNode node)
        {
            if (node is QuestNode_GenerateSite || node is QuestNode_GenerateWorldObject || node is QuestNode_GetSiteTile)
            {
                return true;
            }
            else if (node is QuestNode_RandomNode randomNode)
            {
                foreach (var node2 in randomNode.nodes)
                {
                    if (HasMapNode(node2))
                    {
                        return true;
                    }
                }
            }
            else if (node is QuestNode_Sequence sequence)
            {
                foreach (var node3 in sequence.nodes)
                {
                    if (HasMapNode(node3))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
