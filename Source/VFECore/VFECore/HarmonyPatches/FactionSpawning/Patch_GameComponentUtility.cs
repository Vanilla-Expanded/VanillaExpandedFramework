using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;
using System.Reflection;

namespace VFECore
{
    public static class Patch_GameComponentUtility
    {
        [HarmonyPatch(typeof(GameComponentUtility), nameof(GameComponentUtility.LoadedGame))]
        public static class LoadedGame
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                LongEventHandler.ExecuteWhenFinished(OnGameLoaded);
            }

            private static void OnGameLoaded()
            {
                if (Current.Game == null) return;

                var factionEnumerator = DefDatabase<FactionDef>.AllDefs.Where(Validator).GetEnumerator();
                if (factionEnumerator.MoveNext())
                {
                    // Only one dialog can be stacked at a time, so give it the list of all factions
                    Dialog_NewFactionSpawning.OpenDialog(factionEnumerator);
                }
            }

            internal static bool Validator(FactionDef faction)
            {
                if (faction == null) return false;
                if (faction.isPlayer) return false;
                if (!faction.canMakeRandomly && faction.hidden && faction.maxCountAtGameStart <= 0) return false;
                if (Find.FactionManager.AllFactions.Count(f => f.def == faction) > 0) return false;
                if (Find.World?.GetComponent<NewFactionSpawningState>()?.IsIgnored(faction) == true) return false;
                if (NewFactionSpawningUtility.NeverSpawn(faction)) return false;
                return true;
            }
        }

        [HarmonyPatch(typeof(GameComponentUtility), nameof(GameComponentUtility.StartedNewGame))]
        public static class StartedNewGame
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                LongEventHandler.ExecuteWhenFinished(OnNewGame);
            }

            private static void OnNewGame()
            {
                Find.World?.GetComponent<NewFactionSpawningState>()?.Ignore(DefDatabase<FactionDef>.AllDefs);
            }
        }
    }
}