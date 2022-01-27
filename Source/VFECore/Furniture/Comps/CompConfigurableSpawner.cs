using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using System.Linq;

namespace VanillaFurnitureExpanded
{
    public class CompConfigurableSpawner : ThingComp
    {
        
        private List<IntVec3> cachedAdjCellsCardinal;

        public ConfigurableSpawnerDef currentThingList = null;

        public CompProperties_ConfigurableSpawner PropsSpawner
        {
            get
            {
                return (CompProperties_ConfigurableSpawner)this.props;
            }
        }

        private bool PowerOn
        {
            get
            {
                CompPowerTrader comp = this.parent.GetComp<CompPowerTrader>();
                return comp != null && comp.PowerOn;
            }
        }

        private bool FuelOn
        {
            get
            {
                CompRefuelable comp = this.parent.GetComp<CompRefuelable>();
                return comp != null && comp.HasFuel;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (!respawningAfterLoad)
            {
                this.ResetCountdown();
            }
        }

        public override void CompTick()
        {
            this.TickInterval(1);
        }

        public override void CompTickRare()
        {
            this.TickInterval(250);
        }

        private void TickInterval(int interval)
        {
            if (currentThingList==null)
            {
                return;
            }
            if (this.parent.MapHeld is null)
            {
                return;
            }
            CompCanBeDormant comp = this.parent.GetComp<CompCanBeDormant>();
            if (comp != null)
            {
                if (!comp.Awake)
                {
                    return;
                }
            }
            else if (this.parent.PositionHeld.Fogged(this.parent.MapHeld))
            {
                return;
            }
            if (this.PropsSpawner.requiresPower && !this.PowerOn)
            {
                return;
            }
            if (this.PropsSpawner.requiresFuel && !this.FuelOn)
            {
                return;
            }
            this.ticksUntilSpawn -= interval;
            this.CheckShouldSpawn();
        }

        private void CheckShouldSpawn()
        {
            if (this.ticksUntilSpawn <= 0)
            {
                this.TryDoSpawn();
                this.ResetCountdown();
            }
        }

        public List<IntVec3> AdjCellsCardinalInBounds
        {
            get
            {
                if (this.cachedAdjCellsCardinal == null)
                {
                    this.cachedAdjCellsCardinal = (from c in GenAdj.CellsAdjacentCardinal(this.parent)
                                                   where c.InBounds(this.parent.MapHeld)
                                                   select c).ToList<IntVec3>();
                }
                return this.cachedAdjCellsCardinal;
            }
        }

        public bool TryDoSpawn()
        {
            if (!this.parent.Spawned)
            {
                return false;
            }

            IntVec3 center;

            ThingDef thingToSpawn = ThingDef.Named(currentThingList.items.RandomElement());
            if (thingToSpawn == null)
            {
                return false;
            }
            if (CompConfigurableSpawner.TryFindSpawnCell(this.parent, thingToSpawn, this.PropsSpawner.spawnCount, out center))
            {
                Thing thing = ThingMaker.MakeThing(thingToSpawn, null);
                thing.stackCount = this.PropsSpawner.spawnCount;
                if (thing == null)
                {
                    Log.Error("Could not spawn anything for " + this.parent);
                }
                if (this.PropsSpawner.inheritFaction && thing.Faction != this.parent.Faction)
                {
                    thing.SetFaction(this.parent.Faction, null);
                }
                Thing t;
                GenPlace.TryPlaceThing(thing, this.parent.InteractionCell, this.parent.MapHeld, ThingPlaceMode.Direct, out t, null, null, default(Rot4));
                if (this.PropsSpawner.spawnForbidden)
                {
                    t.SetForbidden(true, true);
                }
                if (this.PropsSpawner.showMessageIfOwned && this.parent.Faction == Faction.OfPlayer)
                {
                    Messages.Message("MessageCompSpawnerSpawnedItem".Translate(thingToSpawn.LabelCap), thing, MessageTypeDefOf.PositiveEvent, true);
                }
                return true;
            }
            return false;
        }

        public static bool TryFindSpawnCell(Thing parent, ThingDef thingToSpawn, int spawnCount, out IntVec3 result)
        {
            foreach (IntVec3 intVec in GenAdj.CellsAdjacent8Way(parent).InRandomOrder(null))
            {
                if (intVec.Walkable(parent.MapHeld))
                {
                    Building edifice = intVec.GetEdifice(parent.MapHeld);
                    if (edifice == null || !thingToSpawn.IsEdifice())
                    {
                        Building_Door building_Door = edifice as Building_Door;
                        if ((building_Door == null || building_Door.FreePassage) && (parent.def.passability == Traversability.Impassable || GenSight.LineOfSight(parent.PositionHeld, 
                            intVec, parent.MapHeld, false, null, 0, 0)))
                        {
                            bool flag = false;
                            List<Thing> thingList = intVec.GetThingList(parent.MapHeld);
                            for (int i = 0; i < thingList.Count; i++)
                            {
                                Thing thing = thingList[i];
                                if (thing.def.category == ThingCategory.Item && (thing.def != thingToSpawn || thing.stackCount > thingToSpawn.stackLimit - spawnCount))
                                {
                                    flag = true;
                                    break;
                                }
                            }
                            if (!flag)
                            {
                                result = intVec;
                                return true;
                            }
                        }
                    }
                }
            }
            result = IntVec3.Invalid;
            return false;
        }

        public  void ResetCountdown()
        {
            if (currentThingList != null)
            {
                this.ticksUntilSpawn = currentThingList.timeInTicks;
            } else
            this.ticksUntilSpawn = 6000;
        }

        public override void PostExposeData()
        {
            string str = this.PropsSpawner.saveKeysPrefix.NullOrEmpty() ? null : (this.PropsSpawner.saveKeysPrefix + "_");
            Scribe_Values.Look<int>(ref this.ticksUntilSpawn, str + "ticksUntilSpawn", 0, false);
            Scribe_Defs.Look<ConfigurableSpawnerDef>(ref this.currentThingList, "currentThingList");
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Spawn product",
                    icon = TexCommand.DesirePower,
                    action = delegate ()
                    {
                        this.TryDoSpawn();
                        this.ResetCountdown();
                    }
                };
            }
            Building building = this.parent as Building;
            yield return ConfigurableSpawnerSettableUtility.SetItemsToSpawnCommand(this);
            yield break;
        }

        public override string CompInspectStringExtra()
        {
            if (this.PropsSpawner.writeTimeLeftToSpawn && (!this.PropsSpawner.requiresPower || this.PowerOn))
            {
                if (this.currentThingList == null)
                {
                    return "VFE_PleaseSelectOutput".Translate();
                }
                else
                {
                    return "NextSpawnedItemIn".Translate(currentThingList.listName.Translate()) + ": " + this.ticksUntilSpawn.ToStringTicksToPeriod(true, false, true, true);
                }

            }
            return null;
        }

        private int ticksUntilSpawn;
    }
}
