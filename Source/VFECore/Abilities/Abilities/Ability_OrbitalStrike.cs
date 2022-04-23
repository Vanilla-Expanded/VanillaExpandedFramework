using RimWorld;
using Verse;

namespace VFECore.Abilities
{
    public class Ability_OrbitalStrike : Ability
    {
        public override void Cast(LocalTargetInfo target)
        {
            base.Cast(target);

            var strike = (OrbitalStrike) GenSpawn.Spawn(def.GetModExtension<AbilityExtension_Projectile>().projectile, target.Cell, pawn.Map);
            strike.duration   = GetDurationForPawn();
            strike.instigator = pawn;
            strike.StartStrike();
        }
    }
}
