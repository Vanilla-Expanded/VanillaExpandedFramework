using Verse;

namespace VWEMakeshift
{
    public class Verb_MakeshiftShoot : Verb_ShootWithSmoke
    {
        protected override int ShotsPerBurst
        {
            get
            {
                var projectile = this.GetProjectile();
                var equipmentDef = EquipmentSource?.def;
                if (equipmentDef is null)
                {
                    Log.Error($"Unable to retrieve weapon def from <color=teal>{GetType()}</color>. Please report to Oskar or Smash Phil.");
                    return base.ShotsPerBurst;
                }

                var makeshiftProps = equipmentDef.GetModExtension<MakeshiftProperties>();
                if (makeshiftProps is null)
                {
                    Log.ErrorOnce($"<color=teal>{GetType()}</color> cannot be used without <color=teal>MakeshiftProperties</color> DefModExtension. Motes will not be thrown.", Gen.HashCombine(projectile.GetHashCode(), "MakeshiftProperties".GetHashCode()));
                    return base.ShotsPerBurst;
                }

                var shots = makeshiftProps.shots.RandomInRange;
                return shots;
            }
        }

        public override bool TryStartCastOn(LocalTargetInfo castTarg, LocalTargetInfo destTarg, bool surpriseAttack = false, bool canHitNonTargetPawns = true, bool preventFriendlyFire = false, bool nonInterruptingSelfCast = false)
        {
            if (ShotsPerBurst <= 0)
            {
                ticksToNextBurstShot = verbProps.ticksBetweenBurstShots;
                if (CasterIsPawn && !verbProps.nonInterruptingSelfCast) CasterPawn.stances.SetStance(new Stance_Cooldown(verbProps.ticksBetweenBurstShots + 1, currentTarget, this));

                return false;
            }

            return base.TryStartCastOn(castTarg, destTarg, surpriseAttack, canHitNonTargetPawns, preventFriendlyFire, nonInterruptingSelfCast);
        }
    }
}
