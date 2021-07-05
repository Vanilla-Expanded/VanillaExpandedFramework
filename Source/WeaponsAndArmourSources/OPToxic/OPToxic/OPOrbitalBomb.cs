using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace OPToxic
{
	public class OPOrbitalBomb : Gas
	{
		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, true);
			this.destroyTick = Find.TickManager.TicksGame + this.def.gas.expireSeconds.RandomInRange.SecondsToTicks();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.destroyTick, "destroyTick", 0, false);
		}

		public override void Tick()
		{
			if (this.destroyTick <= Find.TickManager.TicksGame)
			{
				this.Destroy(DestroyMode.Vanish);
			}
			this.graphicRotation += this.graphicRotationSpeed;
			if (!this.DestroyedOrNull())
			{
				Map map = base.Map;
				IntVec3 position = base.Position;
				if (Find.TickManager.TicksGame % 10 == 0)
				{
					FleckMaker.ThrowSmoke(GenThing.TrueCenter(this) + new Vector3(0f, 0f, 0.1f), map, 1f);
				}
				if (Find.TickManager.TicksGame % 300 == 0)
				{
					OPBombardment opbombardment = (OPBombardment)GenSpawn.Spawn(DefDatabase<ThingDef>.GetNamed("OPBombardment", true), position, map, WipeMode.Vanish);
					opbombardment.duration = 120;
					opbombardment.instigator = this;
					opbombardment.weaponDef = ThingDefOf.OrbitalTargeterBombardment;
					opbombardment.StartStrike();
					if (!this.DestroyedOrNull())
					{
						this.Destroy(DestroyMode.Vanish);
					}
				}
			}
		}
	}
}
