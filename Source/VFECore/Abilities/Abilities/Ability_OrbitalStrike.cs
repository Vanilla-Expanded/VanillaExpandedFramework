using RimWorld;
using Verse;

namespace VFECore.Abilities
{
    using RimWorld.Planet;

    public class Ability_OrbitalStrike : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);

            foreach (GlobalTargetInfo target in targets)
            {
                var strike = (OrbitalStrike)GenSpawn.Spawn(def.GetModExtension<AbilityExtension_Projectile>().projectile, target.Cell, pawn.Map);
                strike.duration = GetDurationForPawn();
                strike.instigator = pawn;
                strike.StartStrike();
            }
        }
    }
}
