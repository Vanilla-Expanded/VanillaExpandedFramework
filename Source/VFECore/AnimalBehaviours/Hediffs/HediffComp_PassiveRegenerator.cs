
using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse.Sound;
using UnityEngine;

namespace AnimalBehaviours
{
    class HediffComp_PassiveRegenerator : HediffComp
    {


        public int tickCounter = 0;
        public List<Pawn> pawnList = new List<Pawn>();
        public Pawn thisPawn;

        public HediffCompProperties_PassiveRegenerator Props
        {
            get
            {
                return (HediffCompProperties_PassiveRegenerator)this.props;
            }
        }
       


        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
           

                tickCounter++;
                //Only do anything every tickInterval
                if (tickCounter > Props.tickInterval)
                {
                    thisPawn = this.parent.pawn as Pawn;
                    //Null map check. Also will only work if pawn is not dead or downed
                    if (thisPawn != null && thisPawn.Map != null && !thisPawn.Dead && !thisPawn.Downed && (!Props.needsToBeTamed || (Props.needsToBeTamed && thisPawn.Faction != null && thisPawn.Faction.IsPlayer)))
                    {
                        foreach (Thing thing in GenRadial.RadialDistinctThingsAround(thisPawn.Position, thisPawn.Map, Props.radius, true))
                        {
                            Pawn pawn = thing as Pawn;
                            //It won't affect mechanoids, dead people or itself
                            if (pawn != null && !pawn.Dead && pawn.RaceProps.IsFlesh && pawn != this.parent.pawn)
                            {

                                //Only show an effect if the user wants it to, or it gets obnoxious
                                if (Props.showEffect)
                                {

                                    SoundDefOf.PsychicPulseGlobal.PlayOneShot(new TargetInfo(this.parent.pawn.Position, this.parent.pawn.Map, false));
                                    FleckMaker.AttachedOverlay(this.parent.pawn, DefDatabase<FleckDef>.GetNamed("PsycastPsychicEffect"), Vector3.zero, 1f, -1f);
                                }
                                //Regenerate wounds
                                if (pawn.health != null)
                                {
                                    IEnumerable<Hediff_Injury> injuriesEnumerable = pawn.health.hediffSet.GetHediffsTendable().OfType<Hediff_Injury>();

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

                            }
                        }
                    }
                    tickCounter = 0;
                }
            
        }


    }
}
