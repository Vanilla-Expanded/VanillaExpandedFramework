using RimWorld.BaseGen;
using System.Collections.Generic;
using Verse;

namespace KCSG
{
    internal class SymbolResolver_RandomDamage : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;

            List<Thing> things = map.listerThings.AllThings.FindAll(t => rp.rect.Contains(t.Position) && t.Faction == map.ParentFaction && t is Building);
            for (int i = 0; i < things.Count; i++)
            {
                Thing t = things[i];
                int hitPointLoss = t.HitPoints / Rand.RangeInclusive(1, 10);

                if (hitPointLoss == t.HitPoints)
                {
                    IntVec3 pos = t.Position;
                    t.Destroy(DestroyMode.KillFinalize);
                    for (int o = 0; o < Rand.RangeInclusive(2, 10); o++)
                        GenSpawn.Spawn(t.def.filthLeaving, pos, map, WipeMode.VanishOrMoveAside);
                }
                else
                    t.HitPoints -= hitPointLoss;
            }
        }
    }
}