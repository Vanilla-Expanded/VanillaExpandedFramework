using Verse;

namespace OPToxic
{
    public class OPToxicDefGetValue
    {
        public static string OPToxicGetHediff(ThingDef thingdef)
        {
            if (thingdef.HasModExtension<OPToxicDefs>())
            {
                return thingdef.GetModExtension<OPToxicDefs>().OPToxicHediff;
            }
            return null;
        }

        public static float OPToxicGetSev(ThingDef thingdef)
        {
            if (thingdef.HasModExtension<OPToxicDefs>())
            {
                return thingdef.GetModExtension<OPToxicDefs>().OPToxicSeverity;
            }
            return 0f;
        }

        public static int OPToxicGetSevUpVal(ThingDef thingdef)
        {
            if (thingdef.HasModExtension<OPToxicDefs>())
            {
                return thingdef.GetModExtension<OPToxicDefs>().OPSevUpTickPeriod;
            }
            return 120;
        }
    }
}