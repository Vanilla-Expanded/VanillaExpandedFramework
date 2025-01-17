using RimWorld;
using Verse;

namespace PipeSystem
{
    [DefOf]
    public static class PSDefOf
    {
        public static DesignationDef PS_Drain;

        public static JobDef PS_DrainFromMarkedStorage;
        public static JobDef PS_FillStorage;

        public static JobDef PS_PickUpProcessor;
        public static JobDef PS_BringToProcessor;
        [MayRequire("VanillaExpanded.VMemesE")]
        public static PreceptDef VME_AutomationEfficiency_Increased;
        [MayRequire("VanillaExpanded.VMemesE")]
        public static PreceptDef VME_AutomationEfficiency_Decreased;
    }
}
