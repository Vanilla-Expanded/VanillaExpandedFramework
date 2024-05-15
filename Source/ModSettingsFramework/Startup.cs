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
            foreach (var patch in ModContentPack_LoadPatches_Patch.allWorkers.ToList())
            {
                var modSettings = patch.SettingsContainer;
                if (modSettings.patchWorkers.TryGetValue(patch.GetType().FullName, out var worker))
                {
                    patch.CopyFrom(worker);
                    patch.ApplySettings();
                }
                else
                {
                    patch.Init();
                    patch.ApplySettings();
                }
            }
        }
    }
}
