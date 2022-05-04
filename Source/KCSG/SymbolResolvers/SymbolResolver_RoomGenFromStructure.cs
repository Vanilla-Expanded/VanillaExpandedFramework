using RimWorld;
using RimWorld.BaseGen;
using System.Collections.Generic;
using Verse;

namespace KCSG
{
    internal class SymbolResolver_RoomGenFromStructure : SymbolResolver
    {
        private readonly List<IntVec3> tmpCells = new List<IntVec3>();

        private readonly Dictionary<PowerNet, bool> tmpPowerNetPredicateResults = new Dictionary<PowerNet, bool>();

        private readonly List<Thing> tmpThings = new List<Thing>();

        public override void Resolve(ResolveParams rp)
        {
            CGO.currentGenStep = "Generating single structure";

            Map map = BaseGen.globalSettings.map;

            for (int i = 0; i < CGO.structureLayoutDef.layouts.Count; i++)
            {
                GenUtils.GenerateRoomFromLayout(CGO.structureLayoutDef, i, rp.rect, map);
            }
            GenUtils.GenerateRoofGrid(CGO.structureLayoutDef.roofGrid, rp.rect, map);

            GenUtils.EnsureBatteriesConnectedAndMakeSense(map, tmpThings, tmpPowerNetPredicateResults, tmpCells, KThingDefOf.KCSG_PowerConduit);
            GenUtils.EnsurePowerUsersConnected(map, tmpThings, tmpPowerNetPredicateResults, tmpCells, KThingDefOf.KCSG_PowerConduit);
            GenUtils.EnsureGeneratorsConnectedAndMakeSense(map, tmpThings, tmpPowerNetPredicateResults, tmpCells, KThingDefOf.KCSG_PowerConduit);
        }
    }
}