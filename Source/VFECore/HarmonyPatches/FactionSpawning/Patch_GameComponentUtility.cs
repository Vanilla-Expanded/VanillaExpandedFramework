using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFECore
{
    public static class Patch_GameComponentUtility
    {
        [HarmonyPatch(typeof(GameComponentUtility), nameof(GameComponentUtility.LoadedGame))]
        public static class LoadedGame
        {
            public static void Postfix()
            {
                LongEventHandler.ExecuteWhenFinished(OnGameLoaded);
            }

            private static void OnGameLoaded()
            {
                var factionEnumerator = DefDatabase<FactionDef>.AllDefs.Where(Validator).GetEnumerator();
                if (factionEnumerator.MoveNext())
                {
                    // Only one dialog can be stacked at a time, so give it the list of all factions
                    Dialog_NewFactionSpawning.OpenDialog(factionEnumerator);
                }
                }
            }

            private static bool Validator(FactionDef faction)
            {
                if (faction == null) return false;
                if (faction.isPlayer) return false;
                var count = Find.FactionManager.AllFactions.Count(f => f.def == faction);
                //if (count > 0) return false;
                if (Find.World?.GetComponent<NewFactionSpawningState>()?.IsIgnored(faction) == true) return false;
                return true;
            }
        }
    }
}
