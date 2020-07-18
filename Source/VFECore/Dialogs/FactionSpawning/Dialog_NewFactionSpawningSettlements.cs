using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VFECore
{
    public class Dialog_NewFactionSpawningSettlements : Window
    {
        private readonly Action<int, int> spawnCallback;

        private const float newFactionSettlementFactor = 0.7f; // recommendation
        private const float settlementsPer100KTiles = 80; // average

        private int settlementsToSpawn;
        private int settlementsRecommended;
        private int distanceToSpawn;
        private int distanceRecommended;

        public static void OpenDialog(Action<int, int> spawnCallback)
        {
            Find.WindowStack.Add(new Dialog_NewFactionSpawningSettlements(spawnCallback));
        }

        private Dialog_NewFactionSpawningSettlements(Action<int, int> spawnCallback)
        {
            doCloseButton = false;
            forcePause = true;
            absorbInputAroundWindow = true;
            this.spawnCallback = spawnCallback;

            settlementsToSpawn = settlementsRecommended = GetSettlementsRecommendation();
            distanceToSpawn = distanceRecommended = SettlementProximityGoodwillUtility.MaxDist;

            if (AccessTools.TypeByName("FactionControl.Controller") != null) FactionControlFix();
        }

        // TODO: Remove this once FactionControl has fixed this bug on their end
        // https://github.com/KiameV/rimworld-factioncontrol/issues/17
        private static void FactionControlFix()
        {
            try
            {
                var controller = AccessTools.TypeByName("FactionControl.Controller");

                var fieldSeparation = controller.GetField("minFactionSeparation", BindingFlags.Static | BindingFlags.Public);
                var fieldFactionSprawl = controller.GetField("maxFactionSprawl", BindingFlags.Static | BindingFlags.Public);
                var fieldPirateSprawl = controller.GetField("pirateSprawl", BindingFlags.Static | BindingFlags.Public);

                // Check if not initialized
                if (((double)fieldSeparation.GetValue(null)).Equals(0))
                {
                    Log.Message($"Found Faction Control mod. Fixing its values to avoid problems.");
                    double tiles = Math.Sqrt(Find.WorldGrid.TilesCount);
                    double factions = Math.Sqrt(Find.FactionManager.AllFactionsVisible.Count() - 1);
                    fieldSeparation.SetValue(null, tiles / (factions * 2));
                    var defaultFactionGrouping = 0.5; // use default; good enough for not breaking everything
                    fieldFactionSprawl.SetValue(null, tiles / (factions * defaultFactionGrouping));
                    fieldPirateSprawl.SetValue(null, tiles / (factions * defaultFactionGrouping));
                }
            }
            catch(Exception e)
            {
                Log.Warning($"Something went wrong when trying to initialize FactionControl.Controller:\n{e.Message}\n{e.StackTrace}");
            }
        }

        private static int GetSettlementsRecommendation()
        {
            int existingFactions = Find.FactionManager.AllFactionsVisible.Count();
            return GenMath.RoundRandom(Find.WorldGrid.TilesCount / 100000f * settlementsPer100KTiles / existingFactions * newFactionSettlementFactor);
        }

        public override void DoWindowContents(Rect inRect)
        {
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect.AtZero());

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;

            // Amount
            listing_Standard.Label($"Settlements to spawn (recommended are {settlementsRecommended}): {settlementsToSpawn}");
            settlementsToSpawn = Mathf.CeilToInt(listing_Standard.Slider(settlementsToSpawn, 1, settlementsRecommended * 2));

            // Distance from player
            listing_Standard.Label($"The minimum distance from player bases (recommended are {distanceRecommended}): {distanceToSpawn}");
            distanceToSpawn = Mathf.CeilToInt(listing_Standard.Slider(distanceToSpawn, 1, distanceRecommended * 2));

            if (listing_Standard.ButtonText("Spawn")) Spawn();
            if (listing_Standard.ButtonText("Cancel")) Close();

            listing_Standard.End();
        }

        private void Spawn()
        {
            Close();
            spawnCallback(settlementsToSpawn, distanceToSpawn);
        }
    }
}
