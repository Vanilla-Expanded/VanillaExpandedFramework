using Verse;

namespace VEF.Factions
{
    public static class VanillaExpandedFramework_FactionDiscovery_ModBase_Patch
    {
        public class manual_RunCheck
        {
            public static bool Prefix()
            {
                Log.Message("FactionDiscovery overridden. It is not required anymore. Use VFE Core instead.");
                return false; // Don't run the original code at all
            }
        }
    }
}
