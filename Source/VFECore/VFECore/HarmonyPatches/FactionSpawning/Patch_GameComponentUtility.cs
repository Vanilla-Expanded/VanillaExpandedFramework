using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

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

                var factionsToConsider = new List<FactionDef>();
                var forcedFactions = new List<(FactionDef, ForcedFactionData)>();

                CollectPotentialFactionsToSpawn(DefDatabase<FactionDef>.AllDefs, factionsToConsider, forcedFactions);

                NewFactionSpawningUtility.SpawnFactions(forcedFactions);

                var factionEnumerator = factionsToConsider.GetEnumerator();
                if (factionEnumerator.MoveNext())
                {
                    // Only one dialog can be stacked at a time, so give it the list of all factions
                    Dialog_NewFactionSpawning.OpenDialog(factionEnumerator);
                }
                else
                {
                    factionEnumerator.Dispose();
                }
            }

            internal static void CollectPotentialFactionsToSpawn(IEnumerable<FactionDef> factions, ICollection<FactionDef> factionsToConsider = null, ICollection<(FactionDef, ForcedFactionData)> forcedFactions = null)
            {
                if (factionsToConsider == null && forcedFactions == null)
                {
                    Log.Error("Trying to gather list of factions to spawn, but both working lists were null.");
                    return;
                }

                foreach (var faction in factions)
                {
                    if (faction == null) continue;
                    if (faction.isPlayer) continue;
                    if (faction.hidden && faction.requiredCountAtGameStart <= 0) continue;
                    if (NewFactionSpawningUtility.NeverSpawn(faction)) continue;

                    var data = FactionDefExtension.Get(faction).forcedFactionData;

                    // Since FactionDefExtension.ConfigErrors can't reference the def it's
                    // attached to, this is going to serve as a workaround to error checking.
                    if (faction.requiredCountAtGameStart < data.requiredFactionCountAtWorldGeneration)
                        Log.Error($"Faction of type {faction} requires {data.requiredFactionCountDuringGameplay} factions at world creation, but {nameof(FactionDef)}.{nameof(FactionDef.requiredCountAtGameStart)} is {faction.requiredCountAtGameStart}.");
                    if (faction.requiredCountAtGameStart < data.requiredFactionCountDuringGameplay)
                        Log.Error($"Faction of type {faction} requires {data.requiredFactionCountDuringGameplay} factions during gameplay, but {nameof(FactionDef)}.{nameof(FactionDef.requiredCountAtGameStart)} is {faction.requiredCountAtGameStart}.");

                    var factionCount = Find.FactionManager.AllFactions.Count(f => f.def == faction);
                    // Skip check for faction count and ignored if under required count and just return true
                    if ((data.forceAddFactionIfMissing || data.forcePlayerToAddFactionIfMissing) && data.UnderRequiredGameplayFactionCount(faction, factionCount))
                    {
                        if (data.forceAddFactionIfMissing)
                            forcedFactions?.Add((faction, data));
                        else
                            factionsToConsider?.Add(faction);
                        continue;
                    }

                    if (factionCount > 0) continue;
                    if (Find.World?.GetComponent<NewFactionSpawningState>()?.IsIgnored(faction) == true) continue;
                    factionsToConsider?.Add(faction);
                }
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