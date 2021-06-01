using HarmonyLib;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Verse;

namespace KCSG
{
    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(GenStep_Settlement))]
    [HarmonyPatch("ScatterAt", MethodType.Normal)]
    public class GenStep_Settlement_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(IntVec3 c, Map map)
        {
            if (map.ParentFaction != null && map.ParentFaction.def.HasModExtension<FactionSettlement>())
            {
                FactionSettlement factionSettlement = map.ParentFaction.def.GetModExtension<FactionSettlement>();

                if (factionSettlement.symbolResolver == null) GenStepPatchesUtils.Generate(map, c, factionSettlement);
                else GenStepPatchesUtils.Generate(map, c, factionSettlement, factionSettlement.symbolResolver);

                return false;
            }
            else if (Find.World.worldObjects.AllWorldObjects.Find(o => o.Tile == map.Tile && o.def.HasModExtension<FactionSettlement>()) is WorldObject worldObject)
            {
                FactionSettlement factionSettlement = worldObject.def.GetModExtension<FactionSettlement>();

                if (factionSettlement.symbolResolver == null) GenStepPatchesUtils.Generate(map, c, factionSettlement);
                else GenStepPatchesUtils.Generate(map, c, factionSettlement, factionSettlement.symbolResolver);

                return false;
            }
            else return true;
        }
    }

    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(GenStep_Power))]
    [HarmonyPatch("Generate", MethodType.Normal)]
    public class GenStep_Power_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(Map map)
        {
            if (map.ParentFaction != null && map.ParentFaction.def.HasModExtension<FactionSettlement>())
            {
                return false;
            }
            else return true;
        }
    }

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
}