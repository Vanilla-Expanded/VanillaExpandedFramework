

using RimWorld;
using System.Collections.Generic;
using Verse;
using System.Linq;
using Verse.Sound;
using UnityEngine;

namespace VanillaCookingExpanded
{
    class HediffComp_WhileHavingThoughts : HediffComp
    {
        public bool flagAmIThinking = false;


        public int checkingInterval = 600;

        public int checkingCounter = 600;

        public override void CompExposeData()
        {
            Scribe_Values.Look<bool>(ref this.flagAmIThinking, "flagAmIThinking", false, false);

        }

        public HediffCompProperties_WhileHavingThoughts Props
        {
            get
            {
                return (HediffCompProperties_WhileHavingThoughts)this.props;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            checkingCounter++;

            if (checkingCounter > checkingInterval)
            {
                if (Props.thoughtDefs.Count > 0)
                {
                    foreach (ThoughtDef thoughtDef in this.Props.thoughtDefs)
                    {
                        flagAmIThinking = false;
                        if (this.Pawn.needs.mood.thoughts.memories.GetFirstMemoryOfDef(thoughtDef) != null)
                        {
                            flagAmIThinking = true;
                            break;
                        }
                    }
                }
                if (Props.removeThoughtDefs.Count > 0)
                {
                    foreach (ThoughtDef thoughtDefToRemove in this.Props.removeThoughtDefs)
                    {
                        if (this.Pawn.needs.mood.thoughts.memories.GetFirstMemoryOfDef(thoughtDefToRemove) != null)
                        {
                            this.Pawn.needs.mood.thoughts.memories.GetFirstMemoryOfDef(thoughtDefToRemove).moodPowerFactor = 0;
                        }
                    }

                }


                if (!flagAmIThinking)
                {
                    this.Pawn.health.RemoveHediff(this.parent);
                }
                checkingCounter = 0;
            }


        }

        public override void Notify_PawnDied()
        {

            if (Props.resurrectionEffect)
            {
                Map map = this.parent.pawn.Corpse.Map;
                if (map != null)
                {
                    SoundDefOf.PsychicPulseGlobal.PlayOneShot(new TargetInfo(this.parent.pawn.Corpse.Position, this.parent.pawn.Corpse.Map, false));
                    MoteMaker.MakeAttachedOverlay(this.parent.pawn.Corpse, ThingDef.Named("Mote_PsycastPsychicEffect"), Vector3.zero, 1f, -1f);
                    ResurrectionUtility.Resurrect(this.parent.pawn.Corpse.InnerPawn);

                }
            }


        }

    }
}
