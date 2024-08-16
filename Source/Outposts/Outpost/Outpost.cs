using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Outposts
{
    public partial class Outpost : MapParent, IRenameable
    {
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

        public Map deliveryMap;

        public virtual int TicksPerProduction => Ext?.TicksPerProduction ?? 15 * 60000;
        public override bool HasName => !Name.NullOrEmpty();
        public override string Label => Name;
        public virtual int TicksToPack => (Ext?.TicksToPack ?? 7 * 60000) / occupants.Count;
        public bool Packing => ticksTillPacked > 0;
        public virtual int Range => Ext?.Range ?? -1;
        public IEnumerable<Thing> Things => containedItems;
        public IEnumerable<Pawn> CapablePawns => AllPawns.Where(IsCapable);

        public float raidPoints;
        public Faction raidFaction;

        public string RenamableLabel
        {
            get
            {
                return Name ?? BaseLabel;
            }
            set
            {
                Name = value;
            }
        }

        public string BaseLabel => this.def.label;

        public string InspectLabel => RenamableLabel;

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

        public virtual ThingDef ProvidedFood => Ext?.ProvidedFood ?? ThingDefOf.MealSimple;
        public OutpostExtension Ext => extensionCached ??= def.GetModExtension<OutpostExtension>();

        public virtual string TimeTillProduction => ticksTillProduction.ToStringTicksToPeriodVerbose().Colorize(ColoredText.DateTimeColor);

        public virtual List<ResultOption> ResultOptions => Ext.ResultOptions;

        public void AddItem(Thing t)
        {
            containedItems.Add(t);
        }

        public Thing TakeItem(Thing t)
        {
            containedItems.Remove(t);
            return t;
        }
        public List<Thing> TakeItems(ThingDef thingDef, int stackCount)
        {
            var items = new List<Thing>();
            foreach (var item in containedItems)
            {
                if (item.def == thingDef)
                {
                    if (stackCount < item.stackCount)
                    {
                        items.Add(item.SplitOff(stackCount));
                        stackCount = 0;                        
                    }
                    else
                    {
                        stackCount = -item.stackCount;
                        items.Add(TakeItem(item));
                    }
                }
                if(stackCount == 0) { break; }
            }
            return items;
        }
        public override void PostAdd()
        {
            base.PostAdd();
            ticksTillProduction = Mathf.RoundToInt(TicksPerProduction * OutpostsMod.Settings.TimeMultiplier);
        }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            if (Range > 0) GenDraw.DrawWorldRadiusRing(Tile, Range);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref occupants, "occupants", LookMode.Deep);
            Scribe_Values.Look(ref ticksTillProduction, "ticksTillProduction");
            Scribe_Values.Look(ref Name, "name");
            Scribe_Collections.Look(ref containedItems, "containedItems", LookMode.Deep);
            Scribe_Values.Look(ref costPaid, "costPaid");
            Scribe_Values.Look(ref ticksTillPacked, "ticksTillPacked");
            Scribe_References.Look(ref raidFaction, "raidFaction");
            Scribe_Values.Look(ref raidPoints, "raidPoints");
            Scribe_References.Look(ref deliveryMap, "deliveryMap");
            try
            {
                RecachePawnTraits();
            }
            catch (Exception e)
            {
                Log.Error("Error recaching pawn traits in " + this.Label + " - " + e.ToString());
            }
        }

        public override IEnumerable<FloatMenuOption> GetTransportPodsFloatMenuOptions(IEnumerable<IThingHolder> pods, CompLaunchable representative) =>
            base.GetTransportPodsFloatMenuOptions(pods, representative)
                .Concat(TransportPodsArrivalAction_AddToOutpost.GetFloatMenuOptions(representative, pods, this));

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
                    //Adding these as in a mass caravan spam test there was dyrad with no rest need
                    if(pawn.needs?.rest == null) { continue; }
                    pawn.needs.rest.CurLevel += RestPerTickResting;
                    if (pawn.IsHashIntervalTick(300))
                    {
                        var food = pawn.needs?.food;
                        if(food == null) { continue;}
                        if (food.CurLevelPercentage <= pawn.RaceProps.FoodLevelPercentageWantEat && ProvidedFood is {IsNutritionGivingIngestible: true} &&
                            ProvidedFood.ingestible.HumanEdible)
                        {
                            var thing = ThingMaker.MakeThing(ProvidedFood);
                            if (thing.IngestibleNow && pawn.RaceProps.CanEverEat(thing)) food.CurLevel += thing.Ingested(pawn, food.NutritionWanted);
                        }
                    }
                }
            //Probably shouldnt be doing this during a raid. Fixed one bug in there, but really it just shouldnt be happening
            if (Map == null)
            {
                SatisfyNeeds();
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
            if (deliveryMap == null)
            {
                deliveryMap = Find.Maps.Where(m => m.IsPlayerHome).OrderBy(m => Find.WorldGrid.ApproxDistanceInTiles(m.Parent.Tile, Tile)).FirstOrDefault();
            }
            RecachePawnTraits();
            OutpostsMod.Notify_Spawned(this);
        }

        public virtual void RecachePawnTraits()
        {
            skillsDirty = true;
            foreach (var pawn in containedItems.OfType<Pawn>().ToList())
            {
                containedItems.Remove(pawn);
                if (pawn.GetCaravan() is { } c) c.RemovePawn(pawn);
                AddPawn(pawn);
            }
        }
        public static Type VehiclePawnType = AccessTools.TypeByName("Vehicles.VehiclePawn");
        public static MethodInfo VehicleRemoveAllPawns;
        public bool AddPawn(Pawn pawn)
        {
            if (VehiclePawnType != null && VehiclePawnType.IsAssignableFrom(pawn.GetType()))
            {
                VehicleRemoveAllPawns ??= AccessTools.Method(VehiclePawnType, "RemoveAllPawns");
                VehicleRemoveAllPawns.Invoke(pawn, null);
                return false;
            }
            if (!Ext.CanAddPawn(pawn, out _)) return false;
            var caravan = pawn.GetCaravan();
            if (caravan != null)
            {                        
                foreach (var item in CaravanInventoryUtility.AllInventoryItems(caravan)
                    .Where(item => CaravanInventoryUtility.GetOwnerOf(caravan, item) == pawn))
                {
                    CaravanInventoryUtility.MoveInventoryToSomeoneElse(pawn, item, caravan.PawnsListForReading, new List<Pawn> { pawn }, item.stackCount);                    
                }
                //Have to empty every pawns inventory items first or they will get added with the things on them. Creating duplicate load IDs/items
                //Move either fails or moves it to an animal. Neither result work
                if (!caravan.PawnsListForReading.Except(pawn).Any(p => p.RaceProps.Humanlike))
                {
                    foreach (var item in CaravanInventoryUtility.AllInventoryItems(caravan).ToList())
                    {
                        Pawn caravanPawn = CaravanInventoryUtility.GetOwnerOf(caravan, item);
                        containedItems.Add(item);
                        caravanPawn.inventory.innerContainer.Remove(item);
                    }
                }
                pawn.ownership.UnclaimAll();
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

                    if (caravan.Destroyed is false)
                    {
                        caravan.Destroy();
                    }
                }
            }

            pawn.holdingOwner?.Remove(pawn);
            if (Find.WorldPawns.Contains(pawn)) Find.WorldPawns.RemovePawn(pawn);
            if (!occupants.Contains(pawn)) occupants.Add(pawn);
            RecachePawnTraits();

            return true;
        }

        public override void PostRemove()
        {
            base.PostRemove();
            OutpostsMod.Notify_Removed(this);
        }

        public void ConvertToCaravan()
        {
            var caravan = CaravanMaker.MakeCaravan(occupants, Faction, Tile, true);
            if (containedItems is not null)
                foreach (var item in containedItems.Except(caravan.AllThings))
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
            }).Append(new Command_Action
            {
                action = () => Find.WindowStack.Add(new Dialog_GiveItems(this, caravan)),
                defaultLabel = "Outposts.Commands.GiveItems.Label".Translate(),
                defaultDesc = "Outposts.Commands.GiveItems.Desc".Translate(caravan.Name),
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
                        new FloatMenuOption((p.Name?.ToStringFull ?? p.Label).CapitalizeFirst(),
                            () => { CaravanMaker.MakeCaravan(Gen.YieldSingle(RemovePawn(p)), p.Faction, Tile, true); }))
                    .ToList())),
                defaultLabel = "Outposts.Commands.Remove.Label".Translate(),
                defaultDesc = "Outposts.Commands.Remove.Desc".Translate(),
                icon = TexOutposts.RemoveTex,
                Disabled = occupants.Count == 1,
                disabledReason = "Outposts.Command.Remove.Only1".Translate()
            };
            if (OutpostsMod.Settings.DeliveryMethod != DeliveryMethod.Store && !ProductionString().NullOrEmpty())
            {
                yield return new Command_Action
                {
                    action = () =>
                    {
                        var menuOptions = new List<FloatMenuOption>();
                        foreach (var map in Find.Maps.Where(m => m.IsPlayerHome).OrderBy(m => Find.WorldGrid.ApproxDistanceInTiles(m.Parent.Tile, Tile)))
                        {                            
                            menuOptions.Add(new FloatMenuOption(map.Parent.LabelCap, () => deliveryMap = map));
                        }
                        Find.WindowStack.Add(new FloatMenu(menuOptions));
                    },
                    defaultLabel = "Outposts.Commands.DeliveryColony.Label".Translate(),
                    defaultDesc = "Outposts.Commands.DeliveryColony.Desc".Translate(deliveryMap?.Parent.LabelCap),
                    icon = SettleUtility.SettleCommandTex
                };
            }
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
                if (Packing)
                    yield return new Command_Action
                    {
                        action = () => { ticksTillPacked = 1; },
                        defaultLabel = "Dev: Pack now",
                        defaultDesc = "Reduce ticksTillPacked to 1"
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
    }
}