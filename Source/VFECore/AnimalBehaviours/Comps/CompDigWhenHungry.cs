using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using VFECore;

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
            if (AnimalBehaviours_Settings.flagDigWhenHungry && (pawn.Map != null) && (pawn.Awake()) && (!Props.digOnlyOnGrowingSeason || 
                (Props.digOnlyOnGrowingSeason && (pawn.Map.mapTemperature.OutdoorTemp > Props.minTemperature && pawn.Map.mapTemperature.OutdoorTemp < Props.maxTemperature))) && 
                ((pawn.needs?.food?.CurLevelPercentage < pawn.needs?.food?.PercentageThreshHungry) ||
                (Props.digAnywayEveryXTicks && this.parent.IsHashIntervalTick(Props.timeToDigForced))))
            {
                if (pawn.Position.GetTerrain(pawn.Map).affordances.Contains(VFEDefOf.Diggable))
                {
                    if (stopdiggingcounter <= 0)
                    {
                        if (Props.acceptedTerrains != null)
                        {
                            if (Props.acceptedTerrains.Contains(pawn.Position.GetTerrain(pawn.Map).defName))
                            {
                                Thing newcorpse;
                                if (Props.isFrostmite)
                                {
                                    PawnKindDef wildman = PawnKindDef.Named("WildMan");
                                    Faction faction = FactionUtility.DefaultFactionFrom(wildman.defaultFactionType);
                                    Pawn newPawn = PawnGenerator.GeneratePawn(wildman, faction);
                                    newcorpse = GenSpawn.Spawn(newPawn, pawn.Position, pawn.Map, WipeMode.Vanish);
                                    newcorpse.Kill(null, null);
                                   
                                }
                                else {
                                    if (Props.customThingsToDig != null)
                                    {
                                        string thingToDig = this.Props.customThingsToDig.RandomElement();
                                        int index = Props.customThingsToDig.IndexOf(thingToDig);
                                        int amount;
                                        if (Props.customAmountsToDig != null)
                                        {
                                            amount = Props.customAmountsToDig[index];

                                        }
                                        else
                                        {
                                            amount = Props.customAmountToDig;
                                        }
                                        ThingDef newThing = ThingDef.Named(thingToDig);
                                        newcorpse = GenSpawn.Spawn(newThing, pawn.Position, pawn.Map, WipeMode.Vanish);
                                        newcorpse.stackCount = amount;
                                    }
                                    else
                                    {
                                        ThingDef newThing = ThingDef.Named(this.Props.customThingToDig);
                                        newcorpse = GenSpawn.Spawn(newThing, pawn.Position, pawn.Map, WipeMode.Vanish);
                                        newcorpse.stackCount = this.Props.customAmountToDig;
                                    }
                                }
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
                            Thing newcorpse;
                            if (Props.isFrostmite)
                            {
                                PawnKindDef wildman = PawnKindDef.Named("WildMan");
                                Faction faction = FactionUtility.DefaultFactionFrom(wildman.defaultFactionType);
                                Pawn newPawn = PawnGenerator.GeneratePawn(wildman, faction);
                                newcorpse = GenSpawn.Spawn(newPawn, pawn.Position, pawn.Map, WipeMode.Vanish);
                                newcorpse.Kill(null, null);

                            }
                            else
                            {
                                if (Props.customThingsToDig != null)
                                {
                                    string thingToDig = this.Props.customThingsToDig.RandomElement();
                                    int index = Props.customThingsToDig.IndexOf(thingToDig);
                                    int amount;
                                    if (Props.customAmountsToDig != null)
                                    {
                                        amount = Props.customAmountsToDig[index];

                                    }
                                    else
                                    {
                                        amount = Props.customAmountToDig;
                                    }
                                    ThingDef newThing = ThingDef.Named(thingToDig);
                                    newcorpse = GenSpawn.Spawn(newThing, pawn.Position, pawn.Map, WipeMode.Vanish);
                                    newcorpse.stackCount = amount;
                                }
                                else
                                {
                                    ThingDef newThing = ThingDef.Named(this.Props.customThingToDig);
                                    newcorpse = GenSpawn.Spawn(newThing, pawn.Position, pawn.Map, WipeMode.Vanish);
                                    newcorpse.stackCount = this.Props.customAmountToDig;
                                }
                            }
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

