using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using RimWorld.Planet;
using UnityEngine;
using Verse.AI.Group;

namespace AnimalBehaviours
{
    public class HediffComp_AsexualReproduction : HediffComp
    {

        public int ticksInday = 60000;
        public int asexualFissionCounter = 0;


        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look<int>(ref this.asexualFissionCounter, "asexualFissionCounter", 0, false);
        }




        public HediffCompProperties_AsexualReproduction Props
        {
            get
            {
                return (HediffCompProperties_AsexualReproduction)this.props;
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

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            Pawn pawn = this.parent.pawn as Pawn;
            //Important, without a null map check creatures will reproduce while on caravans, producing errors
            if (pawn.Map != null)
            {
                if (this.Props.isGreenGoo)
                {
                    asexualFissionCounter++;
                    //This checks if the map has been filled with creatures. If this check wasn't made, the animal would fill
                    //the map and grind the game to a halt
                    if ((asexualFissionCounter >= ticksInday * reproductionIntervalDays) && ((pawn.Map != null) && (pawn.Map.listerThings.ThingsOfDef(ThingDef.Named(this.Props.GreenGooTarget)).Count < this.Props.GreenGooLimit)))
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
                                PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDef.Named(Props.newDef), pawn.Faction, PawnGenerationContext.NonPlayer, -1, false, true, false, false, true, 1f, false, false, true, true, true, false, false, false, false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null);
                                Pawn pawnToGenerate = PawnGenerator.GeneratePawn(request);
                                PawnUtility.TrySpawnHatchedOrBornPawn(pawnToGenerate, pawn);
                                Messages.Message(Props.asexualHatchedMessage.Translate(pawn.LabelIndefinite().CapitalizeFirst()), pawn, MessageTypeDefOf.PositiveEvent, true);
                                asexualFissionCounter = 0;
                            }
                            else
                            {
                                Pawn progenitor = this.parent.pawn as Pawn;
                                int num = (progenitor.RaceProps.litterSizeCurve == null) ? 1 : Mathf.RoundToInt(Rand.ByCurve(progenitor.RaceProps.litterSizeCurve));
                                if (num < 1)
                                {
                                    num = 1;
                                }
                                PawnGenerationRequest request = new PawnGenerationRequest(progenitor.kindDef, progenitor.Faction, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: false, allowDead: false, allowDowned: true, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, DevelopmentalStage.Newborn);
                                Pawn pawnCreated = null;
                                for (int i = 0; i < num; i++)
                                {
                                    pawnCreated = PawnGenerator.GeneratePawn(request);

                                    if (Props.endogeneTransfer)
                                    {
                                        List<Gene> listOfEndogenes = progenitor.genes?.Endogenes;

                                        foreach (Gene gene in listOfEndogenes)
                                        {
                                            pawnCreated.genes?.AddGene(gene.def, false);
                                        }
                                        if (progenitor.genes?.Xenotype != null)
                                        {
                                            pawnCreated.genes?.SetXenotype(progenitor.genes?.Xenotype);
                                        }


                                    }

                                    if (PawnUtility.TrySpawnHatchedOrBornPawn(pawnCreated, progenitor))
                                    {
                                        if (pawnCreated.playerSettings != null && progenitor.playerSettings != null)
                                        {
                                            pawnCreated.playerSettings.AreaRestrictionInPawnCurrentMap = progenitor.playerSettings.AreaRestrictionInPawnCurrentMap;
                                        }
                                        if (pawnCreated.RaceProps.IsFlesh)
                                        {
                                            pawnCreated.relations.AddDirectRelation(PawnRelationDefOf.Parent, progenitor);

                                        }
                                        if (progenitor.Spawned)
                                        {
                                            progenitor.GetLord()?.AddPawn(pawnCreated);
                                        }
                                    }
                                    else
                                    {
                                        Find.WorldPawns.PassToWorld(pawnCreated, PawnDiscardDecideMode.Discard);
                                    }
                                    TaleRecorder.RecordTale(TaleDefOf.GaveBirth, progenitor, pawn);
                                }
                                if (progenitor.Spawned)
                                {
                                    FilthMaker.TryMakeFilth(progenitor.Position, progenitor.Map, ThingDefOf.Filth_AmnioticFluid, progenitor.LabelIndefinite(), 5);
                                    if (progenitor.caller != null)
                                    {
                                        progenitor.caller.DoCall();
                                    }
                                    if (pawn.caller != null)
                                    {
                                        pawn.caller.DoCall();
                                    }
                                }





                                Messages.Message(Props.asexualHatchedMessage.Translate(pawn.LabelIndefinite().CapitalizeFirst()), pawn, MessageTypeDefOf.PositiveEvent, true);

                                asexualFissionCounter = 0;
                            }

                        }

                    }
                }

            }

        }

        public override string CompLabelInBracketsExtra => GetLabel();




        public string GetLabel()
        {

            
                Pawn pawn = this.parent.pawn as Pawn;
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
           


        }

    }
}
