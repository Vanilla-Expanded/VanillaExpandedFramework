using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace AnimalBehaviours
{
    public class CompAsexualReproduction : ThingComp
    {

        public int ticksInday = 60000;
        public int asexualFissionCounter = 0;


        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref this.asexualFissionCounter, "asexualFissionCounter", 0, false);
        }




        public CompProperties_AsexualReproduction Props
        {
            get
            {
                return (CompProperties_AsexualReproduction)this.props;
            }
        }

        protected int reproductionIntervalDays
        {
            get
            {
                return this.Props.reproductionIntervalDays;
            }
        }

        protected string customString
        {
            get
            {
                return this.Props.customString;
            }
        }

        protected bool produceEggs
        {
            get
            {
                return this.Props.produceEggs;
            }
        }

        protected string eggDef
        {
            get
            {
                return this.Props.eggDef;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            Pawn pawn = this.parent as Pawn;
            //Important, without a null map check creatures will reproduce while on caravans, producing errors
            if (pawn.Map != null && AnimalBehaviours_Settings.flagAsexualReproduction)
            {
                if (this.Props.isGreenGoo)
                {
                    asexualFissionCounter++;
                    //This checks if the map has been filled with creatures. If this check wasn't made, the animal would fill
                    //the map and grind the game to a halt
                    if ((asexualFissionCounter >= ticksInday * reproductionIntervalDays) && ((this.parent.Map != null) && (this.parent.Map.listerThings.ThingsOfDef(ThingDef.Named(this.Props.GreenGooTarget)).Count < this.Props.GreenGooLimit)))
                    {
                        //The offspring has the pawn as both mother and father and I find this funny 
                        Hediff_Pregnant.DoBirthSpawn(pawn, pawn);
                        //Only show a message if the pawn is part of the player's faction
                        if (pawn.Faction == Faction.OfPlayer)
                        {
                            Messages.Message(Props.asexualCloningMessage.Translate(pawn.LabelIndefinite().CapitalizeFirst()), pawn, MessageTypeDefOf.PositiveEvent, true);

                        }
                        asexualFissionCounter = 0;
                    }
                    //Just reset the counter if the map is filled
                    else if (asexualFissionCounter >= ticksInday * reproductionIntervalDays)
                    {
                        asexualFissionCounter = 0;
                    }

                }

                //Non-green goo creatures only reproduce if they are part of the player's faction, like vanilla animals
                else if ((pawn.Faction == Faction.OfPlayer) && (pawn.ageTracker.CurLifeStage.reproductive))
                {
                    asexualFissionCounter++;
                    if (asexualFissionCounter >= ticksInday * reproductionIntervalDays)
                    {
                        //If it produces eggs or spores, it will just spawn them. Note that these eggs are not part of the player's
                        //faction, the animal hatched from them will be wild
                        if (produceEggs)
                        {
                            GenSpawn.Spawn(ThingDef.Named(eggDef), pawn.Position, pawn.Map);
                            Messages.Message(Props.asexualEggMessage.Translate(pawn.LabelIndefinite().CapitalizeFirst()), pawn, MessageTypeDefOf.PositiveEvent, true);
                            asexualFissionCounter = 0;
                        }
                        //If not, do a normal fission
                        else
                        {
                            if (Props.convertsIntoAnotherDef)
                            {
                                PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDef.Named(Props.newDef), pawn.Faction, PawnGenerationContext.NonPlayer, -1, false, true, false, false, true, 1f, false, false, true, true, true, false, false, false, false, 0f, 0f,null, 1f, null, null, null, null, null, null, null, null, null, null, null, null);
                                Pawn pawnToGenerate = PawnGenerator.GeneratePawn(request);
                                PawnUtility.TrySpawnHatchedOrBornPawn(pawnToGenerate, pawn);
                                Messages.Message(Props.asexualHatchedMessage.Translate(pawn.LabelIndefinite().CapitalizeFirst()), pawn, MessageTypeDefOf.PositiveEvent, true);
                                asexualFissionCounter = 0;
                            }
                            else
                            {
                                Hediff_Pregnant.DoBirthSpawn(pawn, pawn);
                                Messages.Message(Props.asexualHatchedMessage.Translate(pawn.LabelIndefinite().CapitalizeFirst()), pawn, MessageTypeDefOf.PositiveEvent, true);
                                asexualFissionCounter = 0;
                            }

                        }

                    }
                }

            }

        }

        public override string CompInspectStringExtra()
        {

            //Custom strings to show
            if (AnimalBehaviours_Settings.flagAsexualReproduction) {
                Pawn pawn = this.parent as Pawn;
                if (this.Props.isGreenGoo)
                {
                    float totalProgress = ((float)asexualFissionCounter / (float)(ticksInday * reproductionIntervalDays));
                    return customString + totalProgress.ToStringPercent() + " (" + reproductionIntervalDays.ToString() + " days)";
                }

                else if ((pawn.Faction == Faction.OfPlayer) && (pawn.ageTracker.CurLifeStage.reproductive))
                {
                    float totalProgress = ((float)asexualFissionCounter / (float)(ticksInday * reproductionIntervalDays));
                    return customString + totalProgress.ToStringPercent() + " (" + reproductionIntervalDays.ToString() + " days)";
                }
                else return "";
            } else return "VFE_AsexualReproductionDisabled".Translate();


        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (!DebugSettings.ShowDevGizmos || this.parent.Faction != Faction.OfPlayer)
            {
                yield break;
            }

            yield return new Command_Action
            {
                defaultLabel = "DEV: Reproduce now",
                defaultDesc = "Set asexual reproduction to trigger now",
                action = delegate
                {
                    asexualFissionCounter = ticksInday * reproductionIntervalDays;
                },

            };
        }
    }
}
