using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using KCSG;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VEF.Factions
{
    public static class NewFactionSpawningUtility
    {
        private const float newFactionSettlementFactor = 0.7f; // recommendation
        private const float settlementsPer100KTiles = 80; // average

        public static Faction SpawnWithoutSettlements(FactionDef factionDef)
        {
            // Temporarily set to hidden, so FactionGenerator doesn't spawn a base
            var hidden = factionDef.hidden;
            factionDef.hidden = true;
            var factionGeneratorParms = new FactionGeneratorParms(factionDef);
            var faction               = FactionGenerator.NewGeneratedFaction(factionGeneratorParms);
            factionDef.hidden = hidden;

            var relationKind = GetFactionKind(faction, true);
            InitializeFaction(faction, relationKind);
            return faction;
        }

        private static FactionRelationKind GetFactionKind(Faction faction, bool firstOfKind)
        {
            var result = FactionRelationKind.Hostile;
            if (faction.def.CanEverBeNonHostile)
                if (!firstOfKind || !faction.def.mustStartOneEnemy)
                    if (faction.NaturalGoodwill > 0)
                        result = FactionRelationKind.Neutral;

            return result;
        }

        private static void InitializeFaction(Faction faction, FactionRelationKind kind)
        {
            if (ScenPartUtility.startingGoodwillRangeCache.ContainsKey(faction.def))
            {
                var range = ScenPartUtility.startingGoodwillRangeCache.TryGetValue(faction.def);
                faction.TryAffectGoodwillWith(Faction.OfPlayer, range.RandomInRange - faction.PlayerGoodwill, false, false);
            }

            Find.FactionManager.Add(faction);
        }

        private static void CreateSettlements(Faction faction, int amount, int minDistance, out int spawned)
        {
            spawned = 0;
            // Try twice as many times as needed, but don't try infinitely, this is pretty slow
            var tiles = Enumerable.Range(0, amount * 2).Select(_ => TileFinder.RandomSettlementTileFor(faction)).Where(t => t != 0).Distinct().ToArray();

            var tilesInDistance = tiles.Where(IsFarEnoughFromPlayer).Take(amount).ToArray();

            // Validator
            bool IsFarEnoughFromPlayer(PlanetTile tileId)
            {
                foreach (var playerSettlement in Find.WorldObjects.SettlementBases)
                {
                    if (playerSettlement.Faction != Faction.OfPlayer) continue;
                    var distance = Find.WorldGrid.TraversalDistanceBetween(tileId, playerSettlement.Tile, false, minDistance);
                    var objects  = Find.WorldObjects.ObjectsAt(tileId).ToArray();
                    if (objects.Length > 0)
                        Log.Message($"Tile: {tileId} has {objects.Select(o => o.Label).ToCommaList(true)}.");
                    if (distance < minDistance) return false;
                }

                return true;
            }

            foreach (var tile in tilesInDistance)
            {
                // Spawn base
                var factionBase = (Settlement) WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                factionBase.SetFaction(faction);
                factionBase.Tile = tile;
                factionBase.Name = SettlementNameGenerator.GenerateSettlementName(factionBase);
                Find.WorldObjects.Add(factionBase);
                spawned++;
            }
        }

        public static Faction SpawnWithSettlements(FactionDef factionDef, int amount, int minDistance, out int spawned)
        {
            var faction = SpawnWithoutSettlements(factionDef);
            CreateSettlements(faction, amount, minDistance, out spawned);

            if (amount > 0 && spawned <= 0) RemoveFaction(faction);

            return faction;
        }

        private static void RemoveFaction(Faction faction)
        {
            foreach (var f in Find.FactionManager.AllFactionsListForReading)
            {
                var relations = AccessTools.FieldRefAccess<Faction, List<FactionRelation>>(f, "relations");
                relations.RemoveAll(r => r?.other == null || r.other == faction);
            }

            Log.Message($"Marking faction {faction.Name} as hidden.");
            faction.defeated = true;
            //faction.hidden = true;
        }

        public static bool NeverSpawn(FactionDef faction)
        {
            switch (faction.defName)
            {
                case "PColony": return true; // Empire mod's player faction
                default:        return false;
            }
        }

        public static int GetRecommendedSettlementCount(float settlementCountFactor = 1f)
        {
            var existingFactions = Find.FactionManager.AllFactionsVisible.Count();
            return Mathf.Max(1, GenMath.RoundRandom(Find.WorldGrid.TilesCount / 100000f * settlementsPer100KTiles / existingFactions * newFactionSettlementFactor * settlementCountFactor));
        }

        public static void SpawnFactions(List<(FactionDef, ForcedFactionData)> forcedFactions)
        {
            var allFactions = Find.FactionManager.AllFactionsListForReading;

            foreach (var (factionDef, data) in forcedFactions)
            {
                var currentFactionCount = allFactions.Count(x => x.def == factionDef);
                var maxFactionCount = Mathf.Min(data.requiredFactionCountDuringGameplay, factionDef.maxConfigurableAtWorldCreation);
                var noSettlements = factionDef.hidden || factionDef.GetModExtension<CustomGenOption>()?.canSpawnSettlements == false;

                for (var i = currentFactionCount; i < maxFactionCount; i++)
                {
                    if (noSettlements)
                    {
                        SpawnWithoutSettlements(factionDef);
                    }
                    else
                    {
                        var distance = data.factionDiscoveryMinimumDistanceFromPlayer;
                        if (distance <= 0f) distance = SettlementProximityGoodwillUtility.MaxDist;
                        // The settlement count cannot be more than 4 times the recommended count
                        var settlementCount = Mathf.Min(4 * GetRecommendedSettlementCount(), GetRecommendedSettlementCount(data.factionDiscoveryFactionCountFactor));
                        SpawnWithSettlements(factionDef, settlementCount, distance, out _);
                    }
                }
            }
        }
    }
}