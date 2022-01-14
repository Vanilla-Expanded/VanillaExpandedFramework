using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace AnimalBehaviours
{
    public class CompDigPeriodically : ThingComp
    {
        public int diggingCounter = 0;
        private Effecter effecter;


        public CompProperties_DigPeriodically Props
        {
            get
            {
                return (CompProperties_DigPeriodically)this.props;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (AnimalBehaviours_Settings.flagDigPeriodically) {
                diggingCounter++;
                //Only do every ticksToDig
                if (diggingCounter > Props.ticksToDig)
                {
                    Pawn pawn = this.parent as Pawn;

                    //Null map check. Also check that the animal isn't sleeping, downed or dead, and if onlyWhenTamed is true, that the animal is tamed
                    if ((pawn.Map != null) && pawn.Awake() && !pawn.Downed && !pawn.Dead && (!Props.onlyWhenTamed || (Props.onlyWhenTamed && pawn.Faction != null && pawn.Faction.IsPlayer)))
                    {
                        if (pawn.Position.GetTerrain(pawn.Map).affordances.Contains(TerrainAffordanceDefOf.Diggable))
                        {
                            string thingToDig ="";
                            int amount=1;
                            ThingDef newThing = null;
                            if (!Props.digBiomeRocks) {
                                //This could have been done with a Dictionary
                                thingToDig = this.Props.customThingToDig.RandomElement();
                                int index = Props.customThingToDig.IndexOf(thingToDig);
                                amount = Props.customAmountToDig[index];
                                newThing = ThingDef.Named(thingToDig);
                            }
                            else
                            {
                                amount = Props.customAmountToDigIfRocksOrBricks;
                                IEnumerable<ThingDef> rocksInThisBiome = Find.World.NaturalRockTypesIn(this.parent.Map.Tile);
                                List<ThingDef> chunksInThisBiome = new List<ThingDef>();
                                foreach (ThingDef rock in rocksInThisBiome)
                                {
                                    chunksInThisBiome.Add(rock.building.mineableThing);
                                }
                                if (!Props.digBiomeBricks) {
                                    newThing = Find.World.NaturalRockTypesIn(this.parent.Map.Tile).RandomElementWithFallback().building.mineableThing;

                                }
                                else
                                {
                                    newThing = Find.World.NaturalRockTypesIn(this.parent.Map.Tile).RandomElementWithFallback().building.mineableThing.butcherProducts.FirstOrFallback().thingDef;

                                }

                            }
                            

                           
                            Thing newDugThing = GenSpawn.Spawn(newThing, pawn.Position, pawn.Map, WipeMode.Vanish);
                            newDugThing.stackCount = amount;
                            if (Props.spawnForbidden)
                            {
                                newDugThing.SetForbidden(true);
                            }
                            if (this.effecter == null)
                            {
                                this.effecter = EffecterDefOf.Mine.Spawn();
                            }
                            this.effecter.Trigger(pawn, newDugThing);
                        }

                    }
                    diggingCounter = 0;
                }


            }

            
        }
    }
}
