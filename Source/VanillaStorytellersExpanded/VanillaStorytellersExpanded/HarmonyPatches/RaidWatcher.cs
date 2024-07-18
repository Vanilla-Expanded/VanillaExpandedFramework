using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
                try
                {
                    VanillaStorytellersExpanded.harmonyInstance.Patch(method, null, new HarmonyMethod(postfix));
                }
                catch (Exception)
                {
                    Log.Error("Error patching " + cur + " - " + method);
                }
            }
        }

        public static void RaidGroupChecker(List<Pawn> pawns, IncidentParms parms)
        {
            if (pawns != null && parms != null)
            {
                var gameComp = Current.Game?.GetComponent<StorytellerWatcher>();
                if (gameComp != null)
                {
                    var raidGroup = new RaidGroup();
                    if (parms.faction != null)
                    {
                        raidGroup.faction = parms.faction;
                    }
                    else
                    {
                        raidGroup.faction = pawns.First().Faction;
                    }
                    raidGroup.pawns = pawns.ToHashSet();
                    if (includeRaidToTheList)
                    {
                        if (gameComp.raidGroups is null)
                        {
                            gameComp.raidGroups = new List<RaidGroup>();
                        }
                        gameComp.raidGroups.Add(raidGroup);
                    }
                    else
                    {
                        if (gameComp.reinforcementGroups is null)
                        {
                            gameComp.reinforcementGroups = new List<RaidGroup>();
                        }
                        gameComp.reinforcementGroups.Add(raidGroup);
                    }
                }
            }

        }
    }

    [HarmonyPatch(typeof(Lord), "AddPawn")]
    public static class Patch_AddPawn
    {
        public static void Postfix(Lord __instance, Pawn p)
        {
            var gameComp = Current.Game.GetComponent<StorytellerWatcher>();
            if (gameComp.raidGroups != null)
            {
                foreach (var rg in gameComp.raidGroups)
                {
                    if (rg.pawns.Contains(p))
                    {
                        rg.lords.Add(__instance);
                    }
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
        public static bool ShouldTriggerReinforcements(Pawn victim, DamageInfo? dinfo, out Faction enemyFaction)
        {
            var gameComp = Current.Game.GetComponent<StorytellerWatcher>();
            if (dinfo.HasValue && dinfo.Value.Instigator?.Faction != null 
                && gameComp.FactionPresentInCurrentRaidGroups(dinfo.Value.Instigator.Faction))
            {
                enemyFaction = dinfo.Value.Instigator.Faction;
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
                                if (p != victim && p?.Faction != null && gameComp.FactionPresentInCurrentRaidGroups(p.Faction))
                                {
                                    enemyFaction = p.Faction;
                                    return true;
                                }
                            }
                        }
                    }
                }

                foreach (var log in Find.BattleLog.Battles)
                {
                    foreach (var entry in log.Entries)
                    {
                        if (entry.Timestamp > Find.TickManager.TicksAbs - 60000 && entry.GetConcerns().Contains(victim))
                        {
                            foreach (var p in entry.GetConcerns())
                            {
                                if (p != victim && p?.Faction != null && gameComp.FactionPresentInCurrentRaidGroups(p.Faction))
                                {
                                    enemyFaction = p.Faction;
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            enemyFaction = null;
            return false;
        }
        public static void Prefix(Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit = null)
        {
            try
            {
                var options = Find.Storyteller.def.GetModExtension<StorytellerDefExtension>();
                if (options != null && options.storytellerThreat != null && __instance.IsColonist && __instance.Faction == Faction.OfPlayer)
                {
                    if (ShouldTriggerReinforcements(__instance, dinfo, out Faction enemyFaction))
                    {
                        IncidentParms parms = new IncidentParms
                        {
                            target = __instance.Map,
                            faction = enemyFaction,
                            forced = true,
                            raidStrategy = RaidStrategyDefOf.ImmediateAttack,
                            points = StorytellerUtility.DefaultThreatPointsNow(__instance.Map) / 4f
                        };
                        var incidentDef = DefDatabase<IncidentDef>.GetNamed("VSE_Reinforcements");
                        Find.Storyteller.incidentQueue.Add(incidentDef, Find.TickManager.TicksGame + new IntRange(300, 600).RandomInRange, parms);
                    }
                }
            }
            catch (Exception) 
            {
            };
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
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Storyteller), "TryFire")]
    public static class Patch_MakeIncidentsForInterval
    {
        public static bool Prefix(FiringIncident fi)
        {
            if (fi?.def == null)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(IncidentWorker_Raid))]
    [HarmonyPatch("TryExecuteWorker")]
    public static class Patch_TryExecuteWorker
    {
        public static bool Prefix(IncidentWorker_Raid __instance, IncidentParms parms)
        {
            var options = Find.Storyteller.def.GetModExtension<StorytellerDefExtension>();
            if (options != null && options.storytellerThreat != null && options.storytellerThreat.raidWarningRange.HasValue &&
                __instance is IncidentWorker_RaidEnemy && parms.target is Map mapTarget)
            {
                var result = (bool) AccessTools.Method(typeof(IncidentWorker_RaidEnemy), "TryResolveRaidFaction", null, null).Invoke(__instance, new object[]
                {
                    parms
                });
                if (result)
                {
                    if (parms.faction != null && parms.faction.HostileTo(Faction.OfPlayer))
                    {
                        var comp = Current.Game.GetComponent<StorytellerWatcher>();
                        if (comp != null && !comp.raidQueues.Any(x => x.parms == parms))
                        {
                            var tickToFire = Find.TickManager.TicksAbs + options.storytellerThreat.raidWarningRange.Value.RandomInRange;
                            __instance.ResolveRaidStrategy(parms, PawnGroupKindDefOf.Combat);
                            __instance.ResolveRaidArriveMode(parms);
                            var raidQueue = new RaidQueue(__instance.def, parms, tickToFire);
                            comp.raidQueues.Add(raidQueue);
                            TaggedString letterLabel = "VFEMech.RaidWarningTitle".Translate(parms.faction.Named("FACTION"));
                            TaggedString letterText = "VFEMech.RaidWarningText".Translate(parms.faction.Named("FACTION"), parms.raidStrategy.arrivalTextEnemy);
                            Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.ThreatBig);
                            return false;
                        }
                    }
                }
            }

            return true;
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
                            Find.Storyteller.incidentQueue.Add(incidentDef, Find.TickManager.TicksGame + new IntRange(6000, 12000).RandomInRange, parms);
                        }
                    }
                    gameComp.raidGroups.Remove(raidGroup);
                }

                var reinforcementGroup = gameComp.reinforcementGroups.Where(x => x.lords.Contains(__instance)).FirstOrDefault();
                if (reinforcementGroup != null)
                {
                    gameComp.reinforcementGroups.Remove(raidGroup);
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
                                            Find.Storyteller.incidentQueue.Add(incidentDef, Find.TickManager.TicksGame + new IntRange(6000, 12000).RandomInRange, parms);
                                        }
                                    }
                                    gameComp.raidGroups.Remove(raidGroup);
                                }

                                var reinforcementGroup = gameComp.reinforcementGroups.Where(x => x.lords.Contains(lord)).FirstOrDefault();
                                if (reinforcementGroup != null)
                                {
                                    gameComp.reinforcementGroups.Remove(raidGroup);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
