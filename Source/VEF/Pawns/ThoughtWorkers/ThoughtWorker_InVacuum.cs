using RimWorld;
using Verse;
namespace VEF.Pawns
{
    public class ThoughtWorker_InVacuum : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {

            if (p.Position != IntVec3.Invalid && p.Map?.BiomeAt(p.Position)?.inVacuum == true)
            {
                return true;
            }
            return ThoughtState.Inactive;
        }
    }
}
