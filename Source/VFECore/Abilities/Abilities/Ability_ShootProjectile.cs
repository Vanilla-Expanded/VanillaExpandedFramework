namespace VFECore.Abilities
{
    using RimWorld.Planet;
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
            Projectile projectile = GenSpawn.Spawn(extension.projectile, this.pawn.Position, this.pawn.Map) as Projectile;
            if (projectile is AbilityProjectile abilityProjectile)
            {
                abilityProjectile.ability = this;
            }
            if (target.HasThing)
                projectile?.Launch(this.pawn, this.pawn.DrawPos, target.Thing, target.Thing, extension.hitFlags);
            else
                projectile?.Launch(this.pawn, this.pawn.DrawPos, target.Cell, target.Cell, extension.hitFlags);
            return projectile;
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
        public ProjectileHitFlags hitFlags = ProjectileHitFlags.IntendedTarget;
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
