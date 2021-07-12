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

            List<Thing> things = map.listerThings.AllThings.FindAll(t => rp.rect.Contains(t.Position) && t is Building && t.Faction == map.ParentFaction);
            things.RemoveAll(t => t.def?.graphicData?.linkFlags != null && t.def?.graphicData?.linkFlags == LinkFlags.PowerConduit);
            
            for (int i = 0; i < things.Count; i++)
            {
                Thing t = things[i];
                int hitPointLoss = t.HitPoints / Rand.RangeInclusive(1, 8);

                if (hitPointLoss == t.HitPoints)
                {
                    IntVec3 pos = t.Position;
                    t.Destroy(DestroyMode.KillFinalize);
                    if (t.def.filthLeaving != null)
                        GenSpawn.Spawn(t.def.filthLeaving, pos, map, WipeMode.VanishOrMoveAside);
                }
                else
                {
                    t.HitPoints -= hitPointLoss;
                }                    
            }
        }
    }
}