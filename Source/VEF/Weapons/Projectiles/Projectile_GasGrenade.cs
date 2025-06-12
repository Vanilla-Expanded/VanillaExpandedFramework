using Verse;

namespace VEF.Weapons
{
    public class Projectile_GasGrenade : Projectile_Explosive
    {
        private int TicksforAppearence = 5;

        protected override void Tick()
        {
            base.Tick();
            TicksforAppearence--;
            if (TicksforAppearence == 0 & Map != null)
            {
                SmokeMaker.ThrowSmokeTrail(Position.ToVector3Shifted(), 0.3f, Map, "Mote_GreenSmoketrail");
                TicksforAppearence = 5;
            }
        }
    }
}