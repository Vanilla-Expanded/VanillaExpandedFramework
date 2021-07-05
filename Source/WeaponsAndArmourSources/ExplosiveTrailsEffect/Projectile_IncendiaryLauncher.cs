using Verse;

namespace ExplosiveTrailsEffect
{
    public class Projectile_IncendiaryLauncher : Projectile_Explosive
    {
        private int TicksforAppearence = 3;

        private bool JustStarted = true;

        public override void Tick()
        {
            base.Tick();
            TicksforAppearence--;
            if (TicksforAppearence == 0 & Map != null)
            {
                SmokeThrowher.ThrowSmokeTrail(base.Position.ToVector3Shifted(), 1f, base.Map, "Mote_Firetrail");
                TicksforAppearence = 5;
            }
            else if (JustStarted & base.Map != null)
            {
                JustStarted = false;
                SmokeThrowher.ThrowSmokeTrail(base.Position.ToVector3Shifted(), 1f, base.Map, "Mote_Firetrail");
            }

            if (base.Map != null)
            {
                SmokeThrowher.ThrowSmokeTrail(Position.ToVector3Shifted(), 4f, Map, "Mote_Firetrail");
            }
        }
    }
}