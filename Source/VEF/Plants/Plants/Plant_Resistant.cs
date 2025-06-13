using UnityEngine;
using RimWorld;

namespace VEF.Plants
{
    public class Plant_Resistant : Plant
    {

        public override float CurrentDyingDamagePerTick
        {
            get
            {
                if (!Spawned)
                {
                    return 0f;
                }
                float num = 0f;
                if (def.plant.LimitedLifespan && ageInt > def.plant.LifespanTicks)
                {
                    num = Mathf.Max(num, 0.002f);
                }
                if (!def.plant.cavePlant && def.plant.dieIfNoSunlight && unlitTicks > 650000)
                {
                    num = Mathf.Max(num, 0.002f);
                }

                return num;
            }
        }

        public override void TickLong()
        {
            base.TickLong();

            if ((this.HitPoints < this.MaxHitPoints) && CurrentDyingDamagePerTick == 0)
            {
                this.HitPoints++;
            }

        }



    }
}
