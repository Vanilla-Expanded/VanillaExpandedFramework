using HarmonyLib;
using RimWorld;
using Verse;

namespace VFECore.FactionLoading
{
    public static class Patch_GameComponentUtility
    {
        [HarmonyPatch(typeof(GameComponentUtility), nameof(GameComponentUtility.LoadedGame))]
        public static class LoadedGame
        {
            public static void Postfix()
            {
                LongEventHandler.ExecuteWhenFinished(delegate { Log.Message($"This is faction loading."); });
            }
        }
    }
}
