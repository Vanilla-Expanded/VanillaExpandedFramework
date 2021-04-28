using Verse;
using System.Linq;

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
                //The hediff can be applied to a body part. If not, it will be applied to Whole Body
                if (Props.applyToAGivenBodypart) {
                    BodyPartRecord part = pawn.RaceProps.body.GetPartsWithDef(Props.part).FirstOrDefault();
                    pawn.health.AddHediff(HediffDef.Named(Props.hediffname),part);
                    pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named(Props.hediffname), false).Severity += Props.hediffseverity;

                } else {
                    pawn.health.AddHediff(HediffDef.Named(Props.hediffname));
                    pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named(Props.hediffname), false).Severity += Props.hediffseverity;

                }

                addHediffOnce = false;
            }
        }
    }
}

