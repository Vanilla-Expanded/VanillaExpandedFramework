using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace KCSG
{
    internal class SymbolResolver_KCSG_RoomGenFromStructure : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;

            StructureLayoutDef rld = CurrentGenerationOption.structureLayoutDef;

            foreach (List<String> item in rld.layouts)
            {
                KCSG_Utilities.GenerateRoomFromLayout(item, rp.rect, map, rld);
            }

            ThingDef conduit;
            if (LoadedModManager.RunningMods.ToList().FindAll(m => m.Name == "Subsurface Conduit").Count > 0) conduit = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(d => d.defName == "MUR_SubsurfaceConduit").First();
            else conduit = ThingDefOf.PowerConduit;

            KCSG_Utilities.EnsureBatteriesConnectedAndMakeSense(map, tmpThings, tmpPowerNetPredicateResults, tmpCells, conduit);
            KCSG_Utilities.EnsurePowerUsersConnected(map, tmpThings, tmpPowerNetPredicateResults, tmpCells, conduit);
            KCSG_Utilities.EnsureGeneratorsConnectedAndMakeSense(map, tmpThings, tmpPowerNetPredicateResults, tmpCells, conduit);

            CurrentGenerationOption.ClearAll();
        }

        private readonly List<Thing> tmpThings = new List<Thing>();
        private readonly Dictionary<PowerNet, bool> tmpPowerNetPredicateResults = new Dictionary<PowerNet, bool>();
        private readonly List<IntVec3> tmpCells = new List<IntVec3>();
    }
}