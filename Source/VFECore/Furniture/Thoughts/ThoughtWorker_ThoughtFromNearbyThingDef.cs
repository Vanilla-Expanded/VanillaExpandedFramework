using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VanillaFurnitureExpanded
{
   
    public class ThoughtWorker_ThoughtFromNearbyThingDef : ThoughtWorker
    {

        private const float Radius = 15f;

        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!p.Spawned)
            {
                return false;
            }
            if (!this.def.HasModExtension<ThoughtGiverByProximityDefExtension>())
            {
                return false;
            }
            ThoughtGiverByProximityDefExtension defExtension = this.def.GetModExtension<ThoughtGiverByProximityDefExtension>();
            if (defExtension.ThingToGiveThought == null)
            {
                return false;
            }
            List<Thing> list = p.Map.listerThings.ThingsOfDef(defExtension.ThingToGiveThought);
            for (int i = 0; i < list.Count; i++)
            {
                CompPowerTrader compPowerTrader = list[i].TryGetComp<CompPowerTrader>();
                if ((compPowerTrader == null || compPowerTrader.PowerOn) && list[i] != p)
                {
                    if (p.Position.InHorDistOf(list[i].Position, defExtension.DistanceToGiveThought))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        
       
    }
}