using HarmonyLib;
using Multiplayer.API;
using Verse;

namespace VFECore
{
    [StaticConstructorOnStartup]
    internal static class MultiplayerSupport
    {
        static MultiplayerSupport()
        {
            if (!MP.enabled)
            {
                return;
            }
            MP.RegisterSyncMethod(typeof(CompAutumnLeavesSpawner), "TryFindSpawnCell", null);
            var tryFindSpawnCell = AccessTools.Method(typeof(CompAutumnLeavesSpawner), "TryFindSpawnCell", null, null);
            harmony.Patch(tryFindSpawnCell, new HarmonyMethod(typeof(MultiplayerSupport), "FixRNGPre", null), new HarmonyMethod(typeof(MultiplayerSupport), "FixRNGPos", null), null, null);
        }

        private static void FixRNGPre()
        {
            Rand.PushState(Find.TickManager.TicksAbs);
        }

        private static void FixRNGPos()
        {
            Rand.PopState();
        }

        private static readonly Harmony harmony = new Harmony("vfe.multiplayersupport");
    }
}