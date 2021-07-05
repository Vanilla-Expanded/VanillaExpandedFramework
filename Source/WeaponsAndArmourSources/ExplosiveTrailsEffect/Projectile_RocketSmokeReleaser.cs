using Verse;

namespace ExplosiveTrailsEffect
{
    public class Projectile_RocketSmokeReleaser : Projectile_Explosive
    {
        private int TicksforAppearence = 3;

        private bool JustStarted = true;

        public override void Tick()
        {
            base.Tick();
            this.TicksforAppearence--;
            if (TicksforAppearence == 0 & Map != null)
            {
                SmokeThrowher.ThrowSmokeTrail(base.Position.ToVector3Shifted(), 1f, base.Map, "Mote_Smoketrail");
                this.TicksforAppearence = 5;
            }
            else if (JustStarted & Map != null)
            {
                for (int i = 0; i < 6; i++)
                {
                    SmokeThrowher.ThrowSmokeTrail(base.Position.ToVector3Shifted(), 2f, base.Map, "Mote_Smoketrail");
                }
            }

            if (Map != null)
            {
                SmokeThrowher.ThrowSmokeTrail(Position.ToVector3Shifted(), 4f, Map, "Mote_Firetrail");
            }
        }
    }
}