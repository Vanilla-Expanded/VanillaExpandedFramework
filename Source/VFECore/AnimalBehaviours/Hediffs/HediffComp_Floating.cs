
using Verse;
using RimWorld;

namespace AnimalBehaviours
{
    class HediffComp_Floating : HediffComp
    {

        public int tickCounter = 0;

        public HediffCompProperties_Floating Props
        {
            get
            {
                return (HediffCompProperties_Floating)this.props;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            tickCounter++;
            if (tickCounter > Props.checkingInterval)
            {
               
                AnimalCollectionClass.AddFloatingAnimalToList(this.parent.pawn);
                tickCounter = 0;
            }
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
           
            AnimalCollectionClass.AddFloatingAnimalToList(this.parent.pawn);
           
        }

        public override void CompPostPostRemoved()
        {
            AnimalCollectionClass.RemoveFloatingAnimalFromList(this.parent.pawn);
           
        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            AnimalCollectionClass.RemoveFloatingAnimalFromList(this.parent.pawn);
            
        }

        public override void Notify_PawnKilled()
        {
            AnimalCollectionClass.RemoveFloatingAnimalFromList(this.parent.pawn);
            
        }
    }
}
