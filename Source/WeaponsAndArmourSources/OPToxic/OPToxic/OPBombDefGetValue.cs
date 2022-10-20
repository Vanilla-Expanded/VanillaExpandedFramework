using Verse;

namespace OPToxic
{
    public class OPBombDefGetValue
    {
        public static int OPBombGetDmg(ThingDef thingdef)
        {
            if (thingdef.HasModExtension<OPBombDefs>())
            {
                return thingdef.GetModExtension<OPBombDefs>().OPBombDmg;
            }
            return 25;
        }

        public static int OPBombGetImpactRadius(ThingDef thingdef)
        {
            if (thingdef.HasModExtension<OPBombDefs>())
            {
                return thingdef.GetModExtension<OPBombDefs>().OPBombImpactRadius;
            }
            return 12;
        }

        public static int OPBombGetBlastMinRadius(ThingDef thingdef)
        {
            if (thingdef.HasModExtension<OPBombDefs>())
            {
                return thingdef.GetModExtension<OPBombDefs>().OPBombBlastMinRadius;
            }
            return 4;
        }

        public static int OPBombGetBlastMaxRadius(ThingDef thingdef)
        {
            if (thingdef.HasModExtension<OPBombDefs>())
            {
                return thingdef.GetModExtension<OPBombDefs>().OPBombBlastMaxRadius;
            }
            return 6;
        }
    }
}