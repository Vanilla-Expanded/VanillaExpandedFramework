using RimWorld.BaseGen;
using System;
using Verse;

namespace KCSG
{
    internal class SymbolResolver_ScatterStuffAround : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;
            Random r = new Random(Find.TickManager.TicksGame);

            foreach (IntVec3 cell in rp.rect)
            {
                if (cell.Roofed(map) && cell.Walkable(map) && r.NextDouble() < CGO.factionSettlement.scatterChance)
                {
                    GenSpawn.Spawn(CGO.factionSettlement.scatterThings.RandomElement(), cell, map, WipeMode.FullRefund);
                }
            }
        }
    }
}