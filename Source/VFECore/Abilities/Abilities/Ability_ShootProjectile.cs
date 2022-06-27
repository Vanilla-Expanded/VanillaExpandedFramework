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

                Projectile projectile = GenSpawn.Spawn(this.def.GetModExtension<AbilityExtension_Projectile>().projectile, this.pawn.Position, this.pawn.Map) as Projectile;
                if (projectile is AbilityProjectile abilityProjectile)
                {
                    abilityProjectile.ability = this;
                }
                if(target.HasThing)
                    projectile?.Launch(this.pawn, this.pawn.DrawPos, target.Thing, target.Thing, ProjectileHitFlags.IntendedTarget);
                else
                    projectile?.Launch(this.pawn, this.pawn.DrawPos, target.Cell, target.Cell, ProjectileHitFlags.IntendedTarget);
            }
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
    }

    public class Ability_ShootProjectile_Snow : Ability_ShootProjectile
    {
        public override void TargetEffects(params GlobalTargetInfo[] targetInfos)
        {
            base.TargetEffects(targetInfos);
            foreach (GlobalTargetInfo targetInfo in targetInfos)
            {
                SnowUtility.AddSnowRadial(targetInfo.Cell, this.pawn.Map, 3f, 1f);
            }
        }
    }
}
