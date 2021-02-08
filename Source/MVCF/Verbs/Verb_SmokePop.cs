using RimWorld;
using Verse;

namespace MVCF.Verbs
{
    public class Verb_SmokePop : RimWorld.Verb_SmokePop
    {
        public override bool TryCastShot()
        {
            GenExplosion.DoExplosion(caster.Position, caster.Map,
                EffectiveRange, DamageDefOf.Smoke, null, -1, -1f, null, null,
                null, null, ThingDefOf.Gas_Smoke, 1f, 1, false, null, 0f, 1, 0f, false, null);
            return true;
        }

        public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
        {
            needLOSToCenter = false;
            return EffectiveRange;
        }
    }
}