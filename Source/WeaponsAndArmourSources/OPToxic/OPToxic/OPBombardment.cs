using System;
using System.Linq;
using RimWorld;
using Verse;

namespace OPToxic
{
	public class OPBombardment : OrbitalStrike
	{
		public override void StartStrike()
		{
			base.StartStrike();
		}

		public override void Tick()
		{
			base.Tick();
			if (!base.Destroyed)
			{
				if (Find.TickManager.TicksGame % 28 == 0)
				{
					this.CreateRandomExplosion();
				}
				if (Find.TickManager.TicksGame % 30 == 0)
				{
					this.StartRandomFire();
				}
			}
		}

		private void CreateRandomExplosion()
		{
			ThingDef def = this.def;
			int num = OPBombDefGetValue.OPBombGetDmg(def);
			if (num < 1)
			{
				num = 1;
			}
			if (num > 99)
			{
				num = 99;
			}
			int num2 = OPBombDefGetValue.OPBombGetImpactRadius(def);
			if (num2 < 1)
			{
				num2 = 1;
			}
			if (num2 > 30)
			{
				num2 = 30;
			}
			int num3 = OPBombDefGetValue.OPBombGetBlastMinRadius(def);
			int num4 = OPBombDefGetValue.OPBombGetBlastMaxRadius(def);
			if (num4 > 10)
			{
				num4 = 10;
			}
			if (num4 < 1)
			{
				num4 = 1;
			}
			if (num3 > num4)
			{
				num3 = num4;
			}
			if (num3 < 1)
			{
				num3 = 1;
			}
			IntVec3 intVec = (from x in GenRadial.RadialCellsAround(base.Position, (float)num2, true)
							  where x.InBounds(base.Map)
							  select x).RandomElementByWeight((IntVec3 x) => OPBombardment.DistanceChanceFactor.Evaluate(x.DistanceTo(base.Position)));
			float num5 = (float)Rand.Range(num3, num4);
			Map map = base.Map;
			float num6 = num5;
			DamageDef bomb = DamageDefOf.Bomb;
			Thing instigator = this.instigator;
			ThingDef weaponDef = this.weaponDef;
			GenExplosion.DoExplosion(intVec, map, num6, bomb, instigator, num, -1f, null, weaponDef, def, null, null, 0f, 1, false, null, 0f, 1, 0f, false);
		}

		private void StartRandomFire()
		{
			int num = OPBombDefGetValue.OPBombGetImpactRadius(this.def) + 2;
			FireUtility.TryStartFireIn((from x in GenRadial.RadialCellsAround(base.Position, (float)num, true)
										where x.InBounds(base.Map)
										select x).RandomElementByWeight((IntVec3 x) => OPBombardment.DistanceChanceFactor.Evaluate(x.DistanceTo(base.Position))), base.Map, Rand.Range(0.1f, 0.925f));
		}

		private const int ImpactAreaRadius = 12;

		private const int ExplosionRadiusMin = 4;

		private const int ExplosionRadiusMax = 6;

		public const int EffectiveRadius = 13;

		public const int RandomFireRadius = 15;

		private const int BombIntervalTicks = 28;

		private const int StartRandomFireEveryTicks = 30;

		private static readonly SimpleCurve DistanceChanceFactor = new SimpleCurve
		{
			{
				new CurvePoint(0f, 1f),
				true
			},
			{
				new CurvePoint(30f, 0.1f),
				true
			}
		};
	}
}
