using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Outposts
{
    public class OutpostsMod : Mod
    {
        public static List<WorldObjectDef> Outposts;
        public static Harmony Harm;
        public static OutpostsSettings Settings;

        public OutpostsMod(ModContentPack content) : base(content)
        {
            LongEventHandler.ExecuteWhenFinished(FindOutposts);
            Settings = GetSettings<OutpostsSettings>();
        }

        private static void FindOutposts()
        {
            Outposts = DefDatabase<WorldObjectDef>.AllDefs.Where(def => typeof(Outpost).IsAssignableFrom(def.worldObjectClass)).ToList();
            Harm = new Harmony("vanillaexpanded.outposts");
            if (Outposts.Any()) Harm.Patch(AccessTools.Method(typeof(Caravan), "GetGizmos"), postfix: new HarmonyMethod(typeof(HarmonyPatches), "AddCaravanGizmos"));
        }

        public override string SettingsCategory()
        {
            return Outposts.Any() ? "Vanilla Expanded Framework - Outposts" : null;
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            var listing = new Listing_Standard();
            listing.Begin(inRect);
            listing.Label("Outpost Production Multiplier:");
            Settings.Multiplier = listing.Slider(Settings.Multiplier, 0f, 10f);
        }
    }

    public class OutpostsSettings : ModSettings
    {
        public float Multiplier = 1f;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref Multiplier, "multiplier", 1f);
        }
    }
}