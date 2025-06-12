using Verse;

namespace VEF.Weapons
{
    public class Projectile_SmokeReleaserMortar : Projectile_Explosive
    {
        private int Burnticks = 3;

        private bool JustStarted = true;

        protected override void Tick()
        {
            base.Tick();
            Burnticks--;

            if (Burnticks == 0 & Map != null)
            {
                SmokeMaker.ThrowSmokeTrail(Position.ToVector3Shifted(), 0.3f, Map, "Mote_Smoketrail");
                Burnticks = 3;
            }
            else if (JustStarted & Map != null)
            {
                JustStarted = false;
                SmokeMaker.ThrowSmokeTrail(Position.ToVector3Shifted(), 4f, Map, "Mote_Smoketrail");
            }
        }
    }
}