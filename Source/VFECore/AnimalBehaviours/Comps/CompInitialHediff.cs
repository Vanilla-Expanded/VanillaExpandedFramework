using Verse;

namespace AnimalBehaviours
{
    public class CompInitialHediff : ThingComp
    {
        private bool addHediffOnce = true;
     
        public CompProperties_InitialHediff Props
        {
            get
            {
                return (CompProperties_InitialHediff)this.props;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref this.addHediffOnce, "addHediffOnce", true, false);
        }

        public override void CompTickRare()
        {

            base.CompTickRare();

            //addHediffOnce is used (and saved) so the hediff is only added once when the creature spawns
            if (addHediffOnce)
            {
                Pawn pawn = this.parent as Pawn;
                pawn.health.AddHediff(HediffDef.Named(Props.hediffname));               
                addHediffOnce = false;
            }
        }
    }
}

