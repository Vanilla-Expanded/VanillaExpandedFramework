using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace OPToxic
{
	public class OPToxicGas : Gas
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
			if (!base.Destroyed && Find.TickManager.TicksGame % OPToxicDefGetValue.OPToxicGetSevUpVal(this.def) == 0)
			{
				Map map = base.Map;
				IntVec3 position = base.Position;
				List<Thing> thingList = position.GetThingList(map);
				if (thingList.Count > 0)
				{
					for (int i = 0; i < thingList.Count; i++)
					{
						if (thingList[i] is Pawn && !(thingList[i] as Pawn).RaceProps.IsMechanoid && thingList[i].Position == position)
						{
							this.DoOPToxicGas(this, thingList[i]);
						}
					}
				}
			}
		}

		public void DoOPToxicGas(Thing Gas, Thing targ)
		{
			Pawn pawn = targ as Pawn;
			if (pawn != null && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Breathing))
			{
				HediffDef namedSilentFail = DefDatabase<HediffDef>.GetNamedSilentFail(OPToxicDefGetValue.OPToxicGetHediff(Gas.def));
				if (namedSilentFail != null)
				{
					Pawn_HealthTracker health = pawn.health;
					Hediff hediff;
					if (health == null)
					{
						hediff = null;
					}
					else
					{
						HediffSet hediffSet = health.hediffSet;
						hediff = ((hediffSet != null) ? hediffSet.GetFirstHediffOfDef(namedSilentFail, false) : null);
					}
					float statValue = pawn.GetStatValue(StatDefOf.ToxicSensitivity, true);
					float num = OPToxicDefGetValue.OPToxicGetSev(Gas.def);
					if (num < 0.01f)
					{
						num = 0.01f;
					}
					float num2 = Rand.Range(0.01f * statValue, num * statValue);
					if (hediff != null && num2 > 0f)
					{
						hediff.Severity += num2;
						return;
					}
					Hediff hediff2 = HediffMaker.MakeHediff(namedSilentFail, pawn, null);
					hediff2.Severity = num2;
					pawn.health.AddHediff(hediff2, null, null, null);
				}
			}
		}
	}
}
