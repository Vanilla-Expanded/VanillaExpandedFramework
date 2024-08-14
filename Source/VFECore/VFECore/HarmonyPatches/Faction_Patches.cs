using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;
using VFEMech;
using RimWorld.QuestGen;
using RimWorld.Planet;
using System.Reflection.Emit;
using System.Reflection;

namespace VFECore
{
    [HarmonyPatch(typeof(Building_CommsConsole), "GetCommTargets")]
    public static class Building_CommsConsole_GetCommTargets_Patch
    {
        public static IEnumerable<ICommunicable> Postfix(IEnumerable<ICommunicable> __result)
        {
            foreach (var r in __result)
            {
                if (r is Faction faction)
                {
                    var extension = faction.def.GetModExtension<FactionDefExtension>();
                    if (extension != null && extension.excludeFromCommConsole)
                    {
                        continue;
                    }
                }
                yield return r;
            }
        }
    }

    [HarmonyPatch]
    public static class Patch_GenerateRoadEndpoints
    {
        [HarmonyTargetMethod]
        public static MethodBase TargetMethod()
        {
            return typeof(WorldGenStep_Roads).GetNestedTypes(AccessTools.all)
            .SelectMany(x => x.GetMethods(AccessTools.all))
            .FirstOrDefault(x => x.Name.Contains("<GenerateRoadEndpoints>") && x.ReturnType == typeof(bool));
        }

        public static void Postfix(ref bool __result, WorldObject wo)
        {
            if (wo is Settlement settlement && settlement.Faction != null)
            {
                var extension = settlement.Faction.def.GetModExtension<FactionDefExtension>();
                if (extension != null && extension.neverConnectToRoads)
                {
                    __result = false;
                }
            }
        }
    }

    [HarmonyPatch(typeof(GenStep_Settlement), "ScatterAt")]
    public static class GenStep_Settlement_ScatterAt_Patch
    {

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
#if DEBUG
                    Log.Message("GenStep_Settlement.ScatterAt transpiler start (1 match todo)");
#endif


            var instructionList = instructions.ToList();

            var settlementGenerationSymbolInfo = AccessTools.Method(typeof(GenStep_Settlement_ScatterAt_Patch), nameof(SettlementGenerationSymbol));

            for (int i = 0; i < instructionList.Count; i++)
            {
                var instruction = instructionList[i];

                if (instruction.opcode == OpCodes.Ldstr && (string)instruction.operand == "settlement")
                {
#if DEBUG
                            Log.Message("GenStep_Settlement.ScatterAt match 1 of 1");
#endif

                    yield return instruction; // "settlement"
                    yield return new CodeInstruction(OpCodes.Ldloc_3); // faction
                    instruction = new CodeInstruction(OpCodes.Call, settlementGenerationSymbolInfo); // SettlementGenerationSymbol("settlement", faction)
                }

                yield return instruction;
            }
        }

        private static string SettlementGenerationSymbol(string original, Faction faction)
        {
            var factionDefExtension = FactionDefExtension.Get(faction.def);
            return factionDefExtension.settlementGenerationSymbol ?? original;
        }

    }

    [HarmonyPatch(typeof(SiteMakerHelper), "FactionCanOwn", new Type[] { typeof(SitePartDef), typeof(Faction), typeof(bool), typeof(Predicate<Faction>) })]
    public static class SiteMakerHelper_FactionCanOwn_Patch
    {
        public static void Postfix(ref bool __result, SitePartDef sitePart, Faction faction, bool disallowNonHostileFactions, Predicate<Faction> extraFactionValidator)
        {
            var extension = faction?.def.GetModExtension<FactionDefExtension>();
            if (extension != null && extension.excludeFromQuests)
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(QuestNode_GetFaction), "IsGoodFaction")]
    public static class QuestNode_GetFaction_IsGoodFaction_Patch
    {
        public static void Postfix(ref bool __result, Faction faction, Slate slate)
        {
            var extension = faction?.def.GetModExtension<FactionDefExtension>();
            if (extension != null && extension.excludeFromQuests)
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(QuestNode_GetPawn), "IsGoodPawn")]
    public static class QuestNode_GetPawn_IsGoodPawn_Patch
    {
        public static void Postfix(ref bool __result, Pawn pawn, Slate slate)
        {
            var extension = pawn?.Faction?.def.GetModExtension<FactionDefExtension>();
            if (extension != null && extension.excludeFromQuests)
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch]
    public static class Patch_RandomSettlementTileFor
    {
        [HarmonyTargetMethod]
        public static MethodBase TargetMethod()
        {
            return typeof(TileFinder).GetNestedTypes(AccessTools.all)
                .SelectMany(x => x.GetMethods(AccessTools.all))
                .FirstOrDefault(x => x.Name.Contains("<RandomSettlementTileFor>") && x.ReturnType == typeof(float));
        }

        public static void Postfix(ref float __result, int x)
        {
            if (RandomSettlementTileFor_Patch.factionToCheck?.def?.modExtensions != null && __result > 0)
            {
                var modExtensions = RandomSettlementTileFor_Patch.factionToCheck.def.modExtensions;
                if (modExtensions != null)
                {
                    foreach (var options in modExtensions.OfType<FactionDefExtension>())
                    {
                        Tile tile = Find.WorldGrid[x];
                        if ((options.disallowedBiomes?.Any() ?? false) && options.disallowedBiomes.Contains(tile.biome))
                        {
                            //Log.Message(RandomSettlementTileFor_Patch.factionToCheck.def + " can't settle in " + tile.biome + ", disallowed biomes: " + String.Join(", ", options.disallowedBiomes));
                            __result = 0f;
                            return;
                        }
                        else if ((options.allowedBiomes?.Any() ?? false) && !options.allowedBiomes.Contains(tile.biome))
                        {
                            //Log.Message(RandomSettlementTileFor_Patch.factionToCheck.def + " can't settle in " + tile.biome + ", allowed biomes: " + String.Join(", ", options.allowedBiomes));
                            __result = 0f;
                            return;
                        }

                        if ((options.requiredHillLevels?.Any() ?? false) && !options.requiredHillLevels.Contains(tile.hilliness))
                        {
                            //Log.Message(RandomSettlementTileFor_Patch.factionToCheck.def + " can't settle in " + tile.biome + ", allowed hill levels: " + String.Join(", ", options.requiredHillLevels));
                            __result = 0f;
                            return;
                        }

                        if (options.spawnOnCoastalTilesOnly)
                        {
                            Rot4 rot = Find.World.CoastDirectionAt(x);
                            if (!rot.IsValid)
                            {
                                //Log.Message(RandomSettlementTileFor_Patch.factionToCheck.def + " can't settle in " + tile.biome + ", only coastal tiles allowed");
                                __result = 0f;
                                return;
                            }
                        }
                        if (options.minDistanceToOtherSettlements > 0 && Find.WorldObjects.SettlementBases.Any(other =>
                            Find.WorldGrid.ApproxDistanceInTiles(other.Tile, x) < options.minDistanceToOtherSettlements))
                        {
                            __result = 0f;
                            return;
                        }
                    }
                }


                foreach (var otherSettlement in Find.WorldObjects.SettlementBases)
                {
                    modExtensions = otherSettlement?.Faction?.def?.modExtensions;
                    if (modExtensions != null)
                    {
                        foreach (var options in modExtensions.OfType<FactionDefExtension>())
                        {
                            if (options != null)
                            {
                                if (options.minDistanceToOtherSettlements > 0
                                    && Find.WorldGrid.ApproxDistanceInTiles(otherSettlement.Tile, x) < options.minDistanceToOtherSettlements)
                                {
                                    __result = 0f;
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(TileFinder), nameof(TileFinder.RandomSettlementTileFor))]
        public static class RandomSettlementTileFor_Patch
        {
            public static Faction factionToCheck;
            public static void Prefix(Faction faction)
            {
                factionToCheck = faction;
            }

            public static void Postfix()
            {
                factionToCheck = null;
            }
        }
    }

    [HarmonyPatch]
    public static class IncidentWorker_RaidEnemy_ResolveRaidStrategy_Patch
    {
        [HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "ResolveRaidStrategy")]
        [HarmonyPostfix]
        public static void Postfix(IncidentParms parms, PawnGroupKindDef groupKind)
        {
            Map map = (Map)parms.target;
            Faction fac = parms.faction;
            if (fac.def.GetModExtension<FactionDefExtension>() is FactionDefExtension ext && ext != null && ext.allowedStrategies?.Count > 0)
            {
                DefDatabase<RaidStrategyDef>.AllDefs.Where(d => d.Worker.CanUseWith(parms, groupKind) && ext.allowedStrategies.Contains(d) && d.arriveModes != null && d.arriveModes.Any(x => x.Worker.CanUseWith(parms)))
                    .TryRandomElementByWeight(d => d.Worker.SelectionWeight(map, parms.points), out RaidStrategyDef result);
                parms.raidStrategy = result;

                if (parms.raidStrategy != null)
                    return;
                Log.Error("No raid stategy found, defaulting to ImmediateAttack. Faction=" + parms.faction.def.defName + ", points=" + parms.points + ", groupKind=" + groupKind + ", parms=" + parms);
                parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
            }
        }
    }

    [HarmonyPatch(typeof(Faction), "TryMakeInitialRelationsWith")]
    public static class Faction_TryMakeInitialRelationsWith_Patch
    {
        // Custom initial relations between NPC factions should be patched last, including after HAR's own faction relation code
        [HarmonyAfter(new string[] { "rimworld.erdelf.alien_race.main" })]
        public static void Postfix(Faction __instance, Faction other)
        {
            var currentFactionDefExtension = FactionDefExtension.Get(__instance.def);
            var otherFactionDefExtension = FactionDefExtension.Get(other.def);

            var currentToOtherFactionGoodwill = currentFactionDefExtension?.startingGoodwillByFactionDefs?.Find(x => x.factionDef == other.def);
            var otherToCurrentFactionGoodwill = otherFactionDefExtension?.startingGoodwillByFactionDefs?.Find(x => x.factionDef == __instance.def);

            // If at least one of the factions references the other via custom values in the FactionDefExtension
            if (currentToOtherFactionGoodwill != null || otherToCurrentFactionGoodwill != null)
            {
                // Get the lowest range of goodwill possible between factions
                int? currentToOtherFactionGoodwillMin = currentToOtherFactionGoodwill?.Min;
                int? currentToOtherFactionGoodwillMax = currentToOtherFactionGoodwill?.Max;
                int? otherToCurrentFactionGoodwillMin = otherToCurrentFactionGoodwill?.Min;
                int? otherToCurrentFactionGoodwillMax = otherToCurrentFactionGoodwill?.Max;

                int mutualGoodwillMin = MinOfNullableInts(currentToOtherFactionGoodwillMin, otherToCurrentFactionGoodwillMin);

                int mutualGoodwillMax = MinOfNullableInts(currentToOtherFactionGoodwillMax, otherToCurrentFactionGoodwillMax);

                // Generate a random goodwill value within the range
                int finalMutualGoodWill = Rand.RangeInclusive(mutualGoodwillMin, mutualGoodwillMax);

                // Assign mutual faction relations
                FactionRelationKind kind = (finalMutualGoodWill > -10) ? ((finalMutualGoodWill < 75) ? FactionRelationKind.Neutral : FactionRelationKind.Ally) : FactionRelationKind.Hostile;

                FactionRelation factionRelation = __instance.RelationWith(other, false);
                factionRelation.baseGoodwill = finalMutualGoodWill;
                factionRelation.kind = kind;
                FactionRelation factionRelation2 = other.RelationWith(__instance, false);
                factionRelation2.baseGoodwill = finalMutualGoodWill;
                factionRelation2.kind = kind;
            }
        }

        static int MinOfNullableInts(int? num1, int? num2)
        {
            if (num1.HasValue && num2.HasValue)
            {
                return (num1 < num2) ? (int)num1 : (int)num2;
            }
            if (num1.HasValue && !num2.HasValue)
            {
                return (int)num1;
            }
            if (!num1.HasValue && num2.HasValue)
            {
                return (int)num2;
            }
            return 0;
        }
    }
}
