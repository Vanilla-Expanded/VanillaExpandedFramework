using Verse;

namespace ExplosiveTrailsEffect
{
    public class Projectile_FireTrail : Projectile_Explosive
    {
        private int TicksforAppearence = 3;

        public override void Tick()
        {
            base.Tick();
            TicksforAppearence--;
            if (TicksforAppearence == 0 && Map != null)
            {
                SmokeThrowher.ThrowSmokeTrail(Position.ToVector3Shifted(), 0.5f, Map, "Mote_Firetrail");
                TicksforAppearence = 3;
            }
        }
    }
}