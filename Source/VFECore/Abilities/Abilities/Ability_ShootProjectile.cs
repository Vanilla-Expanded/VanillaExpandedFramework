namespace VFECore.Abilities
{
    using Verse;

    public class Ability_ShootProjectile : Ability
    {
        public override void Cast(LocalTargetInfo target)
        {
            base.Cast(target);

            Projectile projectile = GenSpawn.Spawn(this.def.GetModExtension<AbilityExtension_Projectile>().projectile, this.pawn.Position, this.pawn.Map) as Projectile;
            if (projectile is AbilityProjectile abilityProjectile)
            {
                abilityProjectile.ability = this;
            }
            projectile?.Launch(this.pawn, this.pawn.DrawPos, target, target, ProjectileHitFlags.IntendedTarget);
        }

        public override void CheckCastEffects(LocalTargetInfo targetInfo, out bool cast, out bool target, out bool hediffApply)
        {
            base.CheckCastEffects(targetInfo, out cast, out _, out _);
            target      = false;
            hediffApply = false;
        }
    }

    public class AbilityExtension_Projectile : DefModExtension
    {
        public ThingDef projectile;
    }

    public class Ability_ShootProjectile_Snow : Ability_ShootProjectile
    {
        public override void TargetEffects(LocalTargetInfo targetInfo)
        {
            base.TargetEffects(targetInfo);
            SnowUtility.AddSnowRadial(targetInfo.Cell, this.pawn.Map, 3f, 1f);
        }
    }
}
