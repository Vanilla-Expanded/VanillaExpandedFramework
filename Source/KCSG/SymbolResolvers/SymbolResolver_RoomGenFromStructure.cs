using RimWorld;
using RimWorld.BaseGen;
using System;
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
            CurrentGenerationOption.currentGenStep = "Generating single structure";

            Map map = BaseGen.globalSettings.map;
            StructureLayoutDef rld = CurrentGenerationOption.structureLayoutDef;

            foreach (List<String> item in rld.layouts)
            {
                GenUtils.GenerateRoomFromLayout(item, rp.rect, map, rld);
            }

            GenUtils.EnsureBatteriesConnectedAndMakeSense(map, tmpThings, tmpPowerNetPredicateResults, tmpCells, KDefOf.KCSG_PowerConduit);
            GenUtils.EnsurePowerUsersConnected(map, tmpThings, tmpPowerNetPredicateResults, tmpCells, KDefOf.KCSG_PowerConduit);
            GenUtils.EnsureGeneratorsConnectedAndMakeSense(map, tmpThings, tmpPowerNetPredicateResults, tmpCells, KDefOf.KCSG_PowerConduit);
        }
    }
}