using Verse;

namespace VEF.Weapons
{
    public class Projectile_FlameThrower : Projectile_Explosive
    {
        private int TicksforAppearence = 3;

        protected override void Tick()
        {
            base.Tick();
            TicksforAppearence--;
            if (TicksforAppearence == 0 && Map != null)
            {
                SmokeMaker.ThrowSmokeTrail(Position.ToVector3Shifted(), 0.7f, Map, "Mote_Firetrail");
                TicksforAppearence = 6;
            }
        }
    }
}