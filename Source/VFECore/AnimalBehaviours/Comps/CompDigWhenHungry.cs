using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace AnimalBehaviours
{
    public class CompDigWhenHungry : ThingComp
    {
        public int stopdiggingcounter = 0;
        private Effecter effecter;

        public CompProperties_DigWhenHungry Props
        {
            get
            {
                return (CompProperties_DigWhenHungry)this.props;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            Pawn pawn = this.parent as Pawn;
            if ((pawn.Map != null) && (pawn.Awake()) && ((pawn.needs.food.CurLevelPercentage < pawn.needs.food.PercentageThreshHungry) || this.parent.IsHashIntervalTick(120000)))
            {
                if (pawn.Position.GetTerrain(pawn.Map).affordances.Contains(TerrainAffordanceDefOf.Diggable))
                {
                    if (stopdiggingcounter <= 0)
                    {
                        if (Props.acceptedTerrains != null)
                        {
                            if (Props.acceptedTerrains.Contains(pawn.Position.GetTerrain(pawn.Map).defName))
                            {
                                ThingDef newThing = ThingDef.Named(this.Props.customThingToDig);
                                Thing newcorpse = GenSpawn.Spawn(newThing, pawn.Position, pawn.Map, WipeMode.Vanish);
                                newcorpse.stackCount = this.Props.customAmountToDig;
                                if (Props.spawnForbidden)
                                {
                                    newcorpse.SetForbidden(true);
                                }
                                if (this.effecter == null)
                                {
                                    this.effecter = EffecterDefOf.Mine.Spawn();
                                }
                                this.effecter.Trigger(pawn, newcorpse);

                            }
                        } else
                        {
                            ThingDef newThing = ThingDef.Named(this.Props.customThingToDig);
                            Thing newcorpse = GenSpawn.Spawn(newThing, pawn.Position, pawn.Map, WipeMode.Vanish);
                            newcorpse.stackCount = this.Props.customAmountToDig;
                            if (Props.spawnForbidden)
                            {
                                newcorpse.SetForbidden(true);
                            }
                            if (this.effecter == null)
                            {
                                this.effecter = EffecterDefOf.Mine.Spawn();
                            }
                            this.effecter.Trigger(pawn, newcorpse);
                        }
                                


                         

                        stopdiggingcounter = Props.timeToDig;
                    }
                    stopdiggingcounter--;
                }
            }
        }
    }
}

