using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace ModSettingsFramework
{
    [HarmonyPatch(typeof(ModContentPack), "LoadPatches")]
    public static class ModContentPack_LoadPatches_Patch
    {
        public static Dictionary<ModContentPack, List<PatchOperationModSettings>> allPatches = new();
        public static void Postfix(ModContentPack __instance)
        {
            if (__instance.patches != null)
            {
                allPatches[__instance] = new List<PatchOperationModSettings> ();
                foreach (var patch in  __instance.patches)
                {
                    if (patch is PatchOperationModSettings modSettings)
                    {
                        modSettings.modContentPack = __instance;
                        allPatches[__instance].Add(modSettings);
                        if (patch is PatchOperationWorker patchWorker)
                        {
                            if (patchWorker.SettingsContainer.patchWorkers.TryGetValue(patch.GetType().FullName, out var worker))
                            {
                                patchWorker.CopyFrom(worker);
                            }
                            else
                            {
                                patchWorker.Init();
                            }
                        }
                    }
                }
            }
        }
    }
}
