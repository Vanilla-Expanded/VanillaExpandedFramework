using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Linq;
using Verse;
using Verse.AI.Group;
using static Verse.DamageWorker;

namespace VanillaStorytellersExpanded
{

    [HarmonyPatch(typeof(DamageInfo), "Amount", MethodType.Getter)]
    public static class Patch_Amount
    {
        public static void Postfix(DamageInfo __instance, float ___amountInt, ref float __result)
        {
            if (__instance.Weapon != null)
            {
                var options = Find.Storyteller.def.GetModExtension<StorytellerDefExtension>();
                if (options != null && options.storytellerThreat != null)
                {
                    __result *= options.storytellerThreat.allDamagesMultiplier;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("Kill")]
    public static class Patch_Kill
    {
        public static void Prefix(Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit = null)
        {
            if (__instance.IsColonist && dinfo.HasValue && dinfo.Value.Instigator?.Faction != null 
                && dinfo.Value.Instigator.Faction == Current.Game.GetComponent<StorytellerWatcher>()?.currentRaidingFaction)
            {
                var options = Find.Storyteller.def.GetModExtension<StorytellerDefExtension>();
                if (options != null && options.storytellerThreat != null)
                {
                    IncidentParms parms = new IncidentParms
                    {
                        target = __instance.Map,
                        faction = Current.Game.GetComponent<StorytellerWatcher>().currentRaidingFaction,
                        forced = true,
                        raidStrategy = RaidStrategyDefOf.ImmediateAttack,
                        points = StorytellerUtility.DefaultThreatPointsNow(__instance.Map) / 4f
                    };
                    VSEDefOf.VSE_Reinforcements.Worker.TryExecute(parms);
                }
            }
        }
    }

    [HarmonyPatch(typeof(IncidentWorker))]
    [HarmonyPatch("TryExecute")]
    public static class Patch_TryExecute
    {
        public static bool Prefix(IncidentWorker __instance, IncidentParms parms)
        {
            if (__instance.def.category == IncidentCategoryDefOf.ThreatBig)
            {
                var options = Find.Storyteller.def.GetModExtension<StorytellerDefExtension>();
                if (options != null && options.storytellerThreat != null 
                    && options.storytellerThreat.disableThreatsAtPopulationCount >= Find.ColonistBar.Entries.Where(x => x.pawn.Faction == Faction.OfPlayer).Count())
                {
                    Log.Message("TryExecute is disabled");
                    return false;
                }
            }
            Log.Message("TryExecute is enabled");
            return true;
        }
    }

    [HarmonyPatch(typeof(IncidentWorker_RaidEnemy))]
    [HarmonyPatch("TryExecuteWorker")]
    public static class Patch_TryExecuteWorker
    {
        public static void Postfix(bool __result, IncidentParms parms)
        {
            var options = Find.Storyteller.def.GetModExtension<StorytellerDefExtension>();
            if (options != null && options.storytellerThreat != null && __result)
            {
                var comp = Current.Game.GetComponent<StorytellerWatcher>();
                if (comp != null)
                {
                    comp.currentRaidingFaction = parms.faction;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Transition))]
    [HarmonyPatch("Execute")]
    public static class Patch_Execute
    {
        public static void Prefix(Transition __instance, Lord lord)
        {
            var options = Find.Storyteller.def.GetModExtension<StorytellerDefExtension>();
            if (options != null && options.storytellerThreat != null && lord.LordJob is LordJob_AssaultColony lordJob_AssaultColony)
            {
                if (__instance.canMoveToSameState || __instance.target != lord.CurLordToil)
                {
                    for (int i = 0; i < __instance.preActions.Count; i++)
                    {
                        if (__instance.preActions[i] is TransitionAction_Message transitionAction)
                        {
                            if (transitionAction.message == "MessageRaidersGivenUpLeaving".Translate(lord.faction.def.pawnsPlural.CapitalizeFirst(), lord.faction.Name) 
                                || transitionAction.message == "MessageFightersFleeing".Translate(lord.faction.def.pawnsPlural.CapitalizeFirst(), lord.faction.Name))
                            {
                                IncidentParms parms = new IncidentParms
                                {
                                    target = lord.Map,
                                    forced = true,
                                    points = StorytellerUtility.DefaultThreatPointsNow(lord.Map)
                                };
                                var incidentDef = DefDatabase<IncidentDef>.GetNamed(options.storytellerThreat.goodIncidents.RandomElement());
                                if (incidentDef != null)
                                {
                                    Find.Storyteller.incidentQueue.Add(incidentDef, Find.TickManager.TicksGame + new IntRange(6000, 12000).RandomInRange, parms);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
