using System.Collections.Generic;
using RimWorld;
using RimWorld.BaseGen;
using UnityEngine;
using Verse;

namespace KCSG
{
    internal class SymbolResolver_StorageZone : SymbolResolver
    {
        private readonly List<IntVec3> cells = new List<IntVec3>();

        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;

            CalculateFreeCells(rp.rect, 0.45f);

            ThingSetMakerParams value = default;
            value.techLevel = new TechLevel?((rp.faction != null) ? rp.faction.def.techLevel : TechLevel.Undefined);
            value.makingFaction = rp.faction;
            value.validator = (ThingDef x) => rp.faction == null || x.techLevel >= rp.faction.def.techLevel || !x.IsWeapon || x.GetStatValueAbstract(StatDefOf.MarketValue, GenStuff.DefaultStuffFor(x)) >= 100f;
            float marketValue = rp.stockpileMarketValue ?? Mathf.Min(cells.Count * 130f, 1800f);
            marketValue *= GenOption.settlementLayout.stockpileOptions.stockpileValueMultiplier;
            value.totalMarketValueRange = new FloatRange?(new FloatRange(marketValue, marketValue));

            if (value.countRange == null)
            {
                value.countRange = new IntRange?(new IntRange(cells.Count, cells.Count));
            }

            ResolveParams rp2 = rp;
            rp2.thingSetMakerDef = ThingSetMakerDefOf.MapGen_DefaultStockpile;
            rp2.thingSetMakerParams = new ThingSetMakerParams?(value);
            BaseGen.symbolStack.Push("kcsg_thingsetonlyroofed", rp2, null);
        }

        private void CalculateFreeCells(CellRect rect, float freeCellsFraction)
        {
            Map map = BaseGen.globalSettings.map;
            cells.Clear();
            foreach (IntVec3 intVec in rect)
            {
                if (intVec.Standable(map) && intVec.Roofed(map) && intVec.GetFirstItem(map) == null)
                {
                    cells.Add(intVec);
                }
            }
            int num = (int)(freeCellsFraction * cells.Count);
            for (int i = 0; i < num; i++)
            {
                cells.RemoveAt(Rand.Range(0, cells.Count));
            }
            cells.Shuffle<IntVec3>();
        }
    }
}