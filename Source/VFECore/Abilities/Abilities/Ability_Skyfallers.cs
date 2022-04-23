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
        private static readonly AccessTools.FieldRef<World, List<ThingDef>> allNaturalRockDefs =
            AccessTools.FieldRefAccess<World, List<ThingDef>>("allNaturalRockDefs");

        public override void Cast(LocalTargetInfo target)
        {
            base.Cast(target);

            var cells = GenRadial.RadialCellsAround(target.Cell, GetRadiusForPawn(), true)
                .Where(c => c.InBounds(pawn.Map) && !c.Fogged(pawn.Map) && c.GetEdifice(pawn.Map) == null).ToList();
            var count = GetPowerForPawn();
            for (var i = 0; i < count; i++) SpawnSkyfaller(cells.RandomElement());
        }

        protected virtual void SpawnSkyfaller(IntVec3 cell)
        {
            SkyfallerMaker.SpawnSkyfaller(def.GetModExtension<AbilityExtension_Skyfaller>().skyfaller, GetContents(), cell, pawn.Map);
        }

        protected virtual IEnumerable<Thing> GetContents()
        {
            Find.World.NaturalRockTypesIn(pawn.Map.Tile); // Force the game to generate the rocks list we are querying
            var rocks = def.GetModExtension<AbilityExtension_Skyfaller>()?.rocks ?? 0;
            for (var i = 0; i < rocks; i++) yield return ThingMaker.MakeThing(allNaturalRockDefs(Find.World).RandomElement());
        }
    }

    public class AbilityExtension_Skyfaller : DefModExtension
    {
        public ThingDef skyfaller;
        public int      rocks;
    }
}
