using RimWorld;
using System.Collections.Generic;
using Verse;

namespace NoCamShakeExplosions
{
    public class DamageWorker_FlameNoCamShake : DamageWorker_AddInjury
    {
        public override DamageResult Apply(DamageInfo dinfo, Thing victim)
        {
            Pawn pawn = victim as Pawn;
            if (pawn != null && pawn.Faction == Faction.OfPlayer)
            {
                Find.TickManager.slower.SignalForceNormalSpeedShort();
            }
            Map map = victim.Map;
            DamageResult damageResult = base.Apply(dinfo, victim);
            if (!damageResult.deflected && !dinfo.InstantPermanentInjury)
            {
                victim.TryAttachFire(Rand.Range(0.15f, 0.25f),null);
            }
            if (victim.Destroyed && map != null && pawn == null)
            {
                foreach (IntVec3 c in victim.OccupiedRect())
                {
                    FilthMaker.TryMakeFilth(c, map, ThingDefOf.Filth_Ash, 1, FilthSourceFlags.None);
                }
                if (victim is Plant plant && victim.def.plant.IsTree && victim.def != InternalDefOf.BurnedTree)
                {
                    DeadPlant deadPlant = (DeadPlant)GenSpawn.Spawn(InternalDefOf.BurnedTree, victim.Position, map, WipeMode.Vanish);
                    deadPlant.Growth = plant.Growth;
                }
            }
            return damageResult;
        }

        public override void ExplosionAffectCell(Explosion explosion, IntVec3 c, List<Thing> damagedThings, List<Thing> ignoredThings, bool canThrowMotes)
        {
            base.ExplosionAffectCell(explosion, c, damagedThings, ignoredThings, canThrowMotes);
            if (this.def == DamageDefOf.Flame && Rand.Chance(FireUtility.ChanceToStartFireIn(c, explosion.Map)))
            {
                FireUtility.TryStartFireIn(c, explosion.Map, Rand.Range(0.2f, 0.6f),null);
            }
        }

        public override void ExplosionStart(Explosion explosion, List<IntVec3> cellsToAffect)
        {
            if (this.def.explosionHeatEnergyPerCell > float.Epsilon)
            {
                GenTemperature.PushHeat(explosion.Position, explosion.Map, this.def.explosionHeatEnergyPerCell * (float)cellsToAffect.Count);
            }
            FleckMaker.Static(explosion.Position, explosion.Map, FleckDefOf.ExplosionFlash, explosion.radius * 6f);
        }
    }
}