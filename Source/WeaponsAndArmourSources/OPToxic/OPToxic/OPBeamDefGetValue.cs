using Verse;

namespace OPToxic
{
    public class OPBeamDefGetValue
    {
        public static float OPBeamGetDmgFact(ThingDef thingdef)
        {
            if (thingdef.HasModExtension<OPBeamDefs>())
            {
                return thingdef.GetModExtension<OPBeamDefs>().OPBeamDmgFactor;
            }
            return 1f;
        }

        public static float OPBeamGetRadius(ThingDef thingdef)
        {
            if (thingdef.HasModExtension<OPBeamDefs>())
            {
                return thingdef.GetModExtension<OPBeamDefs>().OPBeamRadius;
            }
            return 8f;
        }

        public static int OPBeamGetNumFires(ThingDef thingdef)
        {
            if (thingdef.HasModExtension<OPBeamDefs>())
            {
                return thingdef.GetModExtension<OPBeamDefs>().OPBeamNumFirePts;
            }
            return 3;
        }
    }
}