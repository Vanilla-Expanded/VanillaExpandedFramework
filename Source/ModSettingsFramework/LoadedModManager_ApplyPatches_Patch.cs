using HarmonyLib;
using Verse;

namespace ModSettingsFramework
{
    [HarmonyPatch(typeof(LoadedModManager), "ApplyPatches")]
    public static class LoadedModManager_ApplyPatches_Patch
    {
        public static void Prefix()
        {
            ModSettingsFrameworkMod.settings = LoadedModManager.GetMod<ModSettingsFrameworkMod>().LoadSettings();
        }
    }
}
