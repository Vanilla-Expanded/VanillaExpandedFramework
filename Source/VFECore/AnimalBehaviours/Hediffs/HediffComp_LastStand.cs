
using Verse;
using RimWorld;

namespace AnimalBehaviours
{
    class HediffComp_LastStand : HediffComp
    {

        public int tickCounter = 0;

        public HediffCompProperties_LastStand Props
        {
            get
            {
                return (HediffCompProperties_LastStand)this.props;
            }
        }


      


       

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {

            AnimalCollectionClass.AddLastStandAnimalToList(this.parent.pawn, Props.finalCoolDownMultiplier);

        }

        public override void CompPostPostRemoved()
        {
            AnimalCollectionClass.RemoveLastStandAnimalFromList(this.parent.pawn);

        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            AnimalCollectionClass.RemoveLastStandAnimalFromList(this.parent.pawn);

        }

        public override void Notify_PawnKilled()
        {
            AnimalCollectionClass.RemoveLastStandAnimalFromList(this.parent.pawn);

        }
    }
}
