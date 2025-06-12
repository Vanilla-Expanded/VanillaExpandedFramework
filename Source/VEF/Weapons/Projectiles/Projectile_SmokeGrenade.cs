using Verse;

namespace VEF.Weapons
{
    public class Projectile_SmokeGrenade : Projectile_Explosive
    {
        private int Burnticks = 5;

        protected override void Tick()
        {
            base.Tick();
            Burnticks--;
            if (Burnticks == 0 & Map != null)
            {
                SmokeMaker.ThrowSmokeTrail(Position.ToVector3Shifted(), 0.3f, Map, "Mote_Smoketrail");
                Burnticks = 5;
            }
        }
    }
}