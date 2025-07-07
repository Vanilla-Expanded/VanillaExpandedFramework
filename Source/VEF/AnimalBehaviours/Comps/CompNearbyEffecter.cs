using RimWorld;
using Verse;
using System;
using System.Collections.Generic;


namespace VEF.AnimalBehaviours
{
    class CompNearbyEffecter : ThingComp
    {

        public CompProperties_NearbyEffecter Props
        {
            get
            {
                return (CompProperties_NearbyEffecter)this.props;
            }
        }
        public override void CompTickInterval(int delta)
        {
            //Only work every ticksConversionRate
            if (parent.IsHashIntervalTick(Props.ticksConversionRate, delta))
            {
                Pawn pawn = this.parent as Pawn;
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