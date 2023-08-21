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
            CachedCompResourceStorage.Clear();
            CachedPipeNetManager.Clear();
        }
    }
}