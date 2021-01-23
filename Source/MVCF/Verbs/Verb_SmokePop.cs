using RimWorld;
using Verse;

namespace MVCF.Verbs
{
    public class Verb_SmokePop : RimWorld.Verb_SmokePop
    {
        protected override bool TryCastShot()
        {
            GenExplosion.DoExplosion(caster.Position, caster.Map,
                caster.GetStatValue(StatDefOf.SmokepopBeltRadius), DamageDefOf.Smoke, null, -1, -1f, null, null,
                null, null, ThingDefOf.Gas_Smoke, 1f, 1, false, null, 0f, 1, 0f, false, null);
            return true;
        }
    }
}