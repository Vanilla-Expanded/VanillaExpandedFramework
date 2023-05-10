using Verse;

namespace MVCF.VerbComps;

public class VerbComp_ForceUse : VerbComp
{
    public override bool ForceUse(Pawn p, LocalTargetInfo target) => true;
}
