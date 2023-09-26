using System.Linq;
using Verse;

namespace ModSettingsFramework
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            foreach (var mod in LoadedModManager.RunningMods)
            {
                foreach (var patch in mod.Patches.OfType<PatchOperationWorker>())
                {
                    var modSettings = ModSettingsFrameworkSettings.GetModSettingsContainer(mod);
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
}
