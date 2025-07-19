using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

namespace VEF.AnimalBehaviours
{
    public static class Utils
    {


        public static float VacuumResistanceFromArmor(this Pawn pawn)
        {
            float vacuumResistance = 0f;
            List<Apparel> wornApparel = pawn.apparel?.WornApparel;
            if (!wornApparel.NullOrEmpty())
            {
                for (int i = 0; i < wornApparel.Count; i++)
                {
                    float statValue;
                    if ((statValue = StatWorker.StatOffsetFromGear(wornApparel[i], StatDefOf.VacuumResistance)) != 0)
                    {
                        vacuumResistance += statValue;
                    }
                }
            }
            Log.Message(vacuumResistance);
            return vacuumResistance;
        }
    }
}
