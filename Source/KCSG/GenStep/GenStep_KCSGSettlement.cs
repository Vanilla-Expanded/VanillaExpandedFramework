using RimWorld;
using RimWorld.BaseGen;
using System.Linq;
using Verse;

namespace KCSG
{
    public class GenStepPatchesUtils
    {
        public static void Generate(Map map, IntVec3 c, FactionSettlement sf, string symbolResolver = "kcsg_settlement")
        {
            CurrentGenerationOption.useStructureLayout = sf.useStructureLayout;

            if (sf.useStructureLayout)
            {
                if (ModLister.RoyaltyInstalled) CurrentGenerationOption.structureLayoutDef = sf.chooseFromlayouts.RandomElement();
                else CurrentGenerationOption.structureLayoutDef = sf.chooseFromlayouts.ToList().FindAll(sfl => !sfl.requireRoyalty).RandomElement();
            }
            else
            {
                CurrentGenerationOption.settlementLayoutDef = sf.chooseFromSettlements.RandomElement();
            }

            // Get faction
            Faction faction;
            if (map.ParentFaction == null || map.ParentFaction == Faction.OfPlayer)
            {
                faction = Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Undefined);
            }
            else faction = map.ParentFaction;

            // Get settlement size
            int width;
            int height;
            if (sf.useStructureLayout)
            {
                RectUtils.HeightWidthFromLayout(CurrentGenerationOption.structureLayoutDef, out height, out width);
            }
            else
            {
                SettlementLayoutDef temp = CurrentGenerationOption.settlementLayoutDef;
                height = temp.settlementSize.x;
                width = temp.settlementSize.z;
            }

            IntVec3 intVec3 = c;
            if (CurrentGenerationOption.factionSettlement.tryFindFreeArea)
            {
                bool success = RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith(cell => new CellRect(cell.x - width / 2, cell.z - height / 2, width, height).Cells.All(pC => pC.Walkable(map) && !pC.GetTerrain(map).affordances.Contains(TerrainAffordanceDefOf.Bridgeable)), map, out intVec3);
                if (VFECore.VFEGlobal.settings.enableVerboseLogging)
                    Log.Message($"Trying to find free spawn area success: {success}");
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

    internal class GenStep_KCSGSettlement : GenStep_Settlement
    {
        public override int SeedPart => 1931078471;

        protected override bool CanScatterAt(IntVec3 loc, Map map)
        {
            return true;
        }

        protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
        {
            CurrentGenerationOption.factionSettlement = map.ParentFaction.def.GetModExtension<FactionSettlement>();

            if (CurrentGenerationOption.factionSettlement.symbolResolver == null) GenStepPatchesUtils.Generate(map, loc, CurrentGenerationOption.factionSettlement);
            else GenStepPatchesUtils.Generate(map, loc, CurrentGenerationOption.factionSettlement, CurrentGenerationOption.factionSettlement.symbolResolver);
        }
    }
}