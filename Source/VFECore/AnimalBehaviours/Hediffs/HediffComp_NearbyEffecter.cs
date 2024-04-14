
using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace AnimalBehaviours
{
    class HediffComp_NearbyEffecter : HediffComp
    {
        public HediffCompProperties_NearbyEffecter Props
        {
            get
            {
                return (HediffCompProperties_NearbyEffecter)this.props;
            }
        }
        private int tickProgress = 0;




        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            this.tickProgress += 1;
            //Only work every ticksConversionRate
            if (this.tickProgress > Props.ticksConversionRate)
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
                            if (hashSet != null)
                            {
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
                this.tickProgress = 0;
            }
        }


    }
}
