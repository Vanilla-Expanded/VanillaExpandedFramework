
using Verse;
using RimWorld;

namespace VEF.AnimalBehaviours
{
    public class HediffComp_Floating : HediffComp
    {

        public HediffCompProperties_Floating Props
        {
            get
            {
                return (HediffCompProperties_Floating)this.props;
            }
        }

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            if (Pawn.IsHashIntervalTick(Props.checkingInterval, delta))
            {
               
                StaticCollectionsClass.AddFloatingAnimalToList(this.parent.pawn);
            }
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
           
            StaticCollectionsClass.AddFloatingAnimalToList(this.parent.pawn);
           
        }

        public override void CompPostPostRemoved()
        {
            StaticCollectionsClass.RemoveFloatingAnimalFromList(this.parent.pawn);
           
        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            StaticCollectionsClass.RemoveFloatingAnimalFromList(this.parent.pawn);
            
        }

        public override void Notify_PawnKilled()
        {
            StaticCollectionsClass.RemoveFloatingAnimalFromList(this.parent.pawn);
            
        }
    }
}
