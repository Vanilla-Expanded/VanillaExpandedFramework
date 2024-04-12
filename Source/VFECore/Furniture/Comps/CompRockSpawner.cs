using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using System.Linq;

namespace VanillaFurnitureExpanded
{
    public class CompRockSpawner : ThingComp
    {
        public ThingDef RockTypeToMine;
        private List<IntVec3> cachedAdjCellsCardinal;

        public CompProperties_RockSpawner PropsSpawner
        {
            get
            {
                return (CompProperties_RockSpawner)this.props;
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
            if (!this.parent.Spawned)
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
            else if (this.parent.Position.Fogged(this.parent.Map))
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
                                                   where c.InBounds(this.parent.Map)
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

            IEnumerable<ThingDef> rocksInThisBiome = Find.World.NaturalRockTypesIn(this.parent.Map.Tile);
            List<ThingDef> chunksInThisBiome = new List<ThingDef>();
            foreach (ThingDef rock in rocksInThisBiome)
            {
                chunksInThisBiome.Add(rock.building.mineableThing);
            }
            ThingDef rockToSpawnNow = Find.World.NaturalRockTypesIn(this.parent.Map.Tile).RandomElementWithFallback().building.mineableThing;
           
            
            for (int i = 0; i < this.AdjCellsCardinalInBounds.Count; i++)
            {
                IntVec3 c = this.AdjCellsCardinalInBounds[i];
                if (c.InBounds(this.parent.Map))
                {
                    List<Thing> thingList = c.GetThingList(this.parent.Map);
                    for (int j = 0; j < thingList.Count; j++)
                    {
                        if (chunksInThisBiome.Contains(thingList[j].def))
                        {
                            
                                return false;
                           
                        }
                    }
                }
            }

            if (RockTypeToMine != null)
            {
                rockToSpawnNow = RockTypeToMine;
            }


            if (CompRockSpawner.TryFindSpawnCell(this.parent, rockToSpawnNow, this.PropsSpawner.spawnCount, out center))
            {
                Thing thing = ThingMaker.MakeThing(rockToSpawnNow, null);
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
                GenPlace.TryPlaceThing(thing, this.parent.InteractionCell, this.parent.Map, ThingPlaceMode.Direct, out t, null, null, default(Rot4));
                if (this.PropsSpawner.spawnForbidden)
                {
                    t.SetForbidden(true, true);
                }
                if (this.PropsSpawner.showMessageIfOwned && this.parent.Faction == Faction.OfPlayer)
                {
                    Messages.Message("MessageCompSpawnerSpawnedItem".Translate(rockToSpawnNow.LabelCap), thing, MessageTypeDefOf.PositiveEvent, true);
                }
                return true;
            }
            return false;
        }

        public static bool TryFindSpawnCell(Thing parent, ThingDef thingToSpawn, int spawnCount, out IntVec3 result)
        {
            foreach (IntVec3 intVec in GenAdj.CellsAdjacent8Way(parent).InRandomOrder(null))
            {
                if (intVec.Walkable(parent.Map))
                {
                    Building edifice = intVec.GetEdifice(parent.Map);
                    if (edifice == null || !thingToSpawn.IsEdifice())
                    {
                        Building_Door building_Door = edifice as Building_Door;
                        if ((building_Door == null || building_Door.FreePassage) && (parent.def.passability == Traversability.Impassable || GenSight.LineOfSight(parent.Position, intVec, parent.Map, false, null, 0, 0)))
                        {
                            bool flag = false;
                            List<Thing> thingList = intVec.GetThingList(parent.Map);
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

        private void ResetCountdown()
        {
            /*if (ModLister.HasActiveModWithName("Vanilla Factions Expanded - Mechanoids"))
            {
                try
                {
                    ((Action)(() =>
                    {
                        VFEM.MechShipsSettings settings = VFEM.MechShipsMod.settings;
                        float multiplier = settings.VFEM_factorySpeedMultiplier;
                        this.ticksUntilSpawn = (int)(this.PropsSpawner.spawnIntervalRange.RandomInRange * multiplier);
                    }))();
                }
                catch (TypeLoadException) { }
            }
            else
            {*/
                this.ticksUntilSpawn = this.PropsSpawner.spawnIntervalRange.RandomInRange;
            //}

        }

        public override void PostExposeData()
        {
            string str = this.PropsSpawner.saveKeysPrefix.NullOrEmpty() ? null : (this.PropsSpawner.saveKeysPrefix + "_");
            Scribe_Values.Look<int>(ref this.ticksUntilSpawn, str + "ticksUntilSpawn", 0, false);
            Scribe_Defs.Look<ThingDef>(ref this.RockTypeToMine, "RockTypeToMine");
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Spawn rock",
                    icon = TexCommand.DesirePower,
                    action = delegate ()
                    {
                        this.TryDoSpawn();
                        this.ResetCountdown();
                    }
                };
            }
            Building building = this.parent as Building;
            yield return StoneTypeSettableUtility.SetStoneToMineCommand(this);
            yield break;
        }

        public override string CompInspectStringExtra()
        {
            if (this.PropsSpawner.writeTimeLeftToSpawn && (!this.PropsSpawner.requiresPower || this.PowerOn))
            {
                if (this.RockTypeToMine == null)
                {
                    return "NextSpawnedItemIn".Translate("VFE_RandomRock".Translate()) + ": " + this.ticksUntilSpawn.ToStringTicksToPeriod(true, false, true, true); 
                }
                else
                {
                    return "NextSpawnedItemIn".Translate(RockTypeToMine.LabelCap) + ": " + this.ticksUntilSpawn.ToStringTicksToPeriod(true, false, true, true);
                }

            }
            return null;
        }

        private int ticksUntilSpawn;
    }
}
