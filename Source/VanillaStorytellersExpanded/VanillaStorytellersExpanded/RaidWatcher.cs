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
    [StaticConstructorOnStartup]
    public static class RaidPatches
    {
        public static bool includeRaidToTheList = true;
        static RaidPatches()
        {
            var postfix = typeof(RaidPatches).GetMethod("RaidGroupChecker");
            var baseType = typeof(PawnsArrivalModeWorker);
            var types = baseType.AllSubclassesNonAbstract();
            foreach (Type cur in types)
            {
                var method = cur.GetMethod("Arrive");
                Log.Message("Patching " + cur + " - " + method);
                try
                {
                    VanillaStorytellersExpanded.harmonyInstance.Patch(method, null, new HarmonyMethod(postfix));
                }
                catch (Exception ex)
                {
                    Log.Error("Error patching " + cur + " - " + method);
                }
            }
        }

        public static void RaidGroupChecker(List<Pawn> pawns, IncidentParms parms)
        {
            if (includeRaidToTheList && pawns != null)
            {
                Log.Message("Creating raid group of : " + pawns?.Count + " pawns");
                var gameComp = Current.Game.GetComponent<StorytellerWatcher>();
                var raidGroup = new RaidGroup();
                raidGroup.pawns = pawns.ToHashSet();
                gameComp.raidGroups.Add(raidGroup);
            }
        }
    }

    [HarmonyPatch(typeof(Lord), "AddPawn")]
    public static class Patch_AddPawn
    {
        public static void Postfix(Lord __instance, Pawn p)
        {
            var gameComp = Current.Game.GetComponent<StorytellerWatcher>();
            foreach (var rg in gameComp.raidGroups)
            {
                if (rg.pawns.Contains(p))
                {
                    rg.lords.Add(__instance);
                }
            }
        }
    }

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
                    dinfo.SetAmount(dinfo.Amount * options.storytellerThreat.allDamagesMultiplier);
                }
            }
        }
    }


    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("Kill")]
    public static class Patch_Kill
    {
        public static bool ShouldTriggerReinforcements(Pawn victim, DamageInfo? dinfo)
        {
            if (dinfo.HasValue && dinfo.Value.Instigator?.Faction != null
                    && dinfo.Value.Instigator.Faction == Current.Game.GetComponent<StorytellerWatcher>()?.currentRaidingFaction)
            {
                return true;
            }
            else if (!dinfo.HasValue)
            {
                foreach (var log in Find.BattleLog.Battles)
                {
                    foreach (var entry in log.Entries)
                    {
                        if (entry.Timestamp == Find.TickManager.TicksAbs && entry.GetConcerns().Contains(victim))
                        {
                            foreach (var p in entry.GetConcerns())
                            {
                                if (p != victim && p?.Faction != null && p.Faction == Current.Game.GetComponent<StorytellerWatcher>()?.currentRaidingFaction)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }
        public static void Prefix(Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit = null)
        {
            if (__instance.IsColonist && __instance.FactionOrExtraMiniOrHomeFaction == Faction.OfPlayer)
            {
                if (ShouldTriggerReinforcements(__instance, dinfo))
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
                        Log.Message("Colonist died! Reinforcements will arrive");
                        var incidentDef = DefDatabase<IncidentDef>.GetNamed("VSE_Reinforcements");
                        Find.Storyteller.incidentQueue.Add(incidentDef, Find.TickManager.TicksGame + new IntRange(300, 600).RandomInRange, parms);
                    }
                }
                else
                {
                    Log.Message("__instance: " + __instance, true);
                    Log.Message("__instance.IsColonist: " + __instance.IsColonist, true);
                    Log.Message("dinfo.HasValue: " + dinfo.HasValue, true);
                    if (dinfo.HasValue)
                    {
                        Log.Message("dinfo.Value.Instigator: " + dinfo.Value.Instigator, true);
                        Log.Message("dinfo.Value.Instigator?.Faction: " + dinfo.Value.Instigator?.Faction, true);
                    }
                    else
                    {
                        Log.Message("dinfo is null: " + dinfo, true);
                        if (exactCulprit != null)
                        {
                            Log.Message("exactCulprit: " + exactCulprit, true);
                            Log.Message("Log: " + exactCulprit.combatLogEntry, true);
                            Log.Message("exactCulprit.combatLogEntry.Target: " + exactCulprit.combatLogEntry?.Target, true);
                        }
                        foreach (var log in Find.BattleLog.Battles)
                        {
                            foreach (var entry in log.Entries)
                            {
                                if (entry.Timestamp == Find.TickManager.TicksAbs && entry.GetConcerns().Contains(__instance))
                                {
                                    Log.Message("Find.TickManager.TicksGame: " + Find.TickManager.TicksGame, true);
                                    Log.Message("Find.TickManager.TicksAbs: " + Find.TickManager.TicksAbs, true);
                                    Log.Message("entry.Timestamp: " + entry.Timestamp, true);
                                    Log.Message("entry: " + entry, true);
                                    if (entry is BattleLogEntry_RangedImpact logEntry)
                                    {
                                        Log.Message("logEntry: " + logEntry, true);
                                    }
                                    foreach (var p in entry.GetConcerns())
                                    {
                                        Log.Message("Pawn: " + p, true);
                                    }
                                }
                            }
                        }
                    }
                    Log.Message("Current.Game.GetComponent<StorytellerWatcher>()?.currentRaidingFaction: "
                        + Current.Game.GetComponent<StorytellerWatcher>()?.currentRaidingFaction, true);
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
                var options = Find.Storyteller.def.GetModExtension<StorytellerDefExtension>();
                if (options != null && options.storytellerThreat != null && __result)
                {
                    var comp = Current.Game.GetComponent<StorytellerWatcher>();
                    if (comp != null)
                    {
                        Log.Message(__instance.def + " fires, parms.faction: " + parms.faction, true);
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
        public static void Postfix(IncidentWorker_RaidEnemy __instance, bool __result, IncidentParms parms)
        {
            var options = Find.Storyteller.def.GetModExtension<StorytellerDefExtension>();
            if (options != null && options.storytellerThreat != null && __result)
            {
                var comp = Current.Game.GetComponent<StorytellerWatcher>();
                if (comp != null)
                {
                    Log.Message(__instance.def + " fires, parms.faction: " + parms.faction, true);
                    comp.currentRaidingFaction = parms.faction;
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
                Log.Message("gameComp.raidGroups: " + gameComp.raidGroups.Count);
                for (int i = gameComp.raidGroups.Count - 1; i >= 0; i--)
                {
                    if (gameComp.raidGroups[i].lords.Contains(__instance) && gameComp.raidGroups[i].lords.Count > 1)
                    {
                        gameComp.raidGroups[i].lords.Remove(__instance);
                        return;
                    }
                }
                var raidGroup = gameComp.raidGroups.Where(x => x.lords.Contains(__instance)).FirstOrDefault();
                if (raidGroup != null)
                {
                    if (__instance.Map.IsPlayerHome && __instance.faction.HostileTo(Faction.OfPlayer))
                    {
                        IncidentParms parms = new IncidentParms
                        {
                            target = __instance.Map,
                            forced = true,
                            points = StorytellerUtility.DefaultThreatPointsNow(__instance.Map)
                        };
                        var incidentDef = DefDatabase<IncidentDef>.GetNamed(options.storytellerThreat.goodIncidents.RandomElement());
                        if (incidentDef != null)
                        {
                            Log.Message("Success: " + incidentDef, true); ;
                            Find.Storyteller.incidentQueue.Add(incidentDef, Find.TickManager.TicksGame + new IntRange(6000, 12000).RandomInRange, parms);
                        }
                    }
                    gameComp.raidGroups.Remove(raidGroup);
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
                                var gameComp = Current.Game.GetComponent<StorytellerWatcher>();
                                Log.Message("gameComp.raidGroups: " + gameComp.raidGroups.Count);
                                for (int j = gameComp.raidGroups.Count - 1; j >= 0; j--)
                                {
                                    if (gameComp.raidGroups[j].lords.Contains(lord) && gameComp.raidGroups[j].lords.Count > 1)
                                    {
                                        gameComp.raidGroups[j].lords.Remove(lord);
                                        return;
                                    }
                                }

                                var raidGroup = gameComp.raidGroups.Where(x => x.lords.Contains(lord)).FirstOrDefault();
                                if (raidGroup != null)
                                {
                                    if (__instance.Map.IsPlayerHome && lord.faction.HostileTo(Faction.OfPlayer))
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
                                            Log.Message("Success: " + incidentDef, true); ;
                                            Find.Storyteller.incidentQueue.Add(incidentDef, Find.TickManager.TicksGame + new IntRange(6000, 12000).RandomInRange, parms);
                                        }
                                    }
                                    gameComp.raidGroups.Remove(raidGroup);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
