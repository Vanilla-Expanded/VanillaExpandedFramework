using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace VFEMech
{

	public class CompProperties_ShieldMechBubble : CompProperties
	{
		public CompProperties_ShieldMechBubble()
		{
			this.compClass = typeof(ShieldMechBubble);
		}

		public float EnergyShieldEnergyMax = 0f;

		public float EnergyShieldRechargeRate = 0f;
	}

	[StaticConstructorOnStartup]
	public class ShieldMechBubble : ThingComp
	{
		private float energy;

		private int ticksToReset = -1;

		private int lastKeepDisplayTick = -9999;

		private Vector3 impactAngleVect;

		private int lastAbsorbDamageTick = -9999;

		private const float MinDrawSize = 1.2f;

		private const float MaxDrawSize = 1.55f;

		private const float MaxDamagedJitterDist = 0.05f;

		private const int JitterDurationTicks = 8;

		private int StartingTicksToReset = 3200;

		private float EnergyOnReset = 0.2f;

		private float EnergyLossPerDamage = 0.033f;

		private int KeepDisplayingTicks = 1000;


		private static readonly Material BubbleMat = MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent);

		public CompProperties_ShieldMechBubble Props
		{
			get
			{
				return (CompProperties_ShieldMechBubble)this.props;
			}
		}
		private float EnergyMax => Props.EnergyShieldEnergyMax;

		private float EnergyGainPerTick => Props.EnergyShieldRechargeRate / 60f;

		public float Energy => energy;

		public ShieldState ShieldState
		{
			get
			{
				if (ticksToReset > 0)
				{
					return ShieldState.Resetting;
				}
				return ShieldState.Active;
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref energy, "energy", 0f);
			Scribe_Values.Look(ref ticksToReset, "ticksToReset", -1);
			Scribe_Values.Look(ref lastKeepDisplayTick, "lastKeepDisplayTick", 0);
		}

		public override void CompTick()
		{
			base.CompTick();
			if (base.parent == null)
			{
				energy = 0f;
			}
			else if (ShieldState == ShieldState.Resetting)
			{
				ticksToReset--;
				if (ticksToReset <= 0)
				{
					Reset();
				}
			}
			else if (ShieldState == ShieldState.Active)
			{
				energy += EnergyGainPerTick;
				if (energy > EnergyMax)
				{
					energy = EnergyMax;
				}
			}
		}

		public override void PostDraw()
		{
			base.PostDraw();
			DrawWornExtras();
		}

		public void DrawWornExtras()
		{
			if (ShieldState == ShieldState.Active)
			{
				float num = Mathf.Lerp(1.5f, 2.0f, energy);
				Pawn pawn = base.parent as Pawn;
				if (pawn != null)
				{
					Vector3 drawPos = pawn.Drawer.DrawPos;
					drawPos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
					int num2 = Find.TickManager.TicksGame - lastAbsorbDamageTick;
					if (num2 < 8)
					{
						float num3 = (float)(8 - num2) / 8f * 0.05f;
						drawPos += impactAngleVect * num3;
						num -= num3;
					}
					float angle = Rand.Range(0, 360);
					Vector3 s = new Vector3(num, 1f, num);
					Matrix4x4 matrix = default(Matrix4x4);
					matrix.SetTRS(drawPos, Quaternion.AngleAxis(angle, Vector3.up), s);
					Graphics.DrawMesh(MeshPool.plane10, matrix, BubbleMat, 0);
				}
			}
		}

		public override void PostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
		{
			base.PostPreApplyDamage(dinfo, out absorbed);
			if (ShieldState != 0)
			{
				absorbed = false;
			}
			else if (dinfo.Def == DamageDefOf.EMP)
			{
				energy = 0f;
				Break();
				absorbed = false;
			}
			else if (dinfo.Def.isRanged || dinfo.Def.isExplosive)
			{
				energy -= dinfo.Amount * EnergyLossPerDamage;
				if (energy < 0f)
				{
					Break();
				}
				else
				{
					AbsorbedDamage(dinfo);
				}
				absorbed = true;
			}
			else
			{
				absorbed = false;
			}
		}

		public void KeepDisplaying()
		{
			lastKeepDisplayTick = Find.TickManager.TicksGame;
		}

		private void AbsorbedDamage(DamageInfo dinfo)
		{
			SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(base.parent.Position, base.parent.Map));
			impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
			Vector3 loc = base.parent.TrueCenter() + impactAngleVect.RotatedBy(180f) * 0.5f;
			float num = Mathf.Min(10f, 2f + dinfo.Amount / 10f);
			MoteMaker.MakeStaticMote(loc, base.parent.Map, ThingDefOf.Mote_ExplosionFlash, num);
			int num2 = (int)num;
			for (int i = 0; i < num2; i++)
			{
				MoteMaker.ThrowDustPuff(loc, base.parent.Map, Rand.Range(0.8f, 1.2f));
			}
			lastAbsorbDamageTick = Find.TickManager.TicksGame;
			KeepDisplaying();
		}

		private void Break()
		{
			SoundDefOf.EnergyShield_Broken.PlayOneShot(new TargetInfo(base.parent.Position, base.parent.Map));
			MoteMaker.MakeStaticMote(base.parent.TrueCenter(), base.parent.Map, ThingDefOf.Mote_ExplosionFlash, 12f);
			for (int i = 0; i < 6; i++)
			{
				MoteMaker.ThrowDustPuff(base.parent.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f), base.parent.Map, Rand.Range(0.8f, 1.2f));
			}
			energy = 0f;
			ticksToReset = StartingTicksToReset;
		}

		private void Reset()
		{
			if (base.parent.Spawned)
			{
				SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(base.parent.Position, base.parent.Map));
				MoteMaker.ThrowLightningGlow(base.parent.TrueCenter(), base.parent.Map, 3f);
			}
			ticksToReset = -1;
			energy = EnergyOnReset;
		}
	}
}
