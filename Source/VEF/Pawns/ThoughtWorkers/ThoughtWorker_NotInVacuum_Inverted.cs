using RimWorld;
using VEF.AnimalBehaviours;
using Verse;
namespace VEF.Pawns
{
    public class ThoughtWorker_NotInVacuum_Inverted : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {

            if (p.Position != IntVec3.Invalid && p.Map?.BiomeAt(p.Position)?.inVacuum == false && p.VacuumResistanceFromArmor() < 0.8f)
            {
                return true;
            }
            return ThoughtState.Inactive;
        }
    }
}
