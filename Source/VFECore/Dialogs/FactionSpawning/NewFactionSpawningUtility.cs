using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VFECore
{
    public static class NewFactionSpawningUtility
    {
        public static Faction SpawnWithoutSettlements(FactionDef factionDef)
        {
            // Temporarily set to hidden, so FactionGenerator doesn't spawn a base
            var hidden = factionDef.hidden;
            factionDef.hidden = true;
            var faction = FactionGenerator.NewGeneratedFaction(factionDef);
            factionDef.hidden = hidden;

            var relationKind = GetFactionKind(faction, true);
            InitializeFaction(faction, relationKind);
            return faction;
        }

        private static FactionRelationKind GetFactionKind(Faction faction, bool firstOfKind)
        {
            var result = FactionRelationKind.Hostile;
            if (faction.def.CanEverBeNonHostile)
            {
                if (!firstOfKind || !faction.def.mustStartOneEnemy)
                {
                    if (faction.def.startingGoodwill.RandomInRange > 0) result = FactionRelationKind.Neutral;
                }
            }

            return result;
        }

        private static void InitializeFaction(Faction faction, FactionRelationKind kind)
        {
            faction.TrySetRelationKind(Faction.OfPlayer, kind, false);
            Find.FactionManager.Add(faction);
        }

        private static void CreateSettlements(Faction faction, int amount, int minDistance, out int spawned)
        {
            spawned = 0;
            // Try twice as many times as needed, but don't try infinitely, this is pretty slow
            var tiles = Enumerable.Range(0, amount * 2).Select(_ => TileFinder.RandomSettlementTileFor(faction)).Where(t => t != 0).Distinct().ToArray();

            var tilesInDistance = tiles.Where(IsFarEnoughFromPlayer).Take(amount).ToArray();

            // Validator
            bool IsFarEnoughFromPlayer(int tileId)
            {
                foreach (var playerSettlement in Find.WorldObjects.SettlementBases)
                {
                    if (playerSettlement.Faction != Faction.OfPlayer) continue;
                    int distance = Find.WorldGrid.TraversalDistanceBetween(tileId, playerSettlement.Tile, false, minDistance);
                    var objects = Find.WorldObjects.ObjectsAt(tileId).ToArray();
                    if(objects.Length > 0)
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

            if (amount > 0 && spawned <= 0)
            {
                RemoveFaction(faction);
            }

            return faction;
        }

        private static void RemoveFaction(Faction faction)
        {
            foreach (var f in Find.FactionManager.AllFactionsListForReading)
            {
                var relations = AccessTools.FieldRefAccess<Faction, List<FactionRelation>>(f, "relations");
                relations.RemoveAll(r => r?.other == null || r.other == faction);
            }
            Find.FactionManager.Remove(faction);
        }
    }
}
