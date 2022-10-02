using HarmonyLib;
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
            MP.RegisterSyncMethod(typeof(WeatherOverlay_Effects), "DoDamage", null);
            var doDamage = AccessTools.Method(typeof(WeatherOverlay_Effects), "DoDamage", null, null);
            harmony.Patch(doDamage, new HarmonyMethod(typeof(MultiplayerSupport), "FixRNGPre", null), new HarmonyMethod(typeof(MultiplayerSupport), "FixRNGPos", null), null, null);
        }

        private static void FixRNGPre()
        {
            Rand.PushState(Find.TickManager.TicksAbs);
        }

        private static void FixRNGPos()
        {
            Rand.PopState();
        }

        private static readonly Harmony harmony = new("vfe.multiplayersupport");
    }
}