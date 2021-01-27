using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
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

		public bool   blockRangedAttack = true;
		public bool   blockMeleeAttack  = false;
		public string shieldTexPath;
		public bool   showWhenDrafted;
		public bool   showOnHostiles = true;
		public bool   showOnNeutralInCombat;
		public float  minShieldSize = 1.5f;
		public float  maxShieldSize = 2f;
		public Color  shieldColor = Color.white;
		public float  EnergyLossPerDamage = 0.033f;
		public bool disableRotation;
	}

	[StaticConstructorOnStartup]
	public class ShieldMechBubble : ThingComp
	{
		protected float energy;

		protected int ticksToReset = -1;

		private int lastKeepDisplayTick = -9999;

		private Vector3 impactAngleVect;

		private int lastAbsorbDamageTick = -9999;

		private const float MinDrawSize = 1.2f;

		private const float MaxDrawSize = 1.55f;

		private const float MaxDamagedJitterDist = 0.05f;

		private const int JitterDurationTicks = 8;

		private int StartingTicksToReset = 3200;

		private float EnergyOnReset = 0.2f;

		private int KeepDisplayingTicks = 1000;

		public Pawn Pawn
		{
			get
			{
				if (this.parent is Pawn pawn)
				{
					return pawn;
				}
				else if (this.parent is Apparel apparel && apparel.Wearer != null)
				{
					return apparel.Wearer;
				}

				return null;
			}
		}

		private Material bubbleMat;


		private Material BubbleMat
		{
			get
			{
				if (bubbleMat is null)
				{
					if (Props.shieldTexPath.NullOrEmpty())
					{
						bubbleMat = MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent, Props.shieldColor);
					}
					else
					{
						bubbleMat = MaterialPool.MatFrom(Props.shieldTexPath, ShaderDatabase.Transparent, Props.shieldColor);
					}
				}

				return bubbleMat;
			}
		}

		public CompProperties_ShieldMechBubble Props
		{
			get { return (CompProperties_ShieldMechBubble) this.props; }
		}

		protected virtual float EnergyMax => Props.EnergyShieldEnergyMax;

		protected virtual float EnergyGainPerTick => Props.EnergyShieldRechargeRate / 60f;

		protected virtual float EnergyLossPerDamage => Props.EnergyLossPerDamage;

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
			Scribe_Values.Look(ref energy,              "energy",              0f);
			Scribe_Values.Look(ref ticksToReset,        "ticksToReset",        -1);
			Scribe_Values.Look(ref lastKeepDisplayTick, "lastKeepDisplayTick", 0);
		}

		public override void CompTick()
		{
			base.CompTick();
			if (this.Pawn == null)
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

		public static HashSet<JobDef> combatJobs = new HashSet<JobDef>
													{
														JobDefOf.AttackMelee,
														JobDefOf.AttackStatic,
														JobDefOf.FleeAndCower,
														JobDefOf.ManTurret,
														JobDefOf.Wait_Combat,
														JobDefOf.Flee
													};

		private bool InCombat(Pawn pawn)
		{
			if (combatJobs.Contains(pawn.CurJobDef))
			{
				return true;
			}
			else if (pawn.mindState.duty?.def.alwaysShowWeapon ?? false)
			{
				return true;
			}
			else if (pawn.CurJobDef?.alwaysShowWeapon ?? false)
			{
				return true;
			}

			return false;
		}

		public void DrawWornExtras()
		{
			if (ShieldState == ShieldState.Active)
			{
				float num  = Mathf.Lerp(Props.minShieldSize, Props.maxShieldSize, energy);
				var   pawn = this.Pawn;
				if (pawn != null &&
					(Props.showWhenDrafted && pawn.Drafted
				|| (Props.showOnHostiles        && pawn.Faction != Faction.OfPlayer && pawn.HostileTo(Faction.OfPlayer))
				|| (Props.showOnNeutralInCombat && pawn.Faction != Faction.OfPlayer && !pawn.HostileTo(Faction.OfPlayer) && InCombat(pawn))))
				{
					Vector3 drawPos = pawn.Drawer.DrawPos;
					drawPos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
					int num2 = Find.TickManager.TicksGame - lastAbsorbDamageTick;
					if (num2 < 8)
					{
						float num3 = (float) (8 - num2) / 8f * 0.05f;
						drawPos += impactAngleVect * num3;
						num     -= num3;
					}

					float     angle  = Props.disableRotation ? 0 : Rand.Range(0, 360);
					Vector3   s      = new Vector3(num, 1f, num);
					Matrix4x4 matrix = default(Matrix4x4);
					matrix.SetTRS(drawPos, Quaternion.AngleAxis(angle, Vector3.up), s);
					Graphics.DrawMesh(MeshPool.plane10, matrix, BubbleMat, 0);
				}
			}
		}

		public override void PostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
		{
			base.PostPreApplyDamage(dinfo, out absorbed);
			if (ShieldState != ShieldState.Active)
			{
				absorbed = false;
			}
			else if (dinfo.Def == DamageDefOf.EMP)
			{
				energy = 0f;
				Break();
				absorbed = false;
			}
			else if (Props.blockRangedAttack && dinfo.Def.isRanged || dinfo.Def.isExplosive || Props.blockMeleeAttack
				&& (dinfo.Weapon == null && dinfo.Instigator is Pawn || dinfo.Weapon.IsMeleeWeapon))
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
			SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(this.Pawn.Position, this.Pawn.Map));
			impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
			Vector3 loc = this.Pawn.TrueCenter() + impactAngleVect.RotatedBy(180f) * 0.5f;
			float   num = Mathf.Min(10f, 2f + dinfo.Amount / 10f);
			MoteMaker.MakeStaticMote(loc, this.Pawn.Map, ThingDefOf.Mote_ExplosionFlash, num);
			int num2 = (int) num;
			for (int i = 0; i < num2; i++)
			{
				MoteMaker.ThrowDustPuff(loc, this.Pawn.Map, Rand.Range(0.8f, 1.2f));
			}

			lastAbsorbDamageTick = Find.TickManager.TicksGame;
			KeepDisplaying();
		}

		protected virtual void Break()
		{
			if (this.Pawn?.Map != null && this.Pawn.Position.InBounds(this.Pawn.Map))
			{
				SoundDefOf.EnergyShield_Broken.PlayOneShot(new TargetInfo(this.Pawn.Position, this.Pawn.Map));
				MoteMaker.MakeStaticMote(this.Pawn.TrueCenter(), this.Pawn.Map, ThingDefOf.Mote_ExplosionFlash, 12f);
				for (int i = 0; i < 6; i++)
				{
					MoteMaker.ThrowDustPuff(this.Pawn.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f), this.Pawn.Map, Rand.Range(0.8f, 1.2f));
				}
			}

			energy       = 0f;
			ticksToReset = StartingTicksToReset;
		}

		protected virtual void Reset()
		{
			if (this.Pawn.Spawned)
			{
				SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(this.Pawn.Position, this.Pawn.Map));
				MoteMaker.ThrowLightningGlow(this.Pawn.TrueCenter(), this.Pawn.Map, 3f);
			}

			ticksToReset = -1;
			energy       = EnergyOnReset;
		}
	}
}