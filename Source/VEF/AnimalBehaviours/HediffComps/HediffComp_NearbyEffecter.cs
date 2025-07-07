
using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace VEF.AnimalBehaviours
{
    public class HediffComp_NearbyEffecter : HediffComp
    {
        public HediffCompProperties_NearbyEffecter Props
        {
            get
            {
                return (HediffCompProperties_NearbyEffecter)this.props;
            }
        }


        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);
            //Only work every ticksConversionRate
            if (Pawn.IsHashIntervalTick(Props.ticksConversionRate, delta))
            {
                Pawn pawn = this.parent.pawn as Pawn;
                //If the pawn isn't down and the map isn't null
                if ((!pawn.Downed) && (pawn.Map != null))
                {
                    CellRect rect = GenAdj.OccupiedRect(pawn.Position, pawn.Rotation, IntVec2.One);
                    rect = rect.ExpandedBy(Props.radius);

                    foreach (IntVec3 current in rect.Cells)
                    {
                        if (current.InBounds(pawn.Map))
                        {
                            HashSet<Thing> hashSet = new HashSet<Thing>(current.GetThingList(pawn.Map));
                            Thing current2 = hashSet.FirstOrFallback();
                            if (current2 != null)
                            {
                                if (Props.thingsToAffect.Contains(current2.def.defName))
                                {

                                    Thing thing = GenSpawn.Spawn(ThingDef.Named(Props.thingsToConvertTo[Props.thingsToAffect.IndexOf(current2.def.defName)]), current, pawn.Map, WipeMode.Vanish);
                                    thing.stackCount = current2.stackCount;
                                    if (Props.isForbidden)
                                    {
                                        thing.SetForbidden(true);

                                    }
                                    if (Props.feedCauser)
                                    {
                                        if (pawn?.needs?.food != null)
                                        {
                                            pawn.needs.food.CurLevel += Props.nutritionGained;

                                        }

                                    }
                                    current2.Destroy();
                                    break;
                                }

                            }
                        }

                    }
                }
            }
        }


    }
}
