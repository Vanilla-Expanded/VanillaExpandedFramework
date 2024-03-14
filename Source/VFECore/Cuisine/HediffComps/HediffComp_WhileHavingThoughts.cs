

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

        //A comp class that keeps a hediff active while a thought (or thoughts) is active on the pawn

        //It also checks if other given thoughts are active on the pawn, and removes them as needed

        public bool flagAmIThinking = false;

        //And for god's sake it only does this every 10 seconds, because if not it would be a true lag fest

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

        public override void CompPostMake()
        {
            base.CompPostMake();
            if (Props.hediffReduction != "")
            {
                if (DefDatabase<HediffDef>.GetNamed(Props.hediffReduction, false) != null)
                {
                    Hediff hediff = this.Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named(Props.hediffReduction), false);
                    if (hediff != null)
                    {
                        hediff.Severity -= Props.reductionAmount;
                    }
                }
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
                    //For each thought defined in the thoughtDefs list
                    foreach (ThoughtDef thoughtDef in this.Props.thoughtDefs)
                    {
                        //Check if the thought is active
                        flagAmIThinking = false;
                        if (this.Pawn.needs.mood.thoughts.memories.GetFirstMemoryOfDef(thoughtDef) != null)
                        {
                            //If it is, the flag goes to true, avoiding deletion of this hediff
                            flagAmIThinking = true;
                            break;
                        }
                    }
                }
                //If I find any of the thoughts in the removeThoughtDefs list, get rid of them! Actually it just sets them to 0, which
                //makes them hidden
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

                //If no active thoughts were found, the flag will be false, so the hediff is deleted and the code doesn't run anymore

                if (!flagAmIThinking)
                {
                    this.Pawn.health.RemoveHediff(this.parent);
                }
                checkingCounter = 0;
            }


        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            //This is just pure laziness, I hooked this code here so Vanilla Cooking Expanded's mechanite resurrector condiment 
            //resurrects the pawn if it dies. But properly this should be a separate class.
            if (Props.resurrectionEffect)
            {
                Map map = this.parent.pawn.Corpse.Map;
                if (map != null)
                {
                    SoundDefOf.PsychicPulseGlobal.PlayOneShot(new TargetInfo(this.parent.pawn.Corpse.Position, this.parent.pawn.Corpse.Map, false));
                    FleckMaker.AttachedOverlay(this.parent.pawn.Corpse, DefDatabase<FleckDef>.GetNamed("PsycastPsychicEffect"), Vector3.zero, 1f, -1f);
                    
                    ResurrectionUtility.TryResurrect(this.parent.pawn.Corpse.InnerPawn);

                }
            }


        }

    }
}
