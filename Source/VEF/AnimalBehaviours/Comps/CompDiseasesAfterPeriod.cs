﻿using RimWorld;
using Verse;
using Verse.Sound;
using System.Collections.Generic;

namespace VEF.AnimalBehaviours

{
    public class CompDiseasesAfterPeriod : ThingComp, PawnGizmoProvider
    {
        public int tickCounter = 0;

        public CompProperties_DiseasesAfterPeriod Props
        {
            get
            {
                return (CompProperties_DiseasesAfterPeriod)this.props;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref this.tickCounter, "tickCounter", 0, false);
        }

        public override void CompTickInterval(int delta)
        {
            base.CompTickInterval(delta);
            tickCounter += delta;

            if (tickCounter >= Props.timeToApplyInTicks)
            {
                if (this.parent is Pawn pawn && pawn.Map != null)
                {
                    HediffDef randomHediff = Props.hediffsToApply.RandomElement();
                    Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(randomHediff);
                    if (hediff == null)
                    {
                        pawn.health.AddHediff(randomHediff);
                    }
                    
                    
                }
                tickCounter = (int)(Props.timeToApplyInTicks*Props.percentageOfMaxToReapply);
            }
        }

        public IEnumerable<Gizmo> GetGizmos()
        {

            if (DebugSettings.ShowDevGizmos)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = "DEBUG: Give age related diseases";
                command_Action.icon = TexCommand.DesirePower;
                command_Action.action = delegate
                {
                    tickCounter = Props.timeToApplyInTicks - 10;
                };
                yield return command_Action;
            }
        }



    }
}
