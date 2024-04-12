
using Verse;
using RimWorld;

namespace AnimalBehaviours
{
    class HediffComp_Waterstriding : HediffComp
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

                AnimalCollectionClass.AddWaterstridingPawnToList(this.parent.pawn);
                tickCounter = 0;
            }
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {

            AnimalCollectionClass.AddWaterstridingPawnToList(this.parent.pawn);

        }

        public override void CompPostPostRemoved()
        {
            AnimalCollectionClass.RemoveWaterstridingPawnFromList(this.parent.pawn);

        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            AnimalCollectionClass.RemoveWaterstridingPawnFromList(this.parent.pawn);

        }

        public override void Notify_PawnKilled()
        {
            AnimalCollectionClass.RemoveWaterstridingPawnFromList(this.parent.pawn);

        }
    }
}
