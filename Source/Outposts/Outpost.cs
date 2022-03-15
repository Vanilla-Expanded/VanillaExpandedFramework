using System.Collections.Generic;
using System.Linq;
using KCSG;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Outposts
{
    public partial class Outpost : MapParent
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

        public bool Has(Pawn pawn) => occupants.Contains(pawn);

        public void AddItem(Thing t)
        {
            containedItems.Add(t);
        }

        public Thing TakeItem(Thing t)
        {
            containedItems.Remove(t);
            return t;
        }

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

            SatisfyNeeds();
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
            {
                yield return new Command_Action
                {
                    action = () => ticksTillProduction = 10,
                    defaultLabel = "Dev: Produce now",
                    defaultDesc = "Reduce ticksTillProduction to 10"
                };
                yield return new Command_Action
                {
                    action = () =>
                    {
                        var dinfo = new DamageInfo(DamageDefOf.Crush, 10f);
                        dinfo.SetIgnoreInstantKillProtection(true);
                        occupants.RandomElement().TakeDamage(dinfo);
                    },
                    defaultLabel = "Dev: Random pawn takes 10 damage"
                };
                yield return new Command_Action
                {
                    action = () =>
                    {
                        foreach (var pawn in occupants) pawn.needs.food.CurLevel = 0f;
                    },
                    defaultLabel = "Dev: All pawns 0% food"
                };
            }

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