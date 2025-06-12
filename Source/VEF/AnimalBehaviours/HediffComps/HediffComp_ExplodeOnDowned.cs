

using RimWorld;
using Verse;

namespace VEF.AnimalBehaviours
{
    public class HediffComp_ExplodeOnDowned : HediffComp
    {
        public int checkDownCounter = 0;
        public int checkEveryTicks = 60;

        public HediffCompProperties_ExplodeOnDowned Props
        {
            get
            {
                return (HediffCompProperties_ExplodeOnDowned)this.props;
            }
        }



        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            checkDownCounter++;
            if (checkDownCounter > checkEveryTicks)
            {
                if (this.parent.pawn.Downed)
                {
                    if (!this.parent.pawn.health.hediffSet.HasHediff(HediffDefOf.Anesthetic))
                    {
                        this.parent.pawn.Kill(null);
                    }

                }
                checkDownCounter = 0;
            }

        }
    }
}

