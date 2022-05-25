using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace KCSG
{
    public class GenStepUtils
    {
        public static void Generate(Map map, IntVec3 c, CustomGenOption sf, string symbolResolver)
        {
            CGO.useStructureLayout = sf.useStructureLayout;

            if (sf.useStructureLayout)
            {
                CGO.structureLayoutDef = LayoutUtils.ChooseLayoutFrom(sf.chooseFromlayouts);
            }
            else
            {
                CGO.settlementLayoutDef = sf.chooseFromSettlements.RandomElement();
            }

            // Get faction
            Faction faction;
            if (map.ParentFaction == null || map.ParentFaction == Faction.OfPlayer)
            {
                faction = Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Undefined);
            }
            else
            {
                faction = map.ParentFaction;
            }

            // Get settlement size
            int width;
            int height;
            if (sf.useStructureLayout)
            {
                width = CGO.structureLayoutDef.width;
                height = CGO.structureLayoutDef.height;
            }
            else
            {
                SettlementLayoutDef temp = CGO.settlementLayoutDef;
                height = temp.settlementSize.x;
                width = temp.settlementSize.z;
            }

            IntVec3 intVec3 = c;
            if (CGO.factionSettlement.tryFindFreeArea)
            {
                bool success = RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith(cell => new CellRect(cell.x - width / 2, cell.z - height / 2, width, height).Cells.All(pC => pC.Walkable(map) && !pC.GetTerrain(map).affordances.Contains(TerrainAffordanceDefOf.Bridgeable)), map, out intVec3);
                KLog.Message($"Trying to find free spawn area success: {success}");
            }

            CellRect rect = new CellRect(intVec3.x - width / 2, intVec3.z - height / 2, width, height);
            rect.ClipInsideMap(map);

            ResolveParams rp = default;
            rp.faction = faction;
            rp.rect = rect;

            BaseGen.globalSettings.map = map;
            BaseGen.symbolStack.Push(symbolResolver, rp, null);
            BaseGen.Generate();
        }
    }
}