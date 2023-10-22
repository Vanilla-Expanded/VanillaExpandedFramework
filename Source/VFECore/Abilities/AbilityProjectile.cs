using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VFECore.Abilities
{
    using RimWorld.Planet;
    public class AbilityProjectile : Projectile
    {
        public Ability ability;

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            Map map = this.Map;
            base.Impact(hitThing);
            DoImpact(hitThing, map);
        }
        protected virtual void DoImpact(Thing hitThing, Map map)
        {
            BattleLogEntry_RangedImpact battleLogEntryRangedImpact =
                new BattleLogEntry_RangedImpact(launcher, hitThing, intendedTarget.Thing, equipmentDef ?? ability.pawn.def, def, targetCoverDef);
            Find.BattleLog.Add(battleLogEntryRangedImpact);

            this.ability.TargetEffects(new GlobalTargetInfo(this.Position, map));
            var extension = this.ability.def.GetModExtension<AbilityExtension_Projectile>();
            if (extension != null)
            {
                if (extension.soundOnImpact != null)
                {
                    extension.soundOnImpact.PlayOneShot(new TargetInfo(this.Position, map));
                }
            }
            float power = this.ability.GetPowerForPawn();

            if (hitThing != null)
            {
                DamageInfo dinfo = new DamageInfo(this.def.projectile.damageDef, power, float.MaxValue, this.ExactRotation.eulerAngles.y, this.launcher, null, this.equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, this.intendedTarget.Thing);
                hitThing.TakeDamage(dinfo).AssociateWithLog(battleLogEntryRangedImpact);

                if (hitThing is Pawn pawn)
                {
                    this.ability.ApplyHediffs(new GlobalTargetInfo(pawn));

                    if (pawn.stances != null && pawn.BodySize <= this.def.projectile.StoppingPower + 0.001f)
                    {
                        pawn.stances.stagger.StaggerFor(95);
                    }
                }

                if (this.def.projectile.extraDamages != null)
                {
                    foreach (ExtraDamage extraDamage in this.def.projectile.extraDamages)
                    {
                        if (Rand.Chance(extraDamage.chance))
                        {
                            DamageInfo dinfo2 = new DamageInfo(extraDamage.def, extraDamage.amount, extraDamage.AdjustedArmorPenetration(), this.ExactRotation.eulerAngles.y, this.launcher, null,
                                                               this.equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, this.intendedTarget.Thing);
                            hitThing.TakeDamage(dinfo2).AssociateWithLog(battleLogEntryRangedImpact);
                        }
                    }
                }
            }
            else
            {
                SoundDefOf.BulletImpact_Ground.PlayOneShot(new TargetInfo(this.Position, map));
                if (this.Position.GetTerrain(map).takeSplashes)
                {
                    FleckMaker.WaterSplash(this.ExactPosition, map, Mathf.Sqrt(power) * 1f, 4f);
                }
                else
                {
                    FleckMaker.Static(this.ExactPosition, map, FleckDefOf.ShotHit_Dirt);
                }
            }

            if (this.def.projectile.explosionRadius > 0)
            {
                DoExplosion(map, power);
            }
        }

        protected virtual void DoExplosion(Map map, float power)
        {
            if (this.def.projectile.explosionEffect != null)
            {
                Effecter effecter = this.def.projectile.explosionEffect.Spawn();
                effecter.Trigger(new TargetInfo(this.Position, map), new TargetInfo(this.Position, map));
                effecter.Cleanup();
            }
            GenExplosion.DoExplosion(this.Position, map, this.def.projectile.explosionRadius, this.def.projectile.damageDef, this.launcher, Mathf.RoundToInt(power), float.MaxValue,
                this.def.projectile.soundExplode, this.equipmentDef, this.def, this.intendedTarget.Thing, this.def.projectile.postExplosionSpawnThingDef,
                this.def.projectile.postExplosionSpawnChance, this.def.projectile.postExplosionSpawnThingCount,
                preExplosionSpawnThingDef: this.def.projectile.preExplosionSpawnThingDef, preExplosionSpawnChance: this.def.projectile.preExplosionSpawnChance,
                preExplosionSpawnThingCount: this.def.projectile.preExplosionSpawnThingCount,
                applyDamageToExplosionCellsNeighbors: this.def.projectile.applyDamageToExplosionCellsNeighbors,
                chanceToStartFire: this.def.projectile.explosionChanceToStartFire, damageFalloff: this.def.projectile.explosionDamageFalloff,
                direction: this.origin.AngleToFlat(this.destination));
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref ability, nameof(ability));
        }
    }

    public class CompAbilityProjectile : ThingComp
    {
        public Ability ability;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref ability, nameof(ability));
        }
    }
}
