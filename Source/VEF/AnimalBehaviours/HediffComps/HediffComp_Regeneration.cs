
using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse.AI;

namespace VEF.AnimalBehaviours
{
    public class HediffComp_Regeneration : HediffComp
    {
        public HediffCompProperties_Regeneration Props
        {
            get
            {
                return (HediffCompProperties_Regeneration)this.props;
            }
        }


        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);

            if (Pawn.IsHashIntervalTick(Props.rateInTicks, delta))
            {
                Pawn pawn = parent.pawn;

                if (pawn.health != null)
                {



                    List<Hediff_Injury> injuries = GetInjuries(pawn, Props.bodypart);

                    if (injuries.Count > 0)
                    {

                        if (!Props.needsSun || pawn.Map != null && pawn.Position.InSunlight(pawn.Map))
                        {
                            if (!Props.needsWater || pawn.Map != null && pawn.Position.GetTerrain(pawn.Map).IsWater)
                            {

                                if (Props.healAll)
                                {
                                    if (Props.onlyTendButNotHeal) {
                                        foreach (Hediff_Injury injury in injuries)
                                        {
                                            if (injury.TendableNow())
                                                injury.Tended(0.7f, 1f);
                                           
                                        }
                                    }
                                    else
                                    {
                                        foreach (Hediff_Injury injury in injuries)
                                        {
                                            injury.Heal(Props.healAmount);
                                        }
                                    }
                                    
                                }
                                else
                                {
                                    if (Props.onlyTendButNotHeal)
                                    {
                                        Hediff_Injury injury = injuries.Where(x => x.TendableNow()).RandomElement();
                                        injury?.Tended(0.7f, 1f);
                                    }
                                    else
                                    {
                                        Hediff_Injury injury = injuries.RandomElement();
                                        injury.Heal(Props.healAmount);
                                    }
                                }

                            }
                        }
                    }
                }
            }

        }

        public List<Hediff_Injury> GetInjuries(Pawn pawn, BodyPartDef bodypart)
        {
            List<Hediff_Injury> injuries = new List<Hediff_Injury>();
            for (int i = 0; i < pawn.health.hediffSet.hediffs.Count; i++)
            {
                if (pawn.health.hediffSet.hediffs[i] is Hediff_Injury hediff_Injury)
                {
                    if(bodypart is null || hediff_Injury.Part.def == bodypart)
                    {
                        if (!Props.onlyBleeding || hediff_Injury.Bleeding)
                        {
                            injuries.Add(hediff_Injury);
                        }

                    }

                                 
                }
            }
            return injuries;
        }

    }
}
