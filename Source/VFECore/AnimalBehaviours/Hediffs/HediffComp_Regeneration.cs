
using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace AnimalBehaviours
{
    class HediffComp_Regeneration : HediffComp
    {
        public HediffCompProperties_Regeneration Props
        {
            get
            {
                return (HediffCompProperties_Regeneration)this.props;
            }
        }
        public int tickCounter = 0;




        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (AnimalBehaviours_Settings.flagRegeneration)
            {
                tickCounter++;

                if (tickCounter >= Props.rateInTicks)
                {
                    Pawn pawn = parent.pawn;

                    if (pawn.health != null)
                    {
                        IEnumerable<Hediff_Injury> injuriesEnumerable = pawn.health.hediffSet.GetInjuriesTendable();

                        if (injuriesEnumerable != null)
                        {
                            Hediff_Injury[] injuries = injuriesEnumerable.ToArray();

                            if (injuries.Any())
                            {
                                if (Props.healAll)
                                {
                                    foreach (Hediff_Injury injury in injuries)
                                    {
                                        injury.Severity = injury.Severity - Props.healAmount;
                                        break;
                                    }
                                }
                                else
                                {
                                    Hediff_Injury injury = injuries.RandomElement();
                                    injury.Severity = injury.Severity - Props.healAmount;
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
