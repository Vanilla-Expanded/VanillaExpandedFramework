

using RimWorld;
using System.Collections.Generic;
using Verse;
using System.Linq;
using Verse.Sound;
using UnityEngine;

namespace AnimalBehaviours
{
    class HediffComp_Resurrect : HediffComp
    {
        public HediffCompProperties_Resurrect Props
        {
            get
            {
                return (HediffCompProperties_Resurrect)this.props;
            }
        }


        public int resurrectionsLeft = 1;

        public override void CompExposeData()
        {
            Scribe_Values.Look<int>(ref this.resurrectionsLeft, "resurrectionsLeft", 1, false);

        }

        public override void CompPostMake()
        {
            base.CompPostMake();
            resurrectionsLeft = Props.livesLeft;
        }

        public override void Notify_PawnDied()
        {


            Map map = this.parent.pawn.Corpse.Map;
            if (map != null)
            {

                if (resurrectionsLeft > 1)
                {
                    SoundDefOf.PsychicPulseGlobal.PlayOneShot(new TargetInfo(this.parent.pawn.Corpse.Position, this.parent.pawn.Corpse.Map, false));
                    MoteMaker.MakeAttachedOverlay(this.parent.pawn.Corpse, ThingDef.Named("Mote_PsycastPsychicEffect"), Vector3.zero, 1f, -1f);
                    ResurrectionUtility.Resurrect(this.parent.pawn.Corpse.InnerPawn);
                    resurrectionsLeft--;
                }

            }



        }
        public override string CompLabelInBracketsExtra
        {
            get
            {
                return base.CompLabelInBracketsExtra + (resurrectionsLeft.ToString()) + " lives";
            }
        }

    }
}
