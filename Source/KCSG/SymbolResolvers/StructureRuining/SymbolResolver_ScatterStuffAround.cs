using RimWorld;
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
            if (CGO.factionSettlement.scatterThings?.Count > 0)
            {
                Random r = new Random(Find.TickManager.TicksGame);

                foreach (IntVec3 cell in rp.rect)
                {
                    if (cell.Roofed(map) && cell.Walkable(map) && r.NextDouble() < CGO.factionSettlement.scatterChance)
                    {
                        ThingDef thingDef = CGO.factionSettlement.scatterThings.RandomElement();
                        Thing thing = ThingMaker.MakeThing(thingDef, thingDef.MadeFromStuff ? thingDef.defaultStuff : null);
                        thing.stackCount = Math.Min(r.Next(5, thing.def.stackLimit), 75);
                        thing.SetForbidden(true, false);
                        GenSpawn.Spawn(thing, cell, map, WipeMode.FullRefund);
                    }
                }
            }
        }
    }
}