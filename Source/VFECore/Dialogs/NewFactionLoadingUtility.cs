using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VFECore
{
    public static class NewFactionLoadingUtility
    {
        public static void SpawnWithoutSettlements(FactionDef factionDef)
        {
            var faction = FactionGenerator.NewGeneratedFaction(factionDef);
            var relationKind = GetFactionKind(faction, true);
            InitializeFaction(faction, relationKind);
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

        private static void CreateSettlements(Faction faction, int amount)
        {
            int count = 0;
            for (int k = 0; k < amount; k++)
            {
                var tile = TileFinder.RandomSettlementTileFor(faction);
                if (tile == 0) continue;

                var factionBase = (Settlement) WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                factionBase.SetFaction(faction);
                factionBase.Tile = tile;
                factionBase.Name = SettlementNameGenerator.GenerateSettlementName(factionBase);
                Find.WorldObjects.Add(factionBase);
                count++;
            }
            Log.Message("Created " + count + " settlements for " + faction.def.LabelCap);
        }


    }
}
