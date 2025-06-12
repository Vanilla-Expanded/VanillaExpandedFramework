
using Verse;
using RimWorld;

namespace VEF.AnimalBehaviours
{
    public class HediffComp_LastStand : HediffComp
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

            StaticCollectionsClass.AddLastStandAnimalToList(this.parent.pawn, Props.finalCoolDownMultiplier);

        }

        public override void CompPostPostRemoved()
        {
            StaticCollectionsClass.RemoveLastStandAnimalFromList(this.parent.pawn);

        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            StaticCollectionsClass.RemoveLastStandAnimalFromList(this.parent.pawn);

        }

        public override void Notify_PawnKilled()
        {
            StaticCollectionsClass.RemoveLastStandAnimalFromList(this.parent.pawn);

        }
    }
}
