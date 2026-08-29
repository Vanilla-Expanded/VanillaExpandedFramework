
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
                return (HediffCompProperties_Floating)props;
            }
        }

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            if (Pawn.IsHashIntervalTick(Props.checkingInterval, delta))
            {
                if (!Props.inSpace)
                {
                    StaticCollectionsClass.AddFloatingAnimalToList(parent.pawn);
                }
                else
                {
                    if (parent.pawn.Position != IntVec3.Invalid && parent.pawn.Map?.BiomeAt(parent.pawn.Position)?.inVacuum == true)
                    {
                        StaticCollectionsClass.AddFloatingAnimalToList(parent.pawn);
                    }
                    else StaticCollectionsClass.RemoveFloatingAnimalFromList(parent.pawn);
                }           
            }
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {           
            StaticCollectionsClass.AddFloatingAnimalToList(parent.pawn);           
        }

        public override void CompPostPostRemoved()
        {
            StaticCollectionsClass.RemoveFloatingAnimalFromList(parent.pawn);           
        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            StaticCollectionsClass.RemoveFloatingAnimalFromList(parent.pawn);           
        }

        public override void Notify_PawnKilled()
        {
            StaticCollectionsClass.RemoveFloatingAnimalFromList(parent.pawn);            
        }
    }
}
