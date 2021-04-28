using Verse;
using System.Linq;

namespace AnimalBehaviours
{
    public class CompInitialHediff : ThingComp
    {
        private bool addHediffOnce = true;
        private System.Random rand = new System.Random();
        public int phase = 1;

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
            Scribe_Values.Look<int>(ref this.phase, "phase", 1, false);
        }

        public override void CompTickRare()
        {

            base.CompTickRare();

            //addHediffOnce is used (and saved) so the hediff is only added once when the creature spawns
            if (addHediffOnce)
            {
                Pawn pawn = this.parent as Pawn;

                if (Props.addRandomHediffs)
                {
                    //Select random hediff
                    int randomHediff = rand.Next(1, Props.numberOfHediffs + 1);
                    phase = randomHediff;
                    //The hediff can be applied to a body part. If not, it will be applied to Whole Body
                    if (Props.applyToAGivenBodypart)
                    {
                        BodyPartRecord part = pawn.RaceProps.body.GetPartsWithDef(Props.part).FirstOrDefault();
                        pawn.health.AddHediff(HediffDef.Named(Props.hediffname + randomHediff.ToString()), part);
                        pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named(Props.hediffname + randomHediff.ToString()), false).Severity += Props.hediffseverity;

                    }
                    else
                    {
                        pawn.health.AddHediff(HediffDef.Named(Props.hediffname + randomHediff.ToString()));
                        pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named(Props.hediffname + randomHediff.ToString()), false).Severity += Props.hediffseverity;

                    }


                }
                else
                {

                    //The hediff can be applied to a body part. If not, it will be applied to Whole Body
                    if (Props.applyToAGivenBodypart)
                    {
                        BodyPartRecord part = pawn.RaceProps.body.GetPartsWithDef(Props.part).FirstOrDefault();
                        pawn.health.AddHediff(HediffDef.Named(Props.hediffname), part);
                        pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named(Props.hediffname), false).Severity += Props.hediffseverity;

                    }
                    else
                    {
                        pawn.health.AddHediff(HediffDef.Named(Props.hediffname));
                        pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named(Props.hediffname), false).Severity += Props.hediffseverity;

                    }
                }

                addHediffOnce = false;
            }
        }
    }
}

