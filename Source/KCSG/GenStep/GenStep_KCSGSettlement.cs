using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
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

            CellRect rect = new CellRect(c.x - width / 2, c.z - height / 2, width, height);
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
            if (map.ParentFaction != null && map.ParentFaction.def.HasModExtension<FactionSettlement>())
            {
                FactionSettlement factionSettlement = map.ParentFaction.def.GetModExtension<FactionSettlement>();

                if (factionSettlement.symbolResolver == null) GenStepPatchesUtils.Generate(map, loc, factionSettlement);
                else GenStepPatchesUtils.Generate(map, loc, factionSettlement, factionSettlement.symbolResolver);
            }
            else if (Find.World.worldObjects.AllWorldObjects.Find(o => o.Tile == map.Tile && o.def.HasModExtension<FactionSettlement>()) is WorldObject worldObject)
            {
                FactionSettlement factionSettlement = worldObject.def.GetModExtension<FactionSettlement>();

                if (factionSettlement.symbolResolver == null) GenStepPatchesUtils.Generate(map, loc, factionSettlement);
                else GenStepPatchesUtils.Generate(map, loc, factionSettlement, factionSettlement.symbolResolver);
            }
        }
    }
}