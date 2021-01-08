using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;
using RimWorld.BaseGen;
using RimWorld.Planet;

namespace KCSG
{
    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(GenStep_Settlement))]
    [HarmonyPatch("ScatterAt", MethodType.Normal)]
    public class GenStepPatches
    {
        [HarmonyPrefix]
        public static bool Prefix(IntVec3 c, Map map, GenStepParams parms, int stackCount = 1)
        {
            /* if (VFECore.VFEGlobal.settings.enableLog)
            {
                Log.Message("Testing defs");
                foreach (SymbolDef def in DefDatabase<SymbolDef>.AllDefs)
                {
                    if (def.isTerrain && def.terrainDef == null)
                        Log.Error("Found invalid terrain def: " + def.defName);
                    else if (def.isPawn && def.pawnKindDefNS == null)
                        Log.Error("Found invalid pawn def: " + def.defName);
                    else if (!def.isTerrain && !def.isPawn && def.thingDef == null)
                        Log.Error("Found invalid thing def: " + def.defName);
                }
            } */

            if (map.ParentFaction != null && map.ParentFaction.def.HasModExtension<FactionSettlement>())
            {
                FactionSettlement factionSettlement = map.ParentFaction.def.GetModExtension<FactionSettlement>();

                if (factionSettlement.symbolResolver == null) GenStepPatchesUtils.Generate(map, c, factionSettlement);
                else GenStepPatchesUtils.Generate(map, c, factionSettlement, factionSettlement.symbolResolver);

                return false;
            }
            else if (Find.World.worldObjects.AllWorldObjects.Find(o=>o.Tile == map.Tile && o.def.HasModExtension<KCSG.FactionSettlement>()) is WorldObject worldObject)
            {
                FactionSettlement factionSettlement = worldObject.def.GetModExtension<FactionSettlement>();

                if (factionSettlement.symbolResolver == null) GenStepPatchesUtils.Generate(map, c, factionSettlement);
                else GenStepPatchesUtils.Generate(map, c, factionSettlement, factionSettlement.symbolResolver);

                return false;
            }
            else return true;
        }
    }

    public class GenStepPatchesUtils
    {
        public static void Generate(Map map, IntVec3 c, FactionSettlement sf, string symbolResolver = "kcsg_settlement")
        {
            FactionSettlement.tempUseStructureLayout = sf.useStructureLayout;

            string sld;

            if (!sf.useStructureLayout)
            {
                sld = sf.chooseFromSettlements.RandomElement().defName;
            }
            else
            {
                if (ModLister.RoyaltyInstalled) sld = sf.chooseFromlayouts.RandomElement().defName;
                else sld = sf.chooseFromlayouts.ToList().FindAll(sfl => !sfl.requireRoyalty).RandomElement().defName;
            }

            FactionSettlement.temp = sld;

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
                KCSG_Utilities.HeightWidthFromLayout(DefDatabase<StructureLayoutDef>.GetNamed(sld), out height, out width);
            }
            else
            {
                SettlementLayoutDef temp = DefDatabase<SettlementLayoutDef>.GetNamed(sld);
                height = temp.settlementSize.x;
                width = temp.settlementSize.z;
            }

            CellRect rect = new CellRect(c.x - width / 2, c.z - height / 2, width, height);
            rect.ClipInsideMap(map);

            ResolveParams rp = default(ResolveParams);
            rp.faction = faction;
            rp.rect = rect;

            BaseGen.globalSettings.map = map;
            BaseGen.symbolStack.Push(symbolResolver, rp, null);
            BaseGen.Generate();
        }
    }
}
