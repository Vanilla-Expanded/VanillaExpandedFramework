using HarmonyLib;
using Verse;

namespace PipeSystem
{
    /// <summary>
    /// Clear all PipeSystem caches when loading game
    /// </summary>
    [HarmonyPatch(typeof(Game))]
    [HarmonyPatch("ClearCaches", MethodType.Normal)]
    public static class Game_ClearCaches
    {
        public static void Postfix()
        {
            // Clear cached thing comps
            CachedCompResourceStorage.Clear();
            CachedCompAdvancedProcessor.Clear();
            // Clear cached map comps
            CachedPipeNetManager.Clear();
            CachedAdvancedProcessorsManager.Clear();
        }
    }
}