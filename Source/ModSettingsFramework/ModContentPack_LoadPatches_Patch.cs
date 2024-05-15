using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace ModSettingsFramework
{
    [HarmonyPatch(typeof(ModContentPack), "LoadPatches")]
    public static class ModContentPack_LoadPatches_Patch
    {
        public static List<PatchOperationWorker> allWorkers = new();

        public static void Postfix(ModContentPack __instance)
        {
            if (__instance.patches != null)
            {
                foreach (var patch in  __instance.patches)
                {
                    if (patch is PatchOperationModSettings modSettings)
                    {
                        modSettings.modContentPack = __instance;
                        if (patch is PatchOperationWorker worker)
                        {
                            allWorkers.Add(worker);
                        }
                    }
                }
            }
        }
    }
}
