using RimWorld;

namespace ExplosiveTrailsEffect
{
    public class Projectile_DoomsdaySmokeReleaser : Projectile_DoomsdayRocket
    {
        private int TicksforAppearence = 3;

        private bool JustStarted = true;

        public override void Tick()
        {
            base.Tick();
            TicksforAppearence--;

            if (TicksforAppearence == 0 & Map != null)
            {
                SmokeThrowher.ThrowSmokeTrail(Position.ToVector3Shifted(), 2.5f, Map, "Mote_Smoketrail");
                TicksforAppearence = 3;
            }
            else if (JustStarted & Map != null)
            {
                JustStarted = false;
                for (int i = 0; i < 4; i++)
                {
                    SmokeThrowher.ThrowSmokeTrail(Position.ToVector3Shifted(), 4f, Map, "Mote_Smoketrail");
                }
            }
            
            if (Map != null)
            {
                SmokeThrowher.ThrowSmokeTrail(Position.ToVector3Shifted(), 4f, Map, "Mote_Firetrail");
            }
        }
    }
}