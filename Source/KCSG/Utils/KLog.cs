using Verse;

namespace KCSG
{
    public class KLog
    {
        public static void Message(string message)
        {
            if (VFECore.VFEGlobal.settings.enableVerboseLogging) Log.Message("[KCSG]" + message);
        }
    }
}
