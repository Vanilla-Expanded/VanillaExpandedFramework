using RimWorld;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VFECore
{
    [StaticConstructorOnStartup]
    public class Gizmo_EnergyCompShieldStatus : Gizmo
    {
        public CompShieldBubble shield;

        private static readonly Texture2D FullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));

        private static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

        public Gizmo_EnergyCompShieldStatus()
        {
            Order = -100f;
        }

        public override float GetWidth(float maxWidth)
        {
            return 140f;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Rect rect2 = rect.ContractedBy(6f);
            Widgets.DrawWindowBackground(rect);
            Rect rect3 = rect2;
            rect3.height = rect.height / 2f;
            Text.Font = GameFont.Tiny;
            Widgets.Label(rect3, shield.parent.LabelCap);
            Rect rect4 = rect2;
            rect4.yMin = rect2.y + rect2.height / 2f;
            float fillPercent = shield.Energy / shield.EnergyMax;
            Widgets.FillableBar(rect4, fillPercent, FullShieldBarTex, EmptyShieldBarTex, doBorder: false);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect4, shield.Energy.ToString("F0") + " / " + shield.EnergyMax.ToString("F0"));
            Text.Anchor = TextAnchor.UpperLeft;
			if (shield.Props.tooltipKey.NullOrEmpty() is false)
			{
                TooltipHandler.TipRegion(rect2, shield.Props.tooltipKey.Translate());
            }
            return new GizmoResult(GizmoState.Clear);
        }
    }

    public class CompProperties_ShieldBubble : CompProperties
	{
		public CompProperties_ShieldBubble()
		{
			this.compClass = typeof(CompShieldBubble);
		}

		public float EnergyShieldEnergyMax = 0f;
		public float EnergyShieldRechargeRate = 0f;
		public bool chargeFullyWhenMade;
		public float initialChargePct;
		public bool   blockRangedAttack = true;
		public bool   blockMeleeAttack  = false;
		public bool dontAllowRangedAttack = false;
		public bool dontAllowMeleeAttack = false;
		public string shieldTexPath;
		public bool	showWhenDrafted;
		public bool	showAlways;
		public bool   showOnHostiles = true;
		public bool   showOnNeutralInCombat;
		public float  minShieldSize = 1.5f;
		public float  maxShieldSize = 2f;
		public Color  shieldColor = Color.white;
		public float  EnergyLossPerDamage = 1f;
		public bool disableRotation;
		public SoundDef absorbDamageSound;
		public SoundDef brokenSound;
		public SoundDef resetSound;
		public string tooltipKey;
	}

	[StaticConstructorOnStartup]
	public class CompShieldBubble : ThingComp
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

		protected Material bubbleMat;


		protected virtual Material BubbleMat
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

		public CompProperties_ShieldBubble Props
		{
			get { return (CompProperties_ShieldBubble) this.props; }
		}

		public virtual float EnergyMax
        {
            get
            {
                var value = this.Props.EnergyShieldEnergyMax;
                if (Pawn != null)
                {
					value *= Pawn.GetStatValue(VFEDefOf.VEF_EnergyShieldEnergyMaxFactor, true);
					value += Pawn.GetStatValue(VFEDefOf.VEF_EnergyShieldEnergyMaxOffset, true);
				}
				return value;
			}
        }

		protected virtual float EnergyGainPerTick => Props.EnergyShieldRechargeRate / 60f;

		protected virtual float EnergyLossPerDamage => Props.EnergyLossPerDamage;

		public float Energy
        {
            get { return energy; }
            set { energy = Mathf.Clamp(value, 0f, EnergyMax); }
        }

		private bool firstTime = true;
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
			firstTime = false;
			Scribe_Values.Look(ref firstTime, "firstTime", false);
			Scribe_Values.Look(ref energy, "energy", 0f);
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
			else if (firstTime && this.Pawn.Faction != Faction.OfPlayer)
            {
				energy = EnergyMax; // in order to refill shields on NPCs
			}
			if (ShieldState == ShieldState.Resetting)
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

			firstTime = false;
		}

        public bool IsApparel => parent is Apparel;
        private bool IsBuiltIn => !IsApparel;

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
			else if (pawn.mindState?.duty?.def.alwaysShowWeapon ?? false)
			{
				return true;
			}
			else if (pawn.CurJobDef?.alwaysShowWeapon ?? false)
			{
				return true;
			}

			return false;
		}

        public override void CompDrawWornExtras()
        {
            base.CompDrawWornExtras();
            if (IsApparel)
            {
                Draw();
            }
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (IsBuiltIn)
            {
                Draw();
            }
        }

        public void Draw()
		{
			if (ShieldState == ShieldState.Active && Energy > 0)
			{
				var pawn = this.Pawn;
				float num = Mathf.Lerp(Props.minShieldSize, Props.maxShieldSize, energy);
				var props = Props;
				if (pawn != null && (props.showAlways 
						|| props.showWhenDrafted && pawn.Drafted
						|| (props.showOnHostiles && pawn.Faction != Faction.OfPlayer && pawn.HostileTo(Faction.OfPlayer))
						|| (props.showOnNeutralInCombat && pawn.Faction != Faction.OfPlayer && !pawn.HostileTo(Faction.OfPlayer) && InCombat(pawn))))
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

					float     angle  = props.disableRotation ? 0 : Rand.Range(0, 360);
					Vector3   s      = new Vector3(num, 1f, num);
					Matrix4x4 matrix = default(Matrix4x4);
					matrix.SetTRS(drawPos, Quaternion.AngleAxis(angle, Vector3.up), s);
					Graphics.DrawMesh(MeshPool.plane10, matrix, BubbleMat, 0);
				}
			}
		}


        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            base.PostPreApplyDamage(ref dinfo, out absorbed);
            AbsorbingDamage(dinfo, out absorbed);
        }

		public bool AbsorbingDamage(DamageInfo dinfo, out bool absorbed)
		{
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
				&& (dinfo.Weapon == null && dinfo.Instigator is Pawn || (dinfo.Weapon?.IsMeleeWeapon ?? false)))
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
			VFEMech.TeslaProjectile.wasDeflected = absorbed;
            return absorbed;
		}

		public void KeepDisplaying()
		{
			lastKeepDisplayTick = Find.TickManager.TicksGame;
		}
		private void AbsorbedDamage(DamageInfo dinfo)
		{
			if (Props.absorbDamageSound != null)
				Props.absorbDamageSound.PlayOneShot(new TargetInfo(this.Pawn.Position, this.Pawn.Map));
			else
				SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(this.Pawn.Position, this.Pawn.Map));

			impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
			Vector3 loc = this.Pawn.TrueCenter() + impactAngleVect.RotatedBy(180f) * 0.5f;
			float   num = Mathf.Min(10f, 2f + dinfo.Amount / 10f);
			
			FleckMaker.Static(loc, this.Pawn.Map, FleckDefOf.ExplosionFlash, num);
			int num2 = (int) num;
			for (int i = 0; i < num2; i++)
			{
				FleckMaker.ThrowDustPuff(loc, this.Pawn.Map, Rand.Range(0.8f, 1.2f));
			}

			lastAbsorbDamageTick = Find.TickManager.TicksGame;
			KeepDisplaying();
		}

		protected virtual void Break()
		{
			if (this.Pawn?.Map != null && this.Pawn.Position.InBounds(this.Pawn.Map))
			{
				if (Props.brokenSound != null)
					Props.brokenSound.PlayOneShot(new TargetInfo(this.Pawn.Position, this.Pawn.Map));
				else
                    VFEDefOf.EnergyShield_Broken.PlayOneShot(new TargetInfo(this.Pawn.Position, this.Pawn.Map));
				
				FleckMaker.Static(this.Pawn.TrueCenter(), this.Pawn.Map, FleckDefOf.ExplosionFlash, 12f);
				for (int i = 0; i < 6; i++)
				{
					FleckMaker.ThrowDustPuff(this.Pawn.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f), this.Pawn.Map, Rand.Range(0.8f, 1.2f));
				}
			}

			energy       = 0f;
			ticksToReset = StartingTicksToReset;
		}

		protected virtual void Reset()
		{
			if (this.Pawn.Spawned)
			{
				if (Props.resetSound != null)
					Props.resetSound.PlayOneShot(new TargetInfo(this.Pawn.Position, this.Pawn.Map));
				else
					SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(this.Pawn.Position, this.Pawn.Map));

				FleckMaker.ThrowLightningGlow(this.Pawn.TrueCenter(), this.Pawn.Map, 3f);
			}

			ticksToReset = -1;
			energy       = EnergyOnReset;
		}

		public override void PostPostMake()
		{
			base.PostPostMake();
            if (Props.chargeFullyWhenMade)
            {
                Energy = EnergyMax;
            }
			else if (Props.initialChargePct > 0)
			{
				Energy = EnergyMax * Props.initialChargePct;
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (typeof(Abilities.CompAbilities).IsAssignableFrom(this.Props.compClass) is false && this.Pawn != null
                && Find.Selector.SingleSelectedThing == this.Pawn && this.Pawn.Faction == Faction.OfPlayer)
            {
                Gizmo_EnergyCompShieldStatus gizmo_EnergyShieldStatus = new Gizmo_EnergyCompShieldStatus();
                gizmo_EnergyShieldStatus.shield = this;
                yield return gizmo_EnergyShieldStatus;
            }
        }
        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
		{
            if (this.Pawn != null && Find.Selector.SingleSelectedThing == this.Pawn && this.Pawn.IsColonistPlayerControlled)
            {
                Gizmo_EnergyCompShieldStatus gizmo_EnergyShieldStatus = new Gizmo_EnergyCompShieldStatus();
                gizmo_EnergyShieldStatus.shield = this;
                yield return gizmo_EnergyShieldStatus;
                if (!DebugSettings.ShowDevGizmos)
                {
                    yield break;
                }
                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = "DEV: Break";
                command_Action.action = Break;
                yield return command_Action;
                if (ticksToReset > 0)
                {
                    Command_Action command_Action2 = new Command_Action();
                    command_Action2.defaultLabel = "DEV: Clear reset";
                    command_Action2.action = delegate
                    {
                        ticksToReset = 0;
                    };
                    yield return command_Action2;
                }
            }
		}

        public override bool CompAllowVerbCast(Verb verb)
        {
            if (verb.IsMeleeAttack && Props.dontAllowMeleeAttack)
            {
                return false;
            }
            else if (!verb.IsMeleeAttack && Props.dontAllowRangedAttack)
            {
                return false;
            }
            return true;
        }
    }
}