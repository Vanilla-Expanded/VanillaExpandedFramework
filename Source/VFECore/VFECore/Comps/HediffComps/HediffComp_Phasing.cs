using Verse;

namespace VFECore
{
    public class HediffComp_Phasing : HediffComp
    {
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            PhasingUtils.PhasingPawns.Add(parent.pawn);
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            parent.pawn.pather.TryRecoverFromUnwalkablePosition(false);
            PhasingUtils.PhasingPawns.Remove(parent.pawn);
        }
    }
}