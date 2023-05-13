using System.Collections.Generic;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace KCSG
{
    internal class GenStep_Settlement : RimWorld.GenStep_Settlement
    {
        public override int SeedPart => 1931078471;

        protected override bool CanScatterAt(IntVec3 loc, Map map)
        {
            return true;
        }

        protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
        {
            Generate(loc, map, map.ParentFaction.def.GetModExtension<CustomGenOption>());
        }

        public static void Generate(IntVec3 loc, Map map, CustomGenOption ext)
        {
            GenOption.ext = ext;

            if (GenOption.ext.UsingSingleLayout)
            {
                GenOption.structureLayoutDef = GenUtils.ChooseStructureLayoutFrom(GenOption.ext.chooseFromlayouts);
            }
            else
            {
                GenOption.sld = GenOption.ext.chooseFromSettlements.RandomElement();
            }

            // Get faction
            Faction faction = map.ParentFaction == null || map.ParentFaction == Faction.OfPlayer ? Find.FactionManager.RandomEnemyFaction() : map.ParentFaction;

            // Get settlement size
            int width = GenOption.ext.UsingSingleLayout ? GenOption.structureLayoutDef.size : GenOption.sld.settlementSize.x;
            int height = GenOption.ext.UsingSingleLayout ? GenOption.structureLayoutDef.size : GenOption.sld.settlementSize.z;

            // Get spawn position
            IntVec3 spawn = loc;
            if (GenOption.ext.tryFindFreeArea)
            {
                if (!RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith(i => RectFreeValidator(CellRect.CenteredOn(i, width, height), map), map, out spawn))
                    Log.Warning($"[KCSG] Trying to find free spawn area failed");
            }

            // Create rect
            CellRect rect = CellRect.CenteredOn(spawn, width, height);
            rect.ClipInsideMap(map);

            GenOption.GetAllMineableIn(rect, map);
            // Pre-gen clean
            if (ext.preGenClear)
                GenUtils.PreClean(GenOption.structureLayoutDef, map, rect, ext.fullClear);

            // Push symbolresolver
            ResolveParams rp = default;
            rp.faction = faction;
            rp.rect = rect;
            BaseGen.globalSettings.map = map;
            BaseGen.symbolStack.Push(GenOption.ext.symbolResolver ?? "kcsg_settlement", rp, null);
            BaseGen.Generate();
        }

        private static bool RectFreeValidator(CellRect rect, Map map)
        {
            foreach (var cell in rect)
            {
                if (!cell.Walkable(map) || cell.GetTerrain(map).affordances.Contains(TerrainAffordanceDefOf.Bridgeable))
                    return false;
            }
            return true;
        }
    }
}