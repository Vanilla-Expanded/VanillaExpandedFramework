
using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse.AI;

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

            tickCounter++;

            if (tickCounter >= Props.rateInTicks)
            {
                Pawn pawn = parent.pawn;

                if (pawn.health != null)
                {



                    List<Hediff_Injury> injuries = GetInjuries(pawn);

                    if (injuries.Count > 0)
                    {

                        if (!Props.needsSun || Props.needsSun && pawn.Map != null && pawn.Position.InSunlight(pawn.Map))
                        {
                            if (!Props.needsWater || Props.needsWater && pawn.Map != null && pawn.Position.GetTerrain(pawn.Map).IsWater)
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
                }
                tickCounter = 0;
            }

        }

        public List<Hediff_Injury> GetInjuries(Pawn pawn)
        {
            List<Hediff_Injury> injuries = new List<Hediff_Injury>();
            for (int i = 0; i < pawn.health.hediffSet.hediffs.Count; i++)
            {
                Hediff_Injury hediff_Injury = pawn.health.hediffSet.hediffs[i] as Hediff_Injury;
                if (hediff_Injury != null)
                {
                    injuries.Add(hediff_Injury);
                }

            }
            return injuries;
        }


    }
}
