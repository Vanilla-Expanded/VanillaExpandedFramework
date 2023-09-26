using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace ModSettingsFramework
{
    [HarmonyPatch(typeof(LoadedModManager), "ModHandles", MethodType.Getter)]
    public static class LoadedModManager_ModHandles_Patch
    {
        public static IEnumerable<Mod> Postfix(IEnumerable<Mod> __result)
        {
            foreach (var mod in __result)
            {
                yield return mod;
            }
            foreach (var modSettings in ModSettingsFrameworkSettings.modSettingsPerModId)
            {
                if (modSettings.Value.modHandle != null)
                {
                    yield return modSettings.Value.modHandle;
                }
            }
        }
    }
}
