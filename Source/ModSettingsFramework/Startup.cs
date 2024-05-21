using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ModSettingsFramework
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            foreach (var patch in ModContentPack_LoadPatches_Patch.allPatches.Values.SelectMany(x => x).OfType<PatchOperationWorker>().ToList())
            {
                patch.ApplySettings();
            }
        }
    }
}
