using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace AnimalBehaviours
{
    public class CompFixedGender : ThingComp
    {
        private bool changeGenderOnce = true;

        public void ExposeData()
        {
            Scribe_Values.Look<bool>(ref this.changeGenderOnce, "changeGenderOnce", true, false);
        }

        public CompProperties_FixedGender Props
        {
            get
            {
                return (CompProperties_FixedGender)this.props;
            }
        }


        public override void CompTick()
        {
            base.CompTick();
            //This flag is used (and saved) so that the animal only changes its gender when spawned
            if (changeGenderOnce)
            {
                if (this.parent.Map != null)
                {
                    Pawn pawn = this.parent as Pawn;
                    pawn.gender = Props.gender;
                    changeGenderOnce = false;
                }
            }
        }
    }
}

