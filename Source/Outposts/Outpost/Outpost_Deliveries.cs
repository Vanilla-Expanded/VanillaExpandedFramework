using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Outposts;

public partial class Outpost
{
    public void Deliver(IEnumerable<Thing> items)
    {
        var things = items.ToList();
        var map = deliveryMap ?? Find.Maps.Where(m => m.IsPlayerHome).OrderBy(m => Find.WorldGrid.ApproxDistanceInTiles(m.Parent.Tile, Tile)).FirstOrDefault();
        if (map == null) //chance of this is super low, but it's possible for those dumb enough to play nomads
        {
            Log.Warning("Vanilla Outpost Expanded Tried to deliver to a null map, storing instead");
            foreach (var item in things) containedItems.Add(item);
            return;
        }

        var text = "Outposts.Letters.Items.Text".Translate(Name) + "\n";
        var counts = new List<ThingDefCountClass>();

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
                                 !x.Fogged(map) && x.Standable(map) &&
                                 map.mapPawns.FreeColonistsSpawned.Any(p => p.CanReach(x, PathEndMode.OnCell, Danger.Some)), map,
                             dir, CellFinder.EdgeRoadChance_Always, out cell))
                    cell = CellFinder.RandomEdgeCell(dir, map);
                foreach (var item in things) GenPlace.TryPlaceThing(item, cell, map, ThingPlaceMode.Near, (t, i) => lookAt.Add(t));

                break;
            }
            case DeliveryMethod.PackAnimal:
                Deliver_PackAnimal(things, map, dir, lookAt);
                break;
            case DeliveryMethod.Store:
                foreach (var item in things) containedItems.Add(item);
                break;
            case DeliveryMethod.ForcePods:
                Deliver_Pods(things, map, lookAt);
                break;
            case DeliveryMethod.PackOrPods:
                if (Outposts_DefOf.TransportPod.IsFinished)
                    Deliver_Pods(things, map, lookAt);
                else
                    Deliver_PackAnimal(things, map, dir, lookAt);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        var singles = new List<Thing>();
        foreach (var item in things)
        {
            if (item.def.MadeFromStuff || (item.def.useHitPoints && item.HitPoints < item.MaxHitPoints) || item.TryGetQuality(out _))
            {
                singles.Add(item);
                continue;
            }

            var count = counts.Find(cc => cc.thingDef == item.def);
            if (count is null)
            {
                count = new ThingDefCountClass { thingDef = item.def, count = 0 };
                counts.Add(count);
            }

            count.count += item.stackCount;
        }

        foreach (var single in singles) text += "  - " + single.LabelCap + "\n";
        foreach (var count in counts) text += "  - " + count.Summary + "\n";

        Find.LetterStack.ReceiveLetter("Outposts.Letters.Items.Label".Translate(Name), text, LetterDefOf.PositiveEvent, new LookTargets(lookAt));
    }

    private static void Deliver_Pods(IEnumerable<Thing> items, Map map, List<Thing> lookAt)
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
        }
    }

    private void Deliver_PackAnimal(IEnumerable<Thing> items, Map map, Rot4 dir, List<Thing> lookAt)
    {
        if (!Biome.AllWildAnimals.Where(x => x.RaceProps.packAnimal).TryRandomElement(out var pawnKind)) pawnKind = PawnKindDefOf.Muffalo;

        if (!CellFinder.TryFindRandomEdgeCellWith(x => !x.Fogged(map) && x.Standable(map), map,
                dir, CellFinder.EdgeRoadChance_Always, out var cell) &&
            !RCellFinder.TryFindRandomPawnEntryCell(out cell, map, CellFinder.EdgeRoadChance_Always))
            cell = CellFinder.RandomEdgeCell(dir, map);

        var animal = PawnGenerator.GeneratePawn(pawnKind, Faction.OfPlayer);

        lookAt.Add(animal);

        foreach (var item in items) animal.inventory.TryAddItemNotForSale(item);

        GenSpawn.Spawn(animal, cell, map);

        IntVec3 deliverTo;
        if (
            map.listerBuildings.AllBuildingsColonistOfDef(Outposts_DefOf.VEF_OutpostDeliverySpot).TryRandomElement(out var spot))
            deliverTo = spot.Position;
        else if (!RCellFinder.TryFindRandomSpotJustOutsideColony(animal, out deliverTo))
            deliverTo = CellFinderLoose.RandomCellWith(x =>
                !x.Fogged(map) && x.Standable(map) && animal.CanReach(x, PathEndMode.OnCell, Danger.Deadly), map);

        LordMaker.MakeNewLord(Faction.OfPlayer, new LordJob_Deliver(deliverTo), map, new[] { animal });
    }
}