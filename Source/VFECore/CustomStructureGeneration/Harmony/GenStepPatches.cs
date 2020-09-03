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

namespace KCSG
{
    [StaticConstructorOnStartup]
    public class GenStepPatches
    {
        //[HarmonyPatch(typeof(GenStep_Settlement), "ScatterAt")]
        public static bool Prefix(IntVec3 c, Map map, GenStepParams parms, int stackCount = 1)
        {
            if (map.ParentFaction != null && map.ParentFaction.def.HasModExtension<FactionSettlement>())
            {
                FactionSettlement sf = map.ParentFaction.def.GetModExtension<FactionSettlement>();
                SettlementLayoutDef sld = sf.chooseFrom.RandomElement();
                sf.temp = sld;

                // Get faction
                Faction faction;
                if (map.ParentFaction == null || map.ParentFaction == Faction.OfPlayer)
                {
                    faction = Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Undefined);
                }
                else faction = map.ParentFaction;

                // Get settlement size
                int width = sld.settlementSize.x;
                int height = sld.settlementSize.z;
                CellRect rect = new CellRect(c.x - width / 2, c.z - height / 2, width, height);
                rect.ClipInsideMap(map);

                ResolveParams rp = default(ResolveParams);
                rp.faction = faction;
                rp.rect = rect;


                BaseGen.globalSettings.map = map;
                BaseGen.symbolStack.Push("kcsg_settlement", rp, null);
                BaseGen.Generate();
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
