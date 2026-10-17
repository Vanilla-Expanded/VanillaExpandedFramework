using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VEF.Things;

public class DamageWorker_CustomFire : DamageWorker_AddInjury
{
    protected CustomFireDamageExtension Extension => field ??= def.GetModExtension<CustomFireDamageExtension>();

    public override DamageResult Apply(DamageInfo dinfo, Thing victim)
    {
        var pawn = victim as Pawn;
        if (pawn != null && pawn.Faction == Faction.OfPlayer)
            Find.TickManager.slower.SignalForceNormalSpeedShort();

        var map = victim.Map;
        var damageResult = base.Apply(dinfo, victim);
        if (map == null)
            return damageResult;

        if (!damageResult.deflected && !dinfo.InstantPermanentInjury && Rand.Chance(FireUtility.ChanceToAttachFireFromEvent(victim)))
            victim.TryAttachCustomFire(Extension.initialAttachedFireSize.RandomInRange, dinfo.Instigator, Extension.fireDef);

        if (victim.Destroyed && pawn == null && Extension.spawnFilthOnThingDestroyed)
        {
            foreach (var c in victim.OccupiedRect())
                FilthMaker.TryMakeFilth(c, map, Extension.filthOnThingDestroyed);
        }

        return damageResult;
    }

    public override void ExplosionAffectCell(Explosion explosion, IntVec3 c, List<Thing> damagedThings, List<Thing> ignoredThings, bool canThrowMotes)
    {
        base.ExplosionAffectCell(explosion, c, damagedThings, ignoredThings, canThrowMotes);

        if ((Extension.ignoreFireDamageDefForExplosions || def == Extension.fireDef.fireDamageDef) && Rand.Chance(CustomFireUtility.ChanceToStartCustomFireIn(c, explosion.Map, Extension.fireDef)))
            CustomFireUtility.TryStartCustomFireIn(c, explosion.Map, Extension.initialExplosionFireSize.RandomInRange, explosion.instigator, Extension.fireDef);
    }
}