

using RimWorld;
using Verse;

namespace VEF.AnimalBehaviours
{
    public class HediffComp_ExplodeOnDowned : HediffComp
    {
        public int checkEveryTicks = 60;

        public HediffCompProperties_ExplodeOnDowned Props
        {
            get
            {
                return (HediffCompProperties_ExplodeOnDowned)this.props;
            }
        }


        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);
            if (Pawn.IsHashIntervalTick(checkEveryTicks, delta))
            {
                if (this.parent.pawn.Downed)
                {
                    if (!this.parent.pawn.health.hediffSet.HasHediff(HediffDefOf.Anesthetic))
                    {
                        this.parent.pawn.Kill(null);
                    }

                }
            }

        }
    }
}

