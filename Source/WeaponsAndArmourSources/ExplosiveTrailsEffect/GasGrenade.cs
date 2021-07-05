using Verse;

namespace ExplosiveTrailsEffect
{
    public class GasGrenade : Projectile_Explosive
    {
        private int TicksforAppearence = 5;

        public override void Tick()
        {
            base.Tick();
            TicksforAppearence--;
            if (TicksforAppearence == 0 & Map != null)
            {
                SmokeThrowher.ThrowSmokeTrail(Position.ToVector3Shifted(), 0.3f, Map, "Mote_GreenSmoketrail");
                TicksforAppearence = 5;
            }
        }
    }
}