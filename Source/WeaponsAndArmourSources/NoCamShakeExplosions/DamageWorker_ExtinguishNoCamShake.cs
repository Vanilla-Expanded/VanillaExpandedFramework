using RimWorld;
using System.Collections.Generic;
using Verse;

namespace NoCamShakeExplosions
{
    public class DamageWorker_ExtinguishNoCamShake : DamageWorker
    {
        public override DamageResult Apply(DamageInfo dinfo, Thing victim)
        {
            DamageResult result;
            if (!(victim is Fire fire) || fire.Destroyed)
            {
                result = new DamageResult();
            }
            else
            {
                base.Apply(dinfo, victim);
                fire.fireSize -= dinfo.Amount * 0.01f;
                if (fire.fireSize <= 0.1f)
                {
                    fire.Destroy(DestroyMode.Vanish);
                }
                result = new DamageResult();
            }
            return result;
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