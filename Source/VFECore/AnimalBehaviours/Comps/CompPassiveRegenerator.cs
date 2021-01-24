using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.Sound;
using UnityEngine;
using System.Collections;

using System.Linq;

namespace AnimalBehaviours
{
    public class CompPassiveRegenerator : ThingComp
    {
        public int tickCounter = 0;
        public List<Pawn> pawnList = new List<Pawn>();
        public Pawn thisPawn;

        public CompProperties_PassiveRegenerator Props
        {
            get
            {
                return (CompProperties_PassiveRegenerator)this.props;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            tickCounter++;
            //Only do anything every tickInterval
            if (tickCounter > Props.tickInterval)
            {
                thisPawn = this.parent as Pawn;
                //Null map check. Also will only work if pawn is not dead or downed
                if (thisPawn != null && thisPawn.Map != null && !thisPawn.Dead && !thisPawn.Downed && (!Props.needsToBeTamed || (Props.needsToBeTamed && thisPawn.Faction != null && thisPawn.Faction.IsPlayer)))
                {
                    foreach (Thing thing in GenRadial.RadialDistinctThingsAround(thisPawn.Position, thisPawn.Map, Props.radius, true))
                    {
                        Pawn pawn = thing as Pawn;
                        //It won't affect mechanoids, dead people or itself
                        if (pawn != null && !pawn.Dead && pawn.RaceProps.IsFlesh && pawn != this.parent)
                        {

                            //Only show an effect if the user wants it to, or it gets obnoxious
                            if (Props.showEffect)
                            {

                                SoundDefOf.PsychicPulseGlobal.PlayOneShot(new TargetInfo(this.parent.Position, this.parent.Map, false));
                                MoteMaker.MakeAttachedOverlay(this.parent, ThingDef.Named("Mote_PsycastPsychicEffect"), Vector3.zero, 1f, -1f);
                            }
                            //Regenerate wounds
                            if (pawn.health != null)
                            {
                                if (pawn.health.hediffSet.GetInjuriesTendable() != null && pawn.health.hediffSet.GetInjuriesTendable().Count<Hediff_Injury>() > 0)
                                {
                                    foreach (Hediff_Injury injury in pawn.health.hediffSet.GetInjuriesTendable())
                                    {
                                        injury.Severity = injury.Severity - 0.1f;
                                        break;
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


