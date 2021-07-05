using Verse;

namespace ExplosiveTrailsEffect
{
    public class SmokeGrenade : Projectile_Explosive
    {
        private int Burnticks = 5;

        public override void Tick()
        {
            base.Tick();
            Burnticks--;
            if (Burnticks == 0 & Map != null)
            {
                SmokeThrowher.ThrowSmokeTrail(Position.ToVector3Shifted(), 0.3f, Map, "Mote_Smoketrail");
                Burnticks = 5;
            }
        }
    }
}