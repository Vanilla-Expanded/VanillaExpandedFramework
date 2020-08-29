using Verse;

namespace VFECore
{
    public static class Patch_FactionDiscovery_ModBase
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
