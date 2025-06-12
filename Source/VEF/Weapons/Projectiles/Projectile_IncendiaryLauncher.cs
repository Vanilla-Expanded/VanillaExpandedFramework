using Verse;

namespace VEF.Weapons
{
    public class Projectile_IncendiaryLauncher : Projectile_Explosive
    {
        private int TicksforAppearence = 3;

        private bool JustStarted = true;

        protected override void Tick()
        {
            base.Tick();
            TicksforAppearence--;
            if (TicksforAppearence == 0 & Map != null)
            {
                SmokeMaker.ThrowSmokeTrail(base.Position.ToVector3Shifted(), 1f, base.Map, "Mote_Firetrail");
                TicksforAppearence = 5;
            }
            else if (JustStarted & base.Map != null)
            {
                JustStarted = false;
                SmokeMaker.ThrowSmokeTrail(base.Position.ToVector3Shifted(), 1f, base.Map, "Mote_Firetrail");
            }

            if (base.Map != null)
            {
                SmokeMaker.ThrowSmokeTrail(Position.ToVector3Shifted(), 4f, Map, "Mote_Firetrail");
            }
        }
    }
}