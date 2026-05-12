using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace VEF.Storyteller
{
    public class QuestNode_Site : QuestNode
    {
        public SlateRef<SitePartDef> sitePartDef;
        public SlateRef<IntRange> distanceRange;

        public virtual Predicate<Map, PlanetTile> TileValidator =>
            distanceRange == null ? null : (Map map, PlanetTile tile) =>
            {
                if (map == null)
                {
                    return true;
                }

                var range = distanceRange.GetValue(QuestGen.slate);
                var dist = Find.WorldGrid.ApproxDistanceInTiles(tile, map.Tile);
                return dist >= range.min && dist <= range.max;
            };

        public virtual List<BiomeDef> AllowedBiomes { get; }

        protected bool TryFindSiteTile(out PlanetTile tile)
        {
            tile = PlanetTile.Invalid;
            Map map = QuestGen_Get.GetMap(mustBeInfestable: false, null, canBeSpace: true);
            if (map == null)
            {
                return false;
            }

            var allowedBiomes = AllowedBiomes;
            if (allowedBiomes != null && !Find.WorldGrid.Tiles.Any(t => allowedBiomes.Contains(t.PrimaryBiome)))
                allowedBiomes = null;

            var slate = QuestGen.slate;
            var range = distanceRange.GetValue(slate);

            FastTileFinder.TileQueryParams normal = new FastTileFinder.TileQueryParams(
                map.Tile,
                range.min,
                range.max,
                FastTileFinder.LandmarkMode.Any,
                reachable: true,
                Hilliness.Undefined,
                Hilliness.Undefined,
                checkBiome: true,
                validSettlement: false
            );

            FastTileFinder.TileQueryParams desperate = new FastTileFinder.TileQueryParams(
                map.Tile,
                0f,
                float.MaxValue,
                FastTileFinder.LandmarkMode.Any,
                reachable: true,
                Hilliness.Undefined,
                Hilliness.Undefined,
                checkBiome: false,
                validSettlement: false
            );

            List<PlanetTile> results = Find.WorldGrid.Surface.FastTileFinder.Query(normal, allowedBiomes, null, desperate);

            if (!results.Empty())
            {
                tile = results.RandomElement();
                return true;
            }

            tile = TileFinder.RandomSettlementTileFor(Find.WorldGrid.Surface, null);
            return tile.Valid;
        }

        public static bool IsValidTile(PlanetTile tile, List<BiomeDef> allowedBiomes = null)
        {
            Tile tile2 = tile.Tile;
            if (!tile2.PrimaryBiome.canBuildBase || !tile2.PrimaryBiome.implemented || tile2.hilliness == Hilliness.Impassable)
            {
                return false;
            }

            if (Find.WorldObjects.AnyMapParentAt(tile) || Current.Game.FindMap(tile) != null || Find.WorldObjects.AnyWorldObjectOfDefAt(WorldObjectDefOf.AbandonedSettlement, tile))
            {
                return false;
            }

            if (allowedBiomes != null && allowedBiomes.Count > 0 && !allowedBiomes.Contains(tile2.PrimaryBiome))
            {
                return false;
            }

            return true;
        }

        protected override bool TestRunInt(Slate slate)
        {
            return true;
        }

        protected Site GenerateSite(float points, PlanetTile tile, 
            Faction parentFaction, Slate slate, out string siteMapGeneratedSignal, 
            out string siteMapRemovedSignal, bool failWhenMapRemoved = true, 
            int timeoutTicks = 0)
        {
            SitePartParams parms = new SitePartParams
            {
                points = points,
                threatPoints = points
            };
            Site site = QuestGen_Sites.GenerateSite(new List<SitePartDefWithParams>
            {
                new SitePartDefWithParams(sitePartDef.GetValue(slate), parms)
            }, tile, parentFaction);
            site.doorsAlwaysOpenForPlayerPawns = true;
            if (parentFaction != null && site.Faction != parentFaction)
            {
                site.SetFaction(parentFaction);
            }

            QuestGen.slate.Set("site", site);
            QuestGen.quest.SpawnWorldObject(site);
            if (timeoutTicks > 0)
            {
                QuestGen.quest.WorldObjectTimeout(site, timeoutTicks);
            }

            siteMapRemovedSignal = QuestGenUtility.HardcodedSignalWithQuestID("site.MapRemoved");
            siteMapGeneratedSignal = QuestGenUtility.HardcodedSignalWithQuestID("site.MapGenerated");
            if (failWhenMapRemoved)
            {
                QuestGen.quest.SignalPassActivable(delegate
                {
                    QuestGen.quest.End(QuestEndOutcome.Fail, 0, null, null, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
                }, siteMapGeneratedSignal, siteMapRemovedSignal);
            }

            return site;
        }

        protected bool PrepareQuest(out Map map, out float points, out PlanetTile tile, out Slate slate)
        {
            slate = QuestGen.slate;
            points = slate.Get("points", 0f);
            map = QuestGen_Get.GetMap();
            if (!TryFindSiteTile(out tile))
            {
                return false;
            }

            slate.Set("playerFaction", Faction.OfPlayer);
            slate.Set("map", map);
            QuestGenUtility.RunAdjustPointsForDistantFight();
            return true;
        }

        protected override void RunInt()
        {

        }
    }
}
