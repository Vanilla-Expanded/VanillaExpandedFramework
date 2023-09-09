namespace VFECore.Abilities
{
    using RimWorld;
    using RimWorld.Planet;
    using System.Collections.Generic;
    using UnityEngine;
    using Verse;

    public class Ability_ShootProjectile : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            foreach (GlobalTargetInfo target in targets)
            {
                ShootProjectile(target);
            }
        }
        protected virtual Projectile ShootProjectile(GlobalTargetInfo target)
        {
            var extension = this.def.GetModExtension<AbilityExtension_Projectile>();
            var origin = pawn.DrawPosHeld ?? pawn.PositionHeld.ToVector3Shifted();
            var source = this.pawn.PositionHeld; 
            
            Projectile projectile = GenSpawn.Spawn(extension.projectile, source, this.pawn.MapHeld) as Projectile;
            if (projectile is AbilityProjectile abilityProjectile)
            {
                abilityProjectile.ability = this;
            }
            if (projectile?.TryGetComp<CompAbilityProjectile>() is {} compAbilityProjectile)
            {
                compAbilityProjectile.ability = this;
            }

            if (extension.forcedMissRadius > 0.5f)
            {
                float forcedMissRadius   = extension.forcedMissRadius;
                float adjustedForcedMiss = VerbUtility.CalculateAdjustedForcedMiss(forcedMissRadius, target.Cell - Caster.Position);
                if (adjustedForcedMiss > 0.5f)
                {

                    int     max              = GenRadial.NumCellsInRadius(forcedMissRadius);
                    IntVec3 forcedMissTarget = target.Cell + GenRadial.RadialPattern[Rand.Range(0, max)];
                    if (forcedMissTarget != target.Cell)
                    {
                        ProjectileHitFlags projectileHitFlags = ProjectileHitFlags.NonTargetWorld;
                        if (Rand.Chance(0.5f)) { projectileHitFlags = ProjectileHitFlags.All; }

                        projectile?.Launch(pawn, origin, forcedMissTarget, target.HasThing ? target.Thing : target.Cell, projectileHitFlags);
                        return projectile;
                    }
                }
            }
            var accuracy = this.CalculateModifiedStatForPawn(1f, extension.accuracyStatFactors, extension.accuracyStatOffsets);
            if (Rand.Chance(accuracy))
            {
                if (target.HasThing)
                    projectile?.Launch(this.pawn, origin, target.Thing, target.Thing, extension.hitFlags);
                else
                    projectile?.Launch(this.pawn, origin, target.Cell, target.Cell, extension.hitFlags);
            }
            else
            {
                ProjectileHitFlags projectileHitFlags = ProjectileHitFlags.NonTargetWorld;
                var cell = ChangeDestToMissWild(target.Cell, source,  accuracy);
                projectile?.Launch(this.pawn, origin, cell, cell, projectileHitFlags);
            }
            return projectile;
        }

        public IntVec3 ChangeDestToMissWild(IntVec3 dest, IntVec3 source, float aimOnChance)
        {
            float num = ShootTuning.MissDistanceFromAimOnChanceCurves.Evaluate(aimOnChance, Rand.Value);
            if (num < 0f)
            {
                Log.ErrorOnce("Attempted to wild-miss less than zero tiles away", 94302089);
            }
            IntVec3 intVec;
            do
            {
                Vector2 unitVector = Rand.UnitVector2;
                Vector3 vector = new Vector3(unitVector.x * num, 0f, unitVector.y * num);
                intVec = (dest.ToVector3Shifted() + vector).ToIntVec3();
            }
            while (Vector3.Dot((dest - source).ToVector3(), (intVec - source).ToVector3()) < 0f);
            return intVec;
        }


        public override void CheckCastEffects(GlobalTargetInfo[] targetInfos, out bool cast, out bool target, out bool hediffApply)
        {
            base.CheckCastEffects(targetInfos, out cast, out _, out _);
            target      = false;
            hediffApply = false;
        }
    }

    public class AbilityExtension_Projectile : DefModExtension
    {
        public ThingDef projectile;
        public SoundDef soundOnImpact;
        public float forcedMissRadius;
        public ProjectileHitFlags hitFlags = ProjectileHitFlags.IntendedTarget;
        public List<StatModifier> accuracyStatFactors = new List<StatModifier>();
        public List<StatModifier> accuracyStatOffsets = new List<StatModifier>();
    }

    public class AbilityExtension_ShootProjectile_Snow : DefModExtension
    {
        public float radius = 3f;
        public float depth = 1f;
    }

    public class Ability_ShootProjectile_Snow : Ability_ShootProjectile
    {
        public override void TargetEffects(params GlobalTargetInfo[] targetInfos)
        {
            base.TargetEffects(targetInfos);
            var snow = this.def.GetModExtension<AbilityExtension_ShootProjectile_Snow>();
            foreach (GlobalTargetInfo targetInfo in targetInfos)
            {
                SnowUtility.AddSnowRadial(targetInfo.Cell, this.pawn.Map, snow?.radius ?? 3f, snow?.depth ?? 1f);
            }
        }
    }
}
