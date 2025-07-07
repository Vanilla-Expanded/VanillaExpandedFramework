
using System.Collections.Generic;
using UnityEngine;
using Verse;
using System.Linq;
using RimWorld;


namespace VEF.AnimalBehaviours
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

       


        public override void CompTick()
        {
            if (AnimalBehaviours_Settings.flagRegeneration)
            {
                tickCounter++;

                if (tickCounter >= Props.rateInTicks)
                {
                    Pawn pawn = this.parent as Pawn;

                    if (pawn.health != null)
                    {


                        List<Hediff_Injury> injuries = GetInjuries(pawn,Props.bodypart);

                        if (injuries.Count > 0)
                        {

                            if (!Props.needsSun || Props.needsSun && pawn.Map != null && pawn.Position.InSunlight(pawn.Map))
                            {
                                if (!Props.needsWater || Props.needsWater && pawn.Map != null && pawn.Position.GetTerrain(pawn.Map).IsWater)
                                {
                                    if (Props.healAll)
                                    {
                                        if (Props.onlyTendButNotHeal)
                                        {
                                            foreach (Hediff_Injury injury in injuries)
                                            {
                                                injury.Tended(0.7f, 1f);

                                            }
                                        }
                                        else
                                        {
                                            foreach (Hediff_Injury injury in injuries)
                                            {
                                                injury.Severity = injury.Severity - Props.healAmount;
                                                break;
                                            }
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

