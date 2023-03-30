
using System.Collections.Generic;
using UnityEngine;
using Verse;
using System.Linq;


namespace AnimalBehaviours
{
    public class CompRegeneration : ThingComp
    {

        public int tickCounter = 0;



        public CompProperties_Regeneration Props
        {
            get
            {
                return (CompProperties_Regeneration)this.props;
            }
        }

        protected int rateInTicks
        {
            get
            {
                return this.Props.rateInTicks;
            }
        }

        protected float healAmount
        {
            get
            {
                return this.Props.healAmount;
            }
        }

        protected bool healAll
        {
            get
            {
                return this.Props.healAll;
            }
        }


        public override void CompTick()
        {
            if (AnimalBehaviours_Settings.flagRegeneration)
            {
                tickCounter++;

                if (tickCounter >= rateInTicks)
                {
                    Pawn pawn = this.parent as Pawn;

                    if (pawn.health != null)
                    {


                        List<Hediff_Injury> injuries = GetInjuries(pawn,Props.bodypart);

                        if (injuries.Count > 0)
                        {


                            if (healAll)
                            {
                                foreach (Hediff_Injury injury in injuries)
                                {
                                    injury.Severity = injury.Severity - healAmount;
                                    break;
                                }
                            }
                            else
                            {
                                Hediff_Injury injury = injuries.RandomElement();
                                injury.Severity = injury.Severity - healAmount;
                            }

                        }




                    }
                    tickCounter = 0;
                }
            }

        }
        public List<Hediff_Injury> GetInjuries(Pawn pawn, BodyPartDef bodypart)
        {
            List<Hediff_Injury> injuries = new List<Hediff_Injury>();
            for (int i = 0; i < pawn.health.hediffSet.hediffs.Count; i++)
            {
                Hediff_Injury hediff_Injury = pawn.health.hediffSet.hediffs[i] as Hediff_Injury;
                if (hediff_Injury != null)
                {
                    if (bodypart != null)
                    {
                        if (hediff_Injury.Part.def == bodypart) { injuries.Add(hediff_Injury); }
                    }
                    else { 
                        injuries.Add(hediff_Injury);
                    }

                    
                }

            }
            return injuries;
        }

       


    }
}

