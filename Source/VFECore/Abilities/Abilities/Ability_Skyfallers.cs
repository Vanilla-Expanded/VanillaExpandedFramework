using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VFECore.Abilities
{
    public class Ability_Skyfallers : Ability
    {
        private static List<ThingDef> allNaturalRockDefs;
        public static List<ThingDef> AllNaturalRockDefs
        {
            get
            {
                if (allNaturalRockDefs == null)
                {
                    allNaturalRockDefs = DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.IsNonResourceNaturalRock).ToList();
                }
                return allNaturalRockDefs;
            }
        }
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            foreach (GlobalTargetInfo target in targets)
            {
                var cells = GenRadial.RadialCellsAround(target.Cell, GetRadiusForPawn(), true)
                                  .Where(c => c.InBounds(pawn.Map) && !c.Fogged(pawn.Map) && c.GetEdifice(pawn.Map) == null).ToList();
                var count = GetPowerForPawn();
                for (var i = 0; i < count; i++) SpawnSkyfaller(cells.RandomElement());
            }
        }

        protected virtual void SpawnSkyfaller(IntVec3 cell)
        {
            SkyfallerMaker.SpawnSkyfaller(def.GetModExtension<AbilityExtension_Skyfaller>().skyfaller, GetContents(), cell, pawn.Map);
        }

        protected virtual IEnumerable<Thing> GetContents()
        {
            var rocks = def.GetModExtension<AbilityExtension_Skyfaller>()?.rocks ?? 0;
            for (var i = 0; i < rocks; i++) yield return ThingMaker.MakeThing(AllNaturalRockDefs.RandomElement());
        }
    }

    public class AbilityExtension_Skyfaller : DefModExtension
    {
        public ThingDef skyfaller;
        public int      rocks;
    }
}
