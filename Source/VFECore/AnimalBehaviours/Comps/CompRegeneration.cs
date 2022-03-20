
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
                        IEnumerable<Hediff_Injury> injuriesEnumerable = pawn.health.hediffSet.GetInjuriesTendable();

                        if (injuriesEnumerable != null)
                        {
                            Hediff_Injury[] injuries = injuriesEnumerable.ToArray();

                            if (injuries.Any())
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
                    }
                    tickCounter = 0;
                }
            }
            
        }


    }
}

