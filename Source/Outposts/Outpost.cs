using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KCSG;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Outposts
{
    public class Outpost : MapParent
    {
        private readonly Dictionary<SkillDef, int> totalSkills = new();

        private Material cachedMat;

        private List<Thing> containedItems = new();

        private bool costPaid;

        private OutpostExtension extensionCached;
        public string Name;
        private List<Pawn> occupants = new();
        private bool skillsDirty = true;
        private int ticksTillPacked = -1;
        private int ticksTillProduction;
        public virtual float RestPerTickResting => 0.005714286f * 2.5f;
        public IEnumerable<Pawn> AllPawns => occupants;
        public int PawnCount => occupants.Count;
        public override Color ExpandingIconColor => Faction.Color;

        public virtual int TicksPerProduction => Ext?.TicksPerProduction ?? 15 * 60000;
        public override bool HasName => !Name.NullOrEmpty();
        public override string Label => Name;
        public virtual int TicksToPack => (Ext?.TicksToPack ?? 7 * 60000) / occupants.Count;
        public bool Packing => ticksTillPacked > 0;
        public virtual int Range => Ext?.Range ?? -1;
        public IEnumerable<Thing> Things => containedItems;

        public override Material Material
        {
            get
            {
                if (cachedMat == null)
                    cachedMat = MaterialPool.MatFrom(Faction.def.settlementTexturePath, ShaderDatabase.WorldOverlayTransparentLit, Faction.Color,
                        WorldMaterials.WorldObjectRenderQueue);

                return cachedMat;
            }
        }

        public override MapGeneratorDef MapGeneratorDef
        {
            get
            {
                if (def.GetModExtension<CustomGenOption>() is { } cGen && (cGen.chooseFromlayouts.Count > 0 || cGen.chooseFromSettlements.Count > 0))
                    return DefDatabase<MapGeneratorDef>.GetNamed("KCSG_WorldObject_Gen");
                return MapGeneratorDefOf.Base_Faction;
            }
        }

        public virtual ThingDef ProvidedFood => Ext?.ProvidedFood ?? ThingDefOf.MealSimple;
        public OutpostExtension Ext => extensionCached ??= def.GetModExtension<OutpostExtension>();

        public virtual string TimeTillProduction => ticksTillProduction.ToStringTicksToPeriodVerbose().Colorize(ColoredText.DateTimeColor);

        public virtual List<ResultOption> ResultOptions => Ext.ResultOptions;

        public override void PostAdd()
        {
            base.PostAdd();
            ticksTillProduction = Mathf.RoundToInt(TicksPerProduction * OutpostsMod.Settings.TimeMultiplier);
        }

        protected static string Requirement(string req, bool passed) => $"{(passed ? "✓" : "✖")} {req}".Colorize(passed ? Color.green : Color.red);

        public static string RequirementsStringBase(OutpostExtension ext, int tileIdx, List<Pawn> pawns)
        {
            var builder = new StringBuilder();
            var biome = Find.WorldGrid[tileIdx].biome;
            if (ext.AllowedBiomes is {Count: >0})
            {
                builder.AppendLine(Requirement("Outposts.AllowedBiomes".Translate(), ext.AllowedBiomes.Contains(biome)));
                builder.AppendLine(ext.AllowedBiomes.Select(b => b.label).ToLineList("  ", true));
            }

            if (ext.DisallowedBiomes is {Count: >0})
            {
                builder.AppendLine(Requirement("Outposts.DisallowedBiomes".Translate(), !ext.DisallowedBiomes.Contains(biome)));
                builder.AppendLine(ext.DisallowedBiomes.Select(b => b.label).ToLineList("  ", true));
            }

            if (ext.MinPawns > 0) builder.AppendLine(Requirement("Outposts.NumPawns".Translate(ext.MinPawns), pawns.Count >= ext.MinPawns));

            if (ext.RequiredSkills is {Count: >0})
                foreach (var requiredSkill in ext.RequiredSkills)
                    builder.AppendLine(Requirement("Outposts.RequiredSkill".Translate(requiredSkill.Skill.skillLabel, requiredSkill.Count),
                        pawns.Sum(p => p.skills.GetSkill(requiredSkill.Skill).Level) >= requiredSkill.Count));

            if (ext.RequiresGrowing)
                builder.AppendLine(Requirement("Outposts.GrowingRequired".Translate(), GenTemperature.TwelfthsInAverageTemperatureRange(tileIdx, 6f, 42f)?.Any() ?? false));

            if (ext.CostToMake is {Count: >0})
            {
                var caravan = Find.WorldObjects.PlayerControlledCaravanAt(tileIdx);
                foreach (var tdcc in ext.CostToMake)
                    builder.AppendLine(Requirement("Outposts.MustHaveInCaravan".Translate(tdcc.Label), CaravanInventoryUtility.HasThings(caravan, tdcc.thingDef, tdcc.count)));
            }

            return builder.ToString();
        }

        public static string CanSpawnOnWithExt(OutpostExtension ext, int tileIdx, List<Pawn> pawns)
        {
            if (Find.WorldGrid[tileIdx] is {biome: var biome} && (ext.DisallowedBiomes is {Count: >0} && ext.DisallowedBiomes.Contains(biome) ||
                                                                  ext.AllowedBiomes is {Count: >0} && !ext.AllowedBiomes.Contains(biome)))
                return "Outposts.CannotBeMade".Translate(biome.label);
            if (Find.WorldObjects.AnySettlementBaseAtOrAdjacent(tileIdx) ||
                Find.WorldObjects.AllWorldObjects.OfType<Outpost>().Any(outpost => Find.WorldGrid.IsNeighborOrSame(tileIdx, outpost.Tile))) return "Outposts.TooClose".Translate();
            if (ext.MinPawns > 0 && pawns.Count < ext.MinPawns)
                return "Outposts.NotEnoughPawns".Translate(ext.MinPawns);
            if (ext.RequiredSkills is {Count: >0} &&
                ext.RequiredSkills.FirstOrDefault(requiredSkill => pawns.Sum(p => p.skills.GetSkill(requiredSkill.Skill).Level) < requiredSkill.Count) is
                    {Skill: {skillLabel: var skillLabel}, Count: var minLevel})
                return "Outposts.NotSkilledEnough".Translate(skillLabel, minLevel);
            if (ext.CostToMake is {Count: >0})
            {
                var caravan = Find.WorldObjects.PlayerControlledCaravanAt(tileIdx);
                if (ext.CostToMake.FirstOrDefault(tdcc => !CaravanInventoryUtility.HasThings(caravan, tdcc.thingDef, tdcc.count)) is {Label: var label})
                    return "Outposts.MustHaveInCaravan".Translate(label);
            }

            return null;
        }

        public int TotalSkill(SkillDef skill)
        {
            if (skillsDirty)
                foreach (var skillDef in DefDatabase<SkillDef>.AllDefs)
                    totalSkills[skillDef] = occupants.Where(p => p.def.race.Humanlike).Sum(p => p.skills.GetSkill(skill).Level);
            return totalSkills[skill];
        }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            if (Range > 0) GenDraw.DrawWorldRadiusRing(Tile, Range);
        }

        public override void PostMapGenerate()
        {
            base.PostMapGenerate();

            foreach (var pawn in Map.mapPawns.AllPawns.Where(p => p.RaceProps.Humanlike)) pawn.Destroy();

            foreach (var occupant in occupants) GenPlace.TryPlaceThing(occupant, Map.Center, Map, ThingPlaceMode.Near);
        }

        public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
        {
            if (!Map.mapPawns.FreeColonists.Any())
            {
                occupants.Clear();
                Find.LetterStack.ReceiveLetter("Outposts.Letters.Lost.Label".Translate(), "Outposts.Letters.Lost.Text".Translate(Name), LetterDefOf.NegativeEvent);
                alsoRemoveWorldObject = true;
                return true;
            }

            if (Map.mapPawns.AllPawns.Where(p => p.RaceProps.Humanlike).All(p => p.Faction is {IsPlayer: true}))
            {
                occupants.Clear();
                Find.LetterStack.ReceiveLetter("Outposts.Letters.BattleWon.Label".Translate(), "Outposts.Letters.BattleWon.Text".Translate(Name), LetterDefOf.PositiveEvent,
                    new LookTargets(Gen.YieldSingle(this)));
                foreach (var pawn in Map.mapPawns.AllPawns.Where(p => p.RaceProps.Humanlike))
                {
                    pawn.DeSpawn();
                    occupants.Add(pawn);
                }

                RecachePawnTraits();
                alsoRemoveWorldObject = false;
                return true;
            }

            alsoRemoveWorldObject = false;
            return false;
        }

        public static string CheckSkill(IEnumerable<Pawn> pawns, SkillDef skill, int minLevel)
        {
            return pawns.Sum(p => p.skills.GetSkill(skill).Level) < minLevel ? "Outposts.NotSkilledEnough".Translate(skill.skillLabel, minLevel) : null;
        }

        public static IEnumerable<Thing> MakeThings(ThingDef thingDef, int count, ThingDef stuff = null)
        {
            while (count > thingDef.stackLimit)
            {
                var temp = ThingMaker.MakeThing(thingDef, stuff);
                temp.stackCount = thingDef.stackLimit;
                yield return temp;
                count -= thingDef.stackLimit;
            }

            var temp2 = ThingMaker.MakeThing(thingDef, stuff);
            temp2.stackCount = count;
            yield return temp2;
        }

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

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref occupants, "occupants", LookMode.Deep);
            Scribe_Values.Look(ref ticksTillProduction, "ticksTillProduction");
            Scribe_Values.Look(ref Name, "name");
            Scribe_Collections.Look(ref containedItems, "containedItems", LookMode.Deep);
            Scribe_Values.Look(ref costPaid, "costPaid");
            RecachePawnTraits();
        }

        public override IEnumerable<FloatMenuOption> GetTransportPodsFloatMenuOptions(IEnumerable<IThingHolder> pods, CompLaunchable representative) =>
            base.GetTransportPodsFloatMenuOptions(pods, representative).Concat(TransportPodsArrivalAction_AddToOutpost.GetFloatMenuOptions(representative, pods, this));

        public override void Tick()
        {
            base.Tick();
            if (PawnCount == 0)
            {
                Find.LetterStack.ReceiveLetter("Outposts.Abandoned".Translate(), "Outposts.Abandoned.Desc".Translate(Name), LetterDefOf.NegativeEvent);
                Destroy();
            }

            if (Packing)
            {
                ticksTillPacked--;
                if (ticksTillPacked <= 0) ConvertToCaravan();
            }
            else if (TicksPerProduction > 0)
            {
                ticksTillProduction--;
                if (ticksTillProduction <= 0)
                {
                    ticksTillProduction = Mathf.RoundToInt(TicksPerProduction * OutpostsMod.Settings.TimeMultiplier);
                    Produce();
                }
            }

            if (Find.WorldObjects.PlayerControlledCaravanAt(Tile) is { } caravan && !caravan.pather.MovingNow)
                foreach (var pawn in caravan.PawnsListForReading)
                {
                    pawn.needs.rest.CurLevel += RestPerTickResting;
                    if (pawn.IsHashIntervalTick(300))
                    {
                        var food = pawn.needs.food;
                        if (food.CurLevelPercentage <= pawn.RaceProps.FoodLevelPercentageWantEat && ProvidedFood != null && ProvidedFood.IsNutritionGivingIngestible &&
                            ProvidedFood.ingestible.HumanEdible)
                        {
                            var thing = ThingMaker.MakeThing(ProvidedFood);
                            if (thing.IngestibleNow && pawn.RaceProps.CanEverEat(thing)) food.CurLevel += thing.Ingested(pawn, food.NutritionWanted);
                        }
                    }
                }
        }

        public virtual IEnumerable<Thing> ProducedThings()
        {
            return ResultOptions.SelectMany(resultOption => resultOption.Make(occupants));
        }

        public virtual void Produce()
        {
            Deliver(ProducedThings());
        }

        public virtual void RecachePawnTraits()
        {
            skillsDirty = true;
        }

        public void AddPawn(Pawn pawn)
        {
            var caravan = pawn.GetCaravan();
            if (caravan != null)
            {
                foreach (var item in CaravanInventoryUtility.AllInventoryItems(caravan).Where(item => CaravanInventoryUtility.GetOwnerOf(caravan, item) == item))
                    CaravanInventoryUtility.MoveInventoryToSomeoneElse(pawn, item, caravan.PawnsListForReading, new List<Pawn> {pawn}, item.stackCount);
                caravan.RemovePawn(pawn);
                containedItems.AddRange(pawn.inventory.innerContainer);
                pawn.inventory.innerContainer.Clear();
                if (!caravan.PawnsListForReading.Any(p => p.RaceProps.Humanlike))
                {
                    containedItems.AddRange(caravan.AllThings);
                    if (!costPaid && Ext.CostToMake is {Count: >0})
                    {
                        var costs = Ext.CostToMake.Select(tdcc => new ThingDefCountClass(tdcc.thingDef, tdcc.count)).ToList();
                        containedItems.RemoveAll(thing =>
                        {
                            if (costs.FirstOrDefault(tdcc => tdcc.thingDef == thing.def) is not { } cost) return false;
                            if (cost.count > thing.stackCount)
                            {
                                cost.count -= thing.stackCount;
                                return true;
                            }

                            if (cost.count < thing.stackCount)
                            {
                                thing.stackCount -= cost.count;
                                costs.Remove(cost);
                                return false;
                            }

                            costs.Remove(cost);
                            return true;
                        });
                        if (!costs.Any()) costPaid = true;
                    }

                    caravan.Destroy();
                }
            }

            pawn.holdingOwner?.Remove(pawn);

            Find.WorldPawns.RemovePawn(pawn);
            occupants.Add(pawn);
            RecachePawnTraits();
        }

        public void ConvertToCaravan()
        {
            var caravan = CaravanMaker.MakeCaravan(occupants, Faction, Tile, true);
            if (containedItems is not null)
                foreach (var item in containedItems)
                    caravan.AddPawnOrItem(item, true);
            if (Find.WorldSelector.IsSelected(this)) Find.WorldSelector.Select(caravan, false);
            Destroy();
        }

        public override IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
        {
            return base.GetCaravanGizmos(caravan).Append(new Command_Action
            {
                action = () => Find.WindowStack.Add(new FloatMenu(caravan.PawnsListForReading.Select(p =>
                    new FloatMenuOption(p.NameFullColored.CapitalizeFirst().Resolve(), () => AddPawn(p))).ToList())),
                defaultLabel = "Outposts.Commands.AddPawn.Label".Translate(),
                defaultDesc = "Outposts.Commands.AddPawn.Desc".Translate(),
                icon = TexOutposts.AddTex
            }).Append(new Command_Action
            {
                action = () => Find.WindowStack.Add(new Dialog_TakeItems(this, caravan)),
                defaultLabel = "Outposts.Commands.TakeItems.Label".Translate(),
                defaultDesc = "Outposts.Commands.TakeItems.Desc".Translate(Name),
                icon = TexOutposts.RemoveItemsTex
            });
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos()) yield return gizmo;

            if (Packing)
                yield return new Command_Action
                {
                    action = () => ticksTillPacked = -1,
                    defaultLabel = "Outposts.Commands.StopPack.Label".Translate(),
                    defaultDesc = "Outposts.Commands.StopPack.Desc".Translate(),
                    icon = TexOutposts.StopPackTex
                };
            else
                yield return new Command_Action
                {
                    action = () => ticksTillPacked = Mathf.RoundToInt(TicksToPack * OutpostsMod.Settings.TimeMultiplier),
                    defaultLabel = "Outposts.Commands.Pack.Label".Translate(),
                    defaultDesc = "Outposts.Commands.Pack.Desc".Translate(),
                    icon = TexOutposts.PackTex
                };
            yield return new Command_Action
            {
                action = () => Find.WindowStack.Add(new FloatMenu(occupants.Select(p =>
                        new FloatMenuOption(p.Name.ToStringFull.CapitalizeFirst(),
                            () => { CaravanMaker.MakeCaravan(Gen.YieldSingle(RemovePawn(p)), p.Faction, Tile, true); }))
                    .ToList())),
                defaultLabel = "Outposts.Commands.Remove.Label".Translate(),
                defaultDesc = "Outposts.Commands.Remove.Desc".Translate(),
                icon = TexOutposts.RemoveTex,
                disabled = occupants.Count == 1,
                disabledReason = "Outposts.Command.Remove.Only1".Translate()
            };

            if (Prefs.DevMode)
                yield return new Command_Action
                {
                    action = () => ticksTillProduction = 10,
                    defaultLabel = "Produce now",
                    defaultDesc = "Reduce ticksTillProduction to 10"
                };

            yield return new Command_Action
            {
                icon = TexButton.Rename,
                defaultLabel = "Rename".Translate(),
                action = () => Find.WindowStack.Add(new Dialog_RenameOutpost(this))
            };
        }

        public Pawn RemovePawn(Pawn p)
        {
            p.GetCaravan()?.RemovePawn(p);
            p.holdingOwner?.Remove(p);
            occupants.Remove(p);
            Find.WorldPawns.PassToWorld(p, PawnDiscardDecideMode.KeepForever);
            RecachePawnTraits();
            p.SetFaction(Faction);
            return p;
        }

        public override string GetInspectString() =>
            base.GetInspectString() +
            Line(def.LabelCap) +
            Line("Outposts.Contains".Translate(occupants.Count)) +
            Line(Packing ? "Outposts.Packing".Translate(ticksTillPacked.ToStringTicksToPeriodVerbose().Colorize(ColoredText.DateTimeColor)).RawText : ProductionString()) +
            Line(Ext?.RelevantSkills?.Count > 0 ? RelevantSkillDisplay() : "");

        public static string Line(string input) => input.NullOrEmpty() ? "" : "\n" + input;

        public virtual string ProductionString()
        {
            var options = ResultOptions;
            if (Ext is null || options is not {Count: >0}) return "";
            return options.Count switch
            {
                1 => "Outposts.WillProduce.1".Translate(options[0].Amount(occupants), options[0].Thing.label, TimeTillProduction).RawText,
                2 => "Outposts.WillProduce.2".Translate(options[0].Amount(occupants), options[0].Thing.label, options[1].Amount(occupants),
                    options[1].Thing.label, TimeTillProduction).RawText,
                _ => "Outposts.WillProduce.N".Translate(TimeTillProduction, options.Select(ro => ro.Explain(occupants)).ToLineList("  - ")).RawText
            };
        }

        public virtual string RelevantSkillDisplay() =>
            Ext.RelevantSkills.Select(skill => "Outposts.TotalSkill".Translate(skill.skillLabel, TotalSkill(skill)).RawText).ToLineList();
    }
}