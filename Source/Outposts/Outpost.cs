using System.Collections.Generic;
using System.Linq;
using KCSG;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Outposts
{
    [StaticConstructorOnStartup]
    public class Outpost : MapParent
    {
        public static readonly Texture2D PackTex = ContentFinder<Texture2D>.Get("UI/Gizmo/AbandonOutpost");
        public static readonly Texture2D AddTex = ContentFinder<Texture2D>.Get("UI/Gizmo/AddToOutpost");
        public static readonly Texture2D RemoveTex = ContentFinder<Texture2D>.Get("UI/Gizmo/RemovePawnFromOutpost");
        private readonly Dictionary<SkillDef, int> totalSkills = new();

        private Material cachedMat;

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
            ticksTillProduction = TicksPerProduction;
        }

        public static string CanSpawnOnWithExt(OutpostExtension ext, int tileIdx, List<Pawn> pawns)
        {
            GenDebug.LogList(pawns);
            if (Find.WorldGrid[tileIdx] is {biome: var biome} && (ext.DisallowedBiomes is {Count: >0} && ext.DisallowedBiomes.Contains(biome) ||
                                                                  ext.AllowedBiomes is {Count: >0} && !ext.AllowedBiomes.Contains(biome)))
                return "Outposts.CannotBeMade".Translate(biome.label);
            if (Find.WorldObjects.AnySettlementBaseAtOrAdjacent(tileIdx) ||
                Find.WorldObjects.AllWorldObjects.OfType<Outpost>().Any(outpost => Find.WorldGrid.IsNeighborOrSame(tileIdx, outpost.Tile))) return "Outposts.TooClose".Translate();
            if (ext.MinPawns > 0 && pawns.Count < ext.MinPawns)
                return "Outposts.NotEnoughPawns".Translate(ext.MinPawns);
            if (ext.RequiredSkills is {Count: >0} &&
                ext.RequiredSkills.FirstOrDefault(requiredSkill => pawns.Sum(p => p.skills.GetSkill(requiredSkill.Skill).Level) < requiredSkill.Count) is
                    {Skill: {skillLabel: var label}, Count: var minLevel})
                return "Outposts.NotSkilledEnough".Translate(label, minLevel);
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
            count = Mathf.RoundToInt(count * OutpostsMod.Settings.ProductionMultiplier);
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
            var dir = Find.WorldGrid.GetRotFromTo(map.Parent.Tile, Tile);
            IntVec3 cell;
            if (map.listerBuildings.AllBuildingsColonistOfDef(Outposts_DefOf.VEF_OutpostDeliverySpot).TryRandomElement(out var spot))
                cell = spot.Position;
            else if (!CellFinder.TryFindRandomEdgeCellWith(x =>
                    !x.Fogged(map) && x.Standable(map) && map.mapPawns.FreeColonistsSpawned.Any(p => p.CanReach(x, PathEndMode.OnCell, Danger.Some)), map,
                dir, CellFinder.EdgeRoadChance_Always, out cell))
                cell = CellFinder.RandomEdgeCell(dir, map);

            var text = "Outposts.Letters.Items.Text".Translate(Name) + "\n";

            var things = new List<Thing>();

            foreach (var item in items)
            {
                GenPlace.TryPlaceThing(item, cell, map, ThingPlaceMode.Near, (t, i) => things.Add(t));
                text += "  - " + item.LabelCap + "\n";
            }

            Find.LetterStack.ReceiveLetter("Outposts.Letters.Items.Label".Translate(Name), text, LetterDefOf.PositiveEvent, new LookTargets(things));
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref occupants, "occupants", LookMode.Deep);
            Scribe_Values.Look(ref ticksTillProduction, "ticksTillProduction");
            Scribe_Values.Look(ref Name, "name");
            RecachePawnTraits();
        }

        public override IEnumerable<FloatMenuOption> GetTransportPodsFloatMenuOptions(IEnumerable<IThingHolder> pods, CompLaunchable representative) =>
            base.GetTransportPodsFloatMenuOptions(pods, representative).Concat(TransportPodsArrivalAction_AddToOutpost.GetFloatMenuOptions(representative, pods, this));

        public override void Tick()
        {
            base.Tick();
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
                if (!caravan.PawnsListForReading.Any()) caravan.Destroy();
            }

            pawn.holdingOwner?.Remove(pawn);

            Find.WorldPawns.RemovePawn(pawn);
            occupants.Add(pawn);
            RecachePawnTraits();
        }

        public void ConvertToCaravan()
        {
            CaravanMaker.MakeCaravan(occupants, Faction, Tile, true);
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
                icon = AddTex
            });
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos()) yield return gizmo;

            yield return new Command_Action
            {
                action = () => ticksTillPacked = Mathf.RoundToInt(TicksToPack * OutpostsMod.Settings.TimeMultiplier),
                defaultLabel = "Outposts.Commands.Pack.Label".Translate(),
                defaultDesc = "Outposts.Commands.Pack.Desc".Translate(),
                icon = PackTex,
                disabled = Packing,
                disabledReason = "Outposts.AlreadyPacking".Translate()
            };
            yield return new Command_Action
            {
                action = () => Find.WindowStack.Add(new FloatMenu(occupants.Select(p =>
                        new FloatMenuOption(p.NameFullColored.CapitalizeFirst().Resolve(),
                            () => { CaravanMaker.MakeCaravan(Gen.YieldSingle(RemovePawn(p)), p.Faction, Tile, true); }))
                    .ToList())),
                defaultLabel = "Outposts.Commands.Remove.Label".Translate(),
                defaultDesc = "Outposts.Commands.Remove.Desc".Translate(),
                icon = RemoveTex,
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
        }

        public Pawn RemovePawn(Pawn p)
        {
            occupants.Remove(p);
            RecachePawnTraits();
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
            if (Ext is null || ResultOptions is null || ResultOptions.Count == 0) return "";
            return ResultOptions.Count switch
            {
                1 => "Outposts.WillProduce.1".Translate(ResultOptions[0].Amount(occupants), ResultOptions[0].Thing.label, TimeTillProduction),
                2 => "Outposts.WillProduce.2".Translate(ResultOptions[0].Amount(occupants), ResultOptions[0].Thing.label, ResultOptions[1].Amount(occupants),
                    ResultOptions[1].Thing.label, TimeTillProduction),
                _ => "Outposts.WillProduce.N".Translate(TimeTillProduction, ResultOptions.Select(ro => ro.Explain(occupants)).ToLineList("  - "))
            };
        }

        public virtual string RelevantSkillDisplay()
        {
            return Ext.RelevantSkills.Select(skill => "Outposts.TotalSkill".Translate(skill.skillLabel, TotalSkill(skill)).RawText).ToLineList();
        }

        public class TransportPodsArrivalAction_AddToOutpost : TransportPodsArrivalAction
        {
            private Outpost outpost;

            public TransportPodsArrivalAction_AddToOutpost()
            {
            }

            public TransportPodsArrivalAction_AddToOutpost(Outpost addTo) => outpost = addTo;

            public override void Arrived(List<ActiveDropPodInfo> pods, int tile)
            {
                var pawns = new List<Pawn>();
                foreach (var p in pods.SelectMany(pod => pod.innerContainer).OfType<Pawn>())
                {
                    pawns.Add(p);
                    Messages.Message("Outposts.AddedFromTransportPods".Translate(p.LabelShortCap, outpost.LabelCap), outpost, MessageTypeDefOf.TaskCompletion);
                }

                foreach (var pawn in pawns) outpost.AddPawn(pawn);
            }

            public override void ExposeData()
            {
                base.ExposeData();
                Scribe_References.Look(ref outpost, "outpost");
            }

            public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, int destinationTile) => outpost.Tile == destinationTile;

            public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(CompLaunchable representative, IEnumerable<IThingHolder> pods, Outpost outpost)
            {
                return TransportPodsArrivalActionUtility.GetFloatMenuOptions(
                    () => true, () => new TransportPodsArrivalAction_AddToOutpost(outpost),
                    "Outposts.AddTo".Translate(outpost.LabelCap), representative, outpost.Tile, launch =>
                    {
                        if (pods.SelectMany(pod => pod.GetDirectlyHeldThings()).Any(t => t is not Pawn))
                            Dialog_MessageBox.CreateConfirmation("Outposts.SendNonPawns".Translate(), launch);
                        else launch();
                    });
            }
        }
    }
}