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
                        //This could have been done with a Dictionary
                        string thingToDig = this.Props.customThingToDig.RandomElement();
                        int index = Props.customThingToDig.IndexOf(thingToDig);
                        int amount = Props.customAmountToDig[index];

                        ThingDef newThing = ThingDef.Named(thingToDig);
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
