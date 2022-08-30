using System;
using Verse;
using Verse.AI;
using RimWorld;
using Verse.Sound;
using UnityEngine;

namespace AnimalBehaviours
{
    public class CompHediffWhenFleeing : ThingComp
    {
        public int cooldownCounter;
        public const int cooldown = 60000; //1 day
        public bool onCoolDown = false;

        public CompProperties_HediffWhenFleeing Props
        {
            get
            {
                return (CompProperties_HediffWhenFleeing)this.props;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look<int>(ref this.cooldownCounter, "cooldownCounter", 0, false);
            Scribe_Values.Look<bool>(ref this.onCoolDown, "onCoolDown", false, false);

        }


        public override void CompTick()
        {
            base.CompTick();

            if (onCoolDown)
            {
                cooldownCounter++;
                if(cooldownCounter >= cooldown)
                {
                    onCoolDown=false;
                }
            }else if (this.parent.IsHashIntervalTick(Props.tickInterval) &&parent.Map!=null)
            {
                Pawn pawn = this.parent as Pawn;

                if(pawn.CurJob.def == JobDefOf.Flee || pawn.CurJob.def == JobDefOf.FleeAndCower)
                {
                    if (Props.graphicAndSoundEffect)
                    {
                        SoundDefOf.PsychicPulseGlobal.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map, false));
                        FleckMaker.AttachedOverlay(pawn, DefDatabase<FleckDef>.GetNamed("PsycastPsychicEffect"), Vector3.zero, 6f, -1f);
                    }

                    pawn.health.AddHediff(Props.hediffToCause);

                    if (Props.hediffOnRadius) {
                        foreach (Thing thing in GenRadial.RadialDistinctThingsAround(pawn.Position, pawn.Map, Props.radius, true))
                        {
                            Pawn affectedPawn = thing as Pawn;
                           
                            if (affectedPawn != null && affectedPawn.Faction == Faction.OfPlayerSilentFail)
                            {                              
                                if (!affectedPawn.Dead && !affectedPawn.Downed && affectedPawn.GetStatValue(StatDefOf.PsychicSensitivity, true) > 0f)
                                {
                                    affectedPawn.health.AddHediff(Props.hediffToCause);
                                }
                            }
                        }
                    }
                    


                    onCoolDown = true;
                }

            }


        }


    }
}

