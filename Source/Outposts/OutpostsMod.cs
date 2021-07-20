using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Outposts
{
    public class OutpostsMod : Mod
    {
        public static List<WorldObjectDef> Outposts;
        public static Harmony Harm;

        public OutpostsMod(ModContentPack content) : base(content)
        {
            LongEventHandler.ExecuteWhenFinished(FindOutposts);
        }

        private static void FindOutposts()
        {
            Outposts = DefDatabase<WorldObjectDef>.AllDefs.Where(def => typeof(Outpost).IsAssignableFrom(def.worldObjectClass)).ToList();
            Harm = new Harmony("vanillaexpanded.outposts");
            if (Outposts.Any()) Harm.Patch(AccessTools.Method(typeof(Caravan), "GetGizmos"), postfix: new HarmonyMethod(typeof(HarmonyPatches), "AddCaravanGizmos"));
        }

        public override string SettingsCategory()
        {
            return Outposts.Any() ? "Vanilla Outposts Expanded" : null;
        }
    }
}