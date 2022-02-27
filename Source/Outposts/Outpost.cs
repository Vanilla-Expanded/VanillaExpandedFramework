using System;
using System.Collections.Generic;
using System.Linq;
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
        public IEnumerable<Pawn> CapablePawns => AllPawns.Where(IsCapable);

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

        protected virtual bool IsCapable(Pawn pawn)
        {
            if (!pawn.RaceProps.Humanlike) return false;
            if (pawn.skills is null) return false;
            return !Ext.RelevantSkills.Any(skill => pawn.skills.GetSkill(skill).TotallyDisabled);
        }

        public override void PostAdd()
        {
            base.PostAdd();
            ticksTillProduction = Mathf.RoundToInt(TicksPerProduction * OutpostsMod.Settings.TimeMultiplier);
        }

        public int TotalSkill(SkillDef skill)
        {
            if (skillsDirty)
                foreach (var skillDef in DefDatabase<SkillDef>.AllDefs)
                    totalSkills[skillDef] = CapablePawns.Sum(p => p.skills.GetSkill(skill).Level);
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

            if (!Map.mapPawns.AllPawns.Where(p => p.RaceProps.Humanlike).Any(p => p.HostileTo(Faction.OfPlayer)))
            {
                occupants.Clear();
                Find.LetterStack.ReceiveLetter("Outposts.Letters.BattleWon.Label".Translate(), "Outposts.Letters.BattleWon.Text".Translate(Name), LetterDefOf.PositiveEvent,
                    new LookTargets(Gen.YieldSingle(this)));
                foreach (var pawn in Map.mapPawns.AllPawns.Where(p => p.Faction is {IsPlayer: true} || p.HostFaction is {IsPlayer: true}))
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
                        if (food.CurLevelPercentage <= pawn.RaceProps.FoodLevelPercentageWantEat && ProvidedFood is {IsNutritionGivingIngestible: true} &&
                            ProvidedFood.ingestible.HumanEdible)
                        {
                            var thing = ThingMaker.MakeThing(ProvidedFood);
                            if (thing.IngestibleNow && pawn.RaceProps.CanEverEat(thing)) food.CurLevel += thing.Ingested(pawn, food.NutritionWanted);
                        }
                    }
                }

            if (this.IsHashIntervalTick(300)) SatisfyNeeds();
        }

        public virtual void SatisfyNeeds()
        {
            foreach (var pawn in AllPawns) SatisfyNeeds(pawn);
        }

        public virtual void SatisfyNeeds(Pawn pawn)
        {
            if (pawn is null) return;
            var food = pawn.needs?.food;
            if (food is not null && food.CurLevelPercentage <= pawn.RaceProps.FoodLevelPercentageWantEat && ProvidedFood is {IsNutritionGivingIngestible: true} &&
                ProvidedFood.ingestible.HumanEdible)
            {
                var thing = ThingMaker.MakeThing(ProvidedFood);
                if (thing.IngestibleNow && pawn.RaceProps.CanEverEat(thing)) food.CurLevel += thing.Ingested(pawn, food.NutritionWanted);
            }

            if (GenLocalDate.HourInteger(Tile) >= 23 || GenLocalDate.HourInteger(Tile) <= 5) pawn.needs?.rest?.TickResting(0.75f);

            if (pawn.health is not null && pawn.health.HasHediffsNeedingTend())
            {
                var doctor = AllPawns.Where(p => p.RaceProps.Humanlike && !p.Downed).MaxBy(p => p.skills?.GetSkill(SkillDefOf.Medicine)?.Level ?? -1f);
                Medicine medicine = null;
                var potency = 0f;
                foreach (var thing in containedItems)
                    if (thing.def.IsMedicine && (pawn.playerSettings is null || pawn.playerSettings.medCare.AllowsMedicine(thing.def)))
                    {
                        var statValue = thing.GetStatValue(StatDefOf.MedicalPotency);
                        if (statValue > potency || medicine == null)
                        {
                            potency = statValue;
                            medicine = (Medicine) thing;
                        }
                    }

                TendUtility.DoTend(doctor, pawn, medicine);
            }

            if (pawn.health?.hediffSet is not null && pawn.health.hediffSet.HasNaturallyHealingInjury())
                pawn.health.hediffSet.GetHediffs<Hediff_Injury>().Where(x => x.CanHealNaturally()).RandomElement()
                    .Heal(pawn.HealthScale * 0.01f * pawn.GetStatValue(StatDefOf.InjuryHealingFactor));

            if (pawn.health?.hediffSet is not null && pawn.health.hediffSet.HasTendedAndHealingInjury())
            {
                var injury = pawn.health.hediffSet.GetHediffs<Hediff_Injury>().Where(x => x.CanHealFromTending()).RandomElement();
                injury.Heal(GenMath.LerpDouble(0f, 1f, 0.5f, 1.5f, Mathf.Clamp01(injury.TryGetComp<HediffComp_TendDuration>().tendQuality)) * pawn.HealthScale * 0.01f *
                            pawn.GetStatValue(StatDefOf.InjuryHealingFactor));
            }
        }

        public virtual IEnumerable<Thing> ProducedThings()
        {
            return ResultOptions.SelectMany(resultOption => resultOption.Make(CapablePawns.ToList()));
        }

        public virtual void Produce()
        {
            Deliver(ProducedThings());
        }

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            RecachePawnTraits();
        }

        public virtual void RecachePawnTraits()
        {
            skillsDirty = true;
        }

        public bool AddPawn(Pawn pawn)
        {
            if (!Ext.CanAddPawn(pawn, out _)) return false;
            var caravan = pawn.GetCaravan();
            if (caravan != null)
            {
                foreach (var item in CaravanInventoryUtility.AllInventoryItems(caravan).Where(item => CaravanInventoryUtility.GetOwnerOf(caravan, item) == item))
                    CaravanInventoryUtility.MoveInventoryToSomeoneElse(pawn, item, caravan.PawnsListForReading, new List<Pawn> {pawn}, item.stackCount);
                if (!caravan.PawnsListForReading.Except(pawn).Any(p => p.RaceProps.Humanlike))
                    containedItems.AddRange(CaravanInventoryUtility.AllInventoryItems(caravan));
                caravan.RemovePawn(pawn);
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

            return true;
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
                    Ext.CanAddPawn(p, out var reason)
                        ? new FloatMenuOption(p.NameFullColored.CapitalizeFirst().Resolve(), () => AddPawn(p))
                        : new FloatMenuOption(p.NameFullColored + " - " + reason, null)).ToList())),
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
            RecachePawnTraits();
            return p;
        }

        public override string GetInspectString() =>
            base.GetInspectString() +
            def.LabelCap.Line() +
            "Outposts.Contains".Translate(occupants.Count).Line() +
            "Outposts.Packing".Translate(ticksTillPacked.ToStringTicksToPeriodVerbose().Colorize(ColoredText.DateTimeColor)).Line(Packing) +
            ProductionString().Line(!Packing) +
            RelevantSkillDisplay().Line(Ext?.RelevantSkills?.Count > 0);

        public virtual string ProductionString()
        {
            var options = ResultOptions;
            if (Ext is null || options is not {Count: >0}) return "";
            return options.Count switch
            {
                1 => "Outposts.WillProduce.1".Translate(options[0].Amount(CapablePawns.ToList()), options[0].Thing.label, TimeTillProduction).RawText,
                2 => "Outposts.WillProduce.2".Translate(options[0].Amount(CapablePawns.ToList()), options[0].Thing.label, options[1].Amount(CapablePawns.ToList()),
                    options[1].Thing.label, TimeTillProduction).RawText,
                _ => "Outposts.WillProduce.N".Translate(TimeTillProduction, options.Select(ro => ro.Explain(CapablePawns.ToList())).ToLineList("  - ")).RawText
            };
        }

        public virtual string RelevantSkillDisplay() =>
            Ext.RelevantSkills.Select(skill => "Outposts.TotalSkill".Translate(skill.skillLabel, TotalSkill(skill)).RawText).ToLineList();
    }
}