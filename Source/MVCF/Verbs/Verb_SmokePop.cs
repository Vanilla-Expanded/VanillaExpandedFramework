using RimWorld;
using Verse;

namespace MVCF.Verbs;

public class Verb_SmokePop : RimWorld.Verb_SmokePop
{
    protected override bool TryCastShot()
    {
        GenExplosion.DoExplosion(caster.Position, caster.Map, EffectiveRange, DamageDefOf.Smoke, null, -1, -1f, null, null, null, null, null, 0f, 1,
            GasType.BlindSmoke);
        return true;
    }

    public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
    {
        needLOSToCenter = false;
        return EffectiveRange;
    }
}