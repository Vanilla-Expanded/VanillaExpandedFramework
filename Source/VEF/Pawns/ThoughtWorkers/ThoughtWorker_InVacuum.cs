using RimWorld;
using Verse;
namespace VEF.Pawns
{
    public class ThoughtWorker_InVacuum : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {

            if (p.Map?.BiomeAt(p.Position)?.inVacuum == true)
            {
                return true;
            }
            return ThoughtState.Inactive;
        }
    }
}
