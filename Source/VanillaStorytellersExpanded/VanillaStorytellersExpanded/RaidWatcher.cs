using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;
using static Verse.DamageWorker;

namespace VanillaStorytellersExpanded
{

    [HarmonyPatch(typeof(DamageWorker_AddInjury), "ApplyDamageToPart")]
    public static class Patch_ApplyDamageToPart
    {
        public static void Prefix(ref DamageInfo dinfo, Pawn pawn, DamageResult result)
        {
            if (dinfo.Weapon != null)
            {
                var options = Find.Storyteller.def.GetModExtension<StorytellerDefExtension>();
                if (options != null && options.storytellerThreat != null)
                {
                    //Log.Message("Before: " + dinfo + " - " + result, true);
                    dinfo.SetAmount(dinfo.Amount * options.storytellerThreat.allDamagesMultiplier);
                }
            }
        }

        //public static void Postfix(DamageInfo dinfo, Pawn pawn, DamageResult result)
        //{
        //    Log.Message("After: " + dinfo + " - " + result, true);
        //}
    }


    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("Kill")]
    public static class Patch_Kill
    {
        public static void Prefix(Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit = null)
        {
            if (__instance.IsColonist)
            {
                Log.Message("__instance: " + __instance, true);
                Log.Message("__instance.IsColonist: " + __instance.IsColonist, true);
                Log.Message("dinfo.HasValue: " + dinfo.HasValue, true);
                Log.Message("dinfo.Value.Instigator?.Faction: " + dinfo.Value.Instigator?.Faction, true);
                Log.Message("Current.Game.GetComponent<StorytellerWatcher>()?.currentRaidingFaction: " + Current.Game.GetComponent<StorytellerWatcher>()?.currentRaidingFaction, true);
            }
            if (__instance.IsColonist && dinfo.HasValue && dinfo.Value.Instigator?.Faction != null 
                && dinfo.Value.Instigator.Faction == Current.Game.GetComponent<StorytellerWatcher>()?.currentRaidingFaction)
            {
                var options = Find.Storyteller.def.GetModExtension<StorytellerDefExtension>();
                if (options != null && options.storytellerThreat != null)
                {
                    Log.Message("Success!");
                    IncidentParms parms = new IncidentParms
                    {
                        target = __instance.Map,
                        faction = Current.Game.GetComponent<StorytellerWatcher>().currentRaidingFaction,
                        forced = true,
                        raidStrategy = RaidStrategyDefOf.ImmediateAttack,
                        points = StorytellerUtility.DefaultThreatPointsNow(__instance.Map) / 4f
                    };
                    var incidentDef = DefDatabase<IncidentDef>.GetNamed("VSE_Reinforcements");
                    Find.Storyteller.incidentQueue.Add(incidentDef, Find.TickManager.TicksGame + new IntRange(300, 600).RandomInRange, parms);
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
                    && options.storytellerThreat.disableThreatsAtPopulationCount >= Find.ColonistBar.Entries.Where(x => x.pawn != null 
                    && !x.pawn.Dead && x.pawn.Faction == Faction.OfPlayer).Count())
                {
                    Log.Message("TryExecute is disabled");
                    return false;
                }
            }
            Log.Message("TryExecute is enabled");
            return true;
        }

        public static void Postfix(IncidentWorker __instance, bool __result, IncidentParms parms)
        {
            if (__instance is IncidentWorker_RaidEnemy && __result && parms.faction.HostileTo(Faction.OfPlayer))
            {
                Log.Message("0 parms.faction: " + parms.faction, true);
                var options = Find.Storyteller.def.GetModExtension<StorytellerDefExtension>();
                if (options != null && options.storytellerThreat != null && __result)
                {
                    Log.Message("1 parms.faction: " + parms.faction, true);
                    var comp = Current.Game.GetComponent<StorytellerWatcher>();
                    if (comp != null)
                    {
                        Log.Message("2 parms.faction: " + parms.faction, true);
                        comp.currentRaidingFaction = parms.faction;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(IncidentWorker_RaidEnemy))]
    [HarmonyPatch("TryExecuteWorker")]
    public static class Patch_TryExecuteWorker
    {
        public static void Postfix(bool __result, IncidentParms parms)
        {
            Log.Message("0 parms.faction: " + parms.faction, true);
            var options = Find.Storyteller.def.GetModExtension<StorytellerDefExtension>();
            if (options != null && options.storytellerThreat != null && __result)
            {
                Log.Message("1 parms.faction: " + parms.faction, true);
                var comp = Current.Game.GetComponent<StorytellerWatcher>();
                if (comp != null)
                {
                    Log.Message("2 parms.faction: " + parms.faction, true);
                    comp.currentRaidingFaction = parms.faction;
                }
            }
        }
    }


    [HarmonyPatch(typeof(Lord))]
    [HarmonyPatch("Notify_PawnLost")]
    public static class Patch_Notify_PawnLost
    {
        public static void Prefix(Lord __instance, Pawn pawn, PawnLostCondition cond, DamageInfo? dinfo = null)
        {
            var options = Find.Storyteller.def.GetModExtension<StorytellerDefExtension>();
            if (options != null && options.storytellerThreat != null)
            {
                Log.Message("cond: " + cond, true);
                if (__instance.faction.HostileTo(Faction.OfPlayer))
                {
                    var gameComp = Current.Game.GetComponent<StorytellerWatcher>();

                    if (gameComp.enemiesAreOutOfTheMap == null)
                        gameComp.enemiesAreOutOfTheMap = new Dictionary<Lord, bool>();
                    if (cond == PawnLostCondition.ExitedMap)
                    {
                        gameComp.enemiesAreOutOfTheMap[__instance] = true;
                    }
                }

            }
        }
    }

    [HarmonyPatch(typeof(Lord))]
    [HarmonyPatch("Cleanup")]
    public static class Patch_Cleanup
    {
        public static void Prefix(Lord __instance)
        {
            var options = Find.Storyteller.def.GetModExtension<StorytellerDefExtension>();
            if (options != null && options.storytellerThreat != null)
            {
                var gameComp = Current.Game.GetComponent<StorytellerWatcher>();
                if (__instance.Map.IsPlayerHome && __instance.faction.HostileTo(Faction.OfPlayer) && (gameComp.enemiesAreOutOfTheMap != null && gameComp.enemiesAreOutOfTheMap.ContainsKey(__instance) 
                    && !gameComp.enemiesAreOutOfTheMap[__instance] || gameComp.enemiesAreOutOfTheMap == null || !gameComp.enemiesAreOutOfTheMap.ContainsKey(__instance)))
                {
                    Log.Message("Success");
                    IncidentParms parms = new IncidentParms
                    {
                        target = __instance.Map,
                        forced = true,
                        points = StorytellerUtility.DefaultThreatPointsNow(__instance.Map)
                    };
                    var incidentDef = DefDatabase<IncidentDef>.GetNamed(options.storytellerThreat.goodIncidents.RandomElement());
                    if (incidentDef != null)
                    {
                        Find.Storyteller.incidentQueue.Add(incidentDef, Find.TickManager.TicksGame + new IntRange(6000, 12000).RandomInRange, parms);
                    }
                }
                if (gameComp.enemiesAreOutOfTheMap != null && gameComp.enemiesAreOutOfTheMap.ContainsKey(__instance))
                {
                    gameComp.enemiesAreOutOfTheMap.Remove(__instance);
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
            if (options != null && options.storytellerThreat != null)
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
                                var gameComp = Current.Game.GetComponent<StorytellerWatcher>();
                                if (gameComp.enemiesAreOutOfTheMap == null)
                                    gameComp.enemiesAreOutOfTheMap = new Dictionary<Lord, bool>();
                                gameComp.enemiesAreOutOfTheMap[lord] = true;
                            }
                        }
                    }
                }
            }
        }
    }
}
