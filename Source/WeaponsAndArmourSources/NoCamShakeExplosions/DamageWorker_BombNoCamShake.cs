using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace NoCamShakeExplosions
{
	public class DamageWorker_BombNoCamShake : DamageWorker_AddInjury
	{
		public override void ExplosionStart(Explosion explosion, List<IntVec3> cellsToAffect)
		{
			bool flag = this.def.explosionHeatEnergyPerCell > float.Epsilon;
			bool flag2 = flag;
			if (flag2)
			{
				GenTemperature.PushHeat(explosion.Position, explosion.Map, this.def.explosionHeatEnergyPerCell * (float)cellsToAffect.Count);
			}
			FleckMaker.Static(explosion.Position, explosion.Map, FleckDefOf.ExplosionFlash, explosion.radius * 6f);
		}
	}
}
