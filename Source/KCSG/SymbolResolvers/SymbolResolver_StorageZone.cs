using RimWorld;
using RimWorld.BaseGen;
using System.Collections.Generic;
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

            this.CalculateFreeCells(rp.rect, 0.45f);
            ThingSetMakerDef thingSetMakerDef = ThingSetMakerDefOf.MapGen_DefaultStockpile;

            ThingSetMakerParams value = default;
            value.techLevel = new TechLevel?((rp.faction != null) ? rp.faction.def.techLevel : TechLevel.Undefined);
            value.makingFaction = rp.faction;
            value.validator = ((ThingDef x) => rp.faction == null || x.techLevel >= rp.faction.def.techLevel || !x.IsWeapon || x.GetStatValueAbstract(StatDefOf.MarketValue, GenStuff.DefaultStuffFor(x)) >= 100f);
            float num3 = rp.stockpileMarketValue ?? Mathf.Min((float)this.cells.Count * 130f, 1800f);
            value.totalMarketValueRange = new FloatRange?(new FloatRange(num3, num3));
            if (value.countRange == null)
            {
                value.countRange = new IntRange?(new IntRange(this.cells.Count, this.cells.Count));
            }
            ResolveParams rp2 = rp;
            rp2.thingSetMakerDef = thingSetMakerDef;
            rp2.thingSetMakerParams = new ThingSetMakerParams?(value);
            BaseGen.symbolStack.Push("kcsg_thingsetonlyroofed", rp2, null);
        }

        private void CalculateFreeCells(CellRect rect, float freeCellsFraction)
        {
            Map map = BaseGen.globalSettings.map;
            this.cells.Clear();
            foreach (IntVec3 intVec in rect)
            {
                if (intVec.Standable(map) && intVec.Roofed(map) && intVec.GetFirstItem(map) == null)
                {
                    this.cells.Add(intVec);
                }
            }
            int num = (int)(freeCellsFraction * (float)this.cells.Count);
            for (int i = 0; i < num; i++)
            {
                this.cells.RemoveAt(Rand.Range(0, this.cells.Count));
            }
            this.cells.Shuffle<IntVec3>();
        }
    }
}