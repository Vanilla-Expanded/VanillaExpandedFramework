using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Outposts
{
    public partial class Outpost
    {
        public void Deliver(IEnumerable<Thing> items)
        {
            var map = Find.Maps.Where(m => m.IsPlayerHome).OrderByDescending(m => Find.WorldGrid.ApproxDistanceInTiles(m.Parent.Tile, Tile)).First();

            var text = "Outposts.Letters.Items.Text".Translate(Name) + "\n";

            var lookAt = new List<Thing>();

            var dir = Find.WorldGrid.GetRotFromTo(map.Parent.Tile, Tile);

            switch (OutpostsMod.Settings.DeliveryMethod)
            {
                case DeliveryMethod.Teleport:
                {
                    IntVec3 cell;
                    if (map.listerBuildings.AllBuildingsColonistOfDef(Outposts_DefOf.VEF_OutpostDeliverySpot).TryRandomElement(out var spot))
                        cell = spot.Position;
                    else if (!CellFinder.TryFindRandomEdgeCellWith(x =>
                            !x.Fogged(map) && x.Standable(map) && map.mapPawns.FreeColonistsSpawned.Any(p => p.CanReach(x, PathEndMode.OnCell, Danger.Some)), map,
                        dir, CellFinder.EdgeRoadChance_Always, out cell))
                        cell = CellFinder.RandomEdgeCell(dir, map);
                    foreach (var item in items)
                    {
                        GenPlace.TryPlaceThing(item, cell, map, ThingPlaceMode.Near, (t, i) => lookAt.Add(t));
                        text += "  - " + item.LabelCap + "\n";
                    }

                    break;
                }
                case DeliveryMethod.PackAnimal:
                    Deliver_PackAnimal(items, map, dir, lookAt, ref text);
                    break;
                case DeliveryMethod.Store:
                    foreach (var item in items)
                    {
                        containedItems.Add(item);
                        text += "  - " + item.LabelCap + "\n";
                    }

                    break;
                case DeliveryMethod.ForcePods:
                    Deliver_Pods(items, map, lookAt, ref text);
                    break;
                case DeliveryMethod.PackOrPods:
                    if (Outposts_DefOf.TransportPod.IsFinished)
                        Deliver_Pods(items, map, lookAt, ref text);
                    else
                        Deliver_PackAnimal(items, map, dir, lookAt, ref text);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Find.LetterStack.ReceiveLetter("Outposts.Letters.Items.Label".Translate(Name), text, LetterDefOf.PositiveEvent, new LookTargets(lookAt));
        }

        private static void Deliver_Pods(IEnumerable<Thing> items, Map map, List<Thing> lookAt, ref TaggedString text)
        {
            IntVec3 cell;
            if (map.listerBuildings.AllBuildingsColonistOfDef(Outposts_DefOf.VEF_OutpostDeliverySpot).TryRandomElement(out var spot))
            {
                lookAt.Add(spot);
                cell = spot.Position;
            }
            else
                cell = DropCellFinder.TradeDropSpot(map);

            foreach (var item in items)
            {
                if (!DropCellFinder.TryFindDropSpotNear(cell, map, out var loc, false, false, false))
                    loc = DropCellFinder.RandomDropSpot(map);
                TradeUtility.SpawnDropPod(loc, map, item);
                text += "  - " + item.LabelCap + "\n";
            }
        }

        private void Deliver_PackAnimal(IEnumerable<Thing> items, Map map, Rot4 dir, List<Thing> lookAt, ref TaggedString text)
        {
            if (!Biome.AllWildAnimals.Where(x => x.RaceProps.packAnimal).TryRandomElement(out var pawnKind)) pawnKind = PawnKindDefOf.Muffalo;

            if (!CellFinder.TryFindRandomEdgeCellWith(x => !x.Fogged(map) && x.Standable(map), map,
                dir, CellFinder.EdgeRoadChance_Always, out var cell) && !RCellFinder.TryFindRandomPawnEntryCell(out cell, map, CellFinder.EdgeRoadChance_Always))
                cell = CellFinder.RandomEdgeCell(dir, map);

            var animal = PawnGenerator.GeneratePawn(pawnKind, Faction.OfPlayer);

            lookAt.Add(animal);

            foreach (var item in items)
            {
                animal.inventory.TryAddItemNotForSale(item);
                text += "  - " + item.LabelCap + "\n";
            }

            GenSpawn.Spawn(animal, cell, map);

            IntVec3 deliverTo;
            if (
                map.listerBuildings.AllBuildingsColonistOfDef(Outposts_DefOf.VEF_OutpostDeliverySpot).TryRandomElement(out var spot))
                deliverTo = spot.Position;
            else if (!RCellFinder.TryFindRandomSpotJustOutsideColony(animal, out deliverTo))
                deliverTo = CellFinderLoose.RandomCellWith(x =>
                    !x.Fogged(map) && x.Standable(map) && animal.CanReach(x, PathEndMode.OnCell, Danger.Deadly), map);

            LordMaker.MakeNewLord(Faction.OfPlayer, new LordJob_Deliver(deliverTo), map, new[] {animal});
        }
    }
}