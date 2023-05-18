using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace KCSG
{
    public static class Debug
    {
        public static bool Enabled => VFECore.VFEGlobal.settings.enableVerboseLogging;

        public static void Message(string message)
        {
            if (Enabled)
                Log.Message($"<color=orange>[KCSG]</color> {message}");
        }

        public static void Error(string message) => Log.Error($"[KCSG] {message}");
    }
}