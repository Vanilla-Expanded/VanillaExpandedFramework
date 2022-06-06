using Verse;
using System.Linq;

namespace AnimalBehaviours
{
    public class CompInitialMentalState : ThingComp
    {
        private bool addStateOnce = true;
      

        public CompProperties_InitialMentalState Props
        {
            get
            {
                return (CompProperties_InitialMentalState)this.props;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref this.addStateOnce, "addStateOnce", true, false);
          
        }

        public override void CompTickRare()
        {

            base.CompTickRare();

            //addStateOnce is used (and saved) so the state is only added once when the creature spawns
            if (addStateOnce)
            {
                Pawn pawn = this.parent as Pawn;

                pawn.mindState.mentalStateHandler.TryStartMentalState(Props.mentalstate, null, true);

                addStateOnce = false;
            }
        }
    }
}

