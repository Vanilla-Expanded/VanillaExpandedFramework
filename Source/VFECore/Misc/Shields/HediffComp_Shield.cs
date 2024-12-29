using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

// ReSharper disable once CheckNamespace
namespace VFECore.Shields
{
    public class HediffComp_Shield : HediffComp_Draw
    {
        public HediffCompProperties_Shield Props => props as HediffCompProperties_Shield;

        public    float     energy;
        public    bool      useEnergy;
        protected int       ticksTillReset;
        protected Sustainer sustainer;
        protected Vector3   impactAngleVect;

        public virtual bool ShieldActive => energy > 0 || !useEnergy;

        public override void DrawAt(Vector3 drawPos)
        {
            drawPos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            drawPos += Props.graphic.drawOffset;
            Graphics.DrawMesh(MeshPool.plane10,
                Matrix4x4.TRS(drawPos, Quaternion.AngleAxis(Props.doRandomRotation ? Rand.Range(0, 360) : 0f, Vector3.up),
                    new Vector3(Props.graphic.drawSize.x, 1f, Props.graphic.drawSize.y)),
                Graphic.MatSingleFor(Pawn), 0);
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            useEnergy = Props.maxEnergy > 0;
            if (useEnergy)
            {
                if (Props.fullOnAdd) energy = Props.maxEnergy;
                else energy                 = Props.energyPctOnReset * Props.maxEnergy;
            }
            else
                energy = -1f;
        }

        public virtual void PreApplyDamage(ref DamageInfo dinfo, ref bool absorbed)
        {
            if (absorbed) return;
            if (ShieldActive)
            {
                if (Props.breakOn.Contains(dinfo.Def))
                {
                    Break();
                    return;
                }

                var absorb = false;
                switch (Props.absorbAttackType)
                {
                    case AttackType.Melee:
                        absorb = !dinfo.Def.isRanged;
                        break;
                    case AttackType.Ranged:
                        absorb = dinfo.Def.isRanged || dinfo.Def.isExplosive;
                        break;
                    case AttackType.Both:
                        absorb = true;
                        break;
                }

                if (absorb && Props.Absorbs(dinfo.Def) && AbsorbDamage(ref dinfo))
                {
                    absorbed        = true;
                    impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
                    var loc = Pawn.TrueCenter() + impactAngleVect.RotatedBy(180f) * 0.5f;
                    var num = Mathf.Min(10f, 2f + dinfo.Amount / 10f);
                    if (Props.absorbedFleck != null) FleckMaker.Static(loc, Pawn.Map, Props.absorbedFleck, num);
                    if (Props.doDust)
                    {
                        var num2 = (int) num;
                        for (var i = 0; i < num2; i++) FleckMaker.ThrowDustPuff(loc, Pawn.Map, Rand.Range(0.8f, 1.2f));
                    }
                }

                var damage = false;
                switch (Props.damageOnAttack)
                {
                    case AttackType.Melee:
                        damage = !dinfo.Def.isRanged;
                        break;
                    case AttackType.Ranged:
                        damage = dinfo.Def.isRanged || dinfo.Def.isExplosive;
                        break;
                    case AttackType.Both:
                        damage = true;
                        break;
                }

                if (damage && dinfo.Instigator != null) ApplyDamage(dinfo);
            }
        }

        protected virtual void ApplyDamage(DamageInfo dinfo)
        {
            dinfo.Instigator.TakeDamage(new DamageInfo(Props.damageType, Props.damageAmount, Props.armorPenetration));
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref energy,         nameof(energy));
            Scribe_Values.Look(ref useEnergy,      nameof(useEnergy));
            Scribe_Values.Look(ref ticksTillReset, nameof(ticksTillReset));
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (useEnergy)
            {
                if (ticksTillReset > 0)
                {
                    ticksTillReset--;
                    if (ticksTillReset <= 0) Reset();
                }
                else if (energy <= Props.maxEnergy) energy += Props.energyPerTick;
            }

            if (ShieldActive && sustainer is null)
                sustainer = Props.sustainer?.TrySpawnSustainer(Pawn);
            else
                sustainer.Maintain();
        }

        protected virtual void Break()
        {
            energy = 0;

            sustainer.End();
            Props.soundBroken?.PlayOneShot(Pawn);
            if (Props.brokenFleck != null) FleckMaker.Static(Pawn.TrueCenter(), Pawn.Map, Props.brokenFleck, 12f);
            if (Props.doDust)
                for (var i = 0; i < 6; i++)
                    FleckMaker.ThrowDustPuff(
                        Pawn.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f),
                        Pawn.Map, Rand.Range(0.8f, 1.2f));

            ticksTillReset = Props.rechargeDelay;
            if (ticksTillReset <= 0) Reset();
        }

        protected virtual bool AbsorbDamage(ref DamageInfo dinfo)
        {
            if (useEnergy)
            {
                var loss = dinfo.Amount * Props.energyLossPerDamage;
                if (loss < energy)
                {
                    energy -= loss;
                    dinfo.SetAmount(0f);
                    return true;
                }

                Break();
                dinfo.SetAmount(dinfo.Amount - energy / Props.energyLossPerDamage);
                return false;
            }

            dinfo.SetAmount(0f);
            return true;
        }

        protected virtual void Reset()
        {
            ticksTillReset = 0;
            energy         = Props.maxEnergy * Props.energyPctOnReset;

            Props.soundRecharge?.PlayOneShot(Pawn);
            if (Props.doDust) FleckMaker.ThrowLightningGlow(Pawn.TrueCenter(), Pawn.Map, 3f);
        }

        public override void CompPostPostRemoved()
        {
            sustainer.End();
            Props.soundEnded?.PlayOneShot(Pawn);
            base.CompPostPostRemoved();
        }

        public virtual bool AllowVerbCast(Verb verb)
        {
            if (Props.cannotUseAttackType == AttackType.None) return true;
            else if (Props.cannotUseAttackType == AttackType.Both) return false;
            else if (Props.cannotUseAttackType == AttackType.Melee) return verb is not Verb_MeleeAttack;
            else if (Props.cannotUseAttackType == AttackType.Ranged) return verb is not Verb_LaunchProjectile;
            return true;
        }
    }

    public class HediffCompProperties_Shield : HediffCompProperties_Draw
    {
        public List<DamageDef> breakOn;
        public List<DamageDef> absorb;

        public AttackType absorbAttackType    = AttackType.Ranged;
        public AttackType cannotUseAttackType = AttackType.Ranged;

        public float maxEnergy           = -1f;
        public float energyPerTick       = -1f;
        public int   rechargeDelay       = -1;
        public float energyLossPerDamage = 0.033f;
        public bool  fullOnAdd           = true;
        public float energyPctOnReset    = 0.2f;

        public SoundDef sustainer;
        public SoundDef soundBroken;
        public SoundDef soundRecharge;
        public SoundDef soundEnded;

        public FleckDef absorbedFleck;
        public FleckDef brokenFleck;

        public bool doDust = true;

        public AttackType damageOnAttack = AttackType.None;
        public DamageDef  damageType;
        public int        damageAmount     = -1;
        public float      armorPenetration = -1f;

        public bool doRandomRotation = true;

        public override void ResolveReferences(HediffDef parent)
        {
            base.ResolveReferences(parent);
            if (breakOn is null)
                breakOn = maxEnergy > 0
                    ? new List<DamageDef> {DamageDefOf.EMP}
                    : new List<DamageDef>();
            if (graphic is null)
                graphic = new GraphicData
                {
                    graphicClass = typeof(Graphic_Single),
                    texPath      = "Other/ShieldBubble",
                    shaderType   = ShaderTypeDefOf.Transparent
                };
        }

        public bool Absorbs(DamageDef def) => absorb is null || absorb.Contains(def);

        public override void PostLoad()
        {
            base.PostLoad();
            ShieldsSystem.ApplyShieldPatches();
        }
    }

    public enum AttackType
    {
        None,
        Melee,
        Ranged,
        Both
    }
}
