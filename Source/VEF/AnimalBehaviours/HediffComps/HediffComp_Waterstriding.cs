
using Verse;
using RimWorld;

namespace VEF.AnimalBehaviours
{
    public class HediffComp_Waterstriding : HediffComp
    {

        public int tickCounter = 0;

        public HediffCompProperties_Waterstriding Props
        {
            get
            {
                return (HediffCompProperties_Waterstriding)this.props;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            tickCounter++;
            if (tickCounter > Props.checkingInterval)
            {

                StaticCollectionsClass.AddWaterstridingPawnToList(this.parent.pawn);
                tickCounter = 0;
            }
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {

            StaticCollectionsClass.AddWaterstridingPawnToList(this.parent.pawn);

        }

        public override void CompPostPostRemoved()
        {
            StaticCollectionsClass.RemoveWaterstridingPawnFromList(this.parent.pawn);

        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            StaticCollectionsClass.RemoveWaterstridingPawnFromList(this.parent.pawn);

        }

        public override void Notify_PawnKilled()
        {
            StaticCollectionsClass.RemoveWaterstridingPawnFromList(this.parent.pawn);

        }
    }
}
