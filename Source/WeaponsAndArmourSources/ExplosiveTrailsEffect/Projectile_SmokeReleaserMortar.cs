using Verse;

namespace ExplosiveTrailsEffect
{
    public class Projectile_SmokeReleaserMortar : Projectile_Explosive
    {
        private int Burnticks = 3;

        private bool JustStarted = true;

        public override void Tick()
        {
            base.Tick();
            Burnticks--;

            if (Burnticks == 0 & Map != null)
            {
                SmokeThrowher.ThrowSmokeTrail(Position.ToVector3Shifted(), 0.3f, Map, "Mote_Smoketrail");
                Burnticks = 3;
            }
            else if (JustStarted & Map != null)
            {
                JustStarted = false;
                SmokeThrowher.ThrowSmokeTrail(Position.ToVector3Shifted(), 4f, Map, "Mote_Smoketrail");
            }
        }
    }
}