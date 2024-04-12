using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using UnityEngine;
using System.Collections;

namespace AnimalBehaviours
{
    public class CompStateAfterHealthLoss : ThingComp
    {
        public int tickCounter = 0;


        public CompProperties_StateAfterHealthLoss Props
        {
            get
            {
                return (CompProperties_StateAfterHealthLoss)this.props;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            tickCounter++;
            //Only check things every tickInterval
            if (tickCounter > Props.tickInterval)
            {
                Pawn thisPawn = this.parent as Pawn;
                if (thisPawn != null && thisPawn.Map != null && !thisPawn.Dead && !thisPawn.Downed)
                {
                    //If pawn's health reaches a threshold
                    if (thisPawn.health.summaryHealth.SummaryHealthPercent < ((float)(Props.healthPercent) / 100))
                    {
                        //apply mental state
                        thisPawn.mindState.mentalStateHandler.TryStartMentalState(DefDatabase<MentalStateDef>.GetNamed(Props.mentalState, true), null, true, false, false,null, false);
                    }
                }
                tickCounter = 0;
            }
        }
    }
}

