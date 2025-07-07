using System.Collections.Generic;
using RimWorld;
using Verse;


namespace VEF.AnimalBehaviours
{
    class CompFilthProducer : ThingComp
    {


        public CompProperties_FilthProducer Props
        {
            get
            {
                return (CompProperties_FilthProducer)this.props;
            }
        }

        public override void CompTickInterval(int delta)
        {
            if (AnimalBehaviours_Settings.flagAnimalParticles) {
                if (parent.IsHashIntervalTick(Props.ticksToCreateFilth, delta))
                {
                    Pawn pawn = this.parent as Pawn;
                    if (pawn.Map != null && pawn.Awake() && !pawn.Downed && !pawn.Dead)
                    {
                        CellRect rect = GenAdj.OccupiedRect(pawn.Position, pawn.Rotation, IntVec2.One);
                        rect = rect.ExpandedBy(Props.radius);

                        foreach (IntVec3 current in rect.Cells)
                        {
                            if (current.InBounds(pawn.Map) && Rand.Chance(Props.rate))
                            {
                                int filthNumber = 0;
                                List<Thing> list = this.parent.Map.thingGrid.ThingsListAt(current);
                                for (int i = 0; i < list.Count; i++)
                                {
                                    if ((list[i] is Filth) && list[i].def.defName == Props.filthType)
                                    {
                                        filthNumber++;
                                    }
                                }
                                if (filthNumber < 3)
                                {
                                    Thing thing = ThingMaker.MakeThing(ThingDef.Named(Props.filthType), null);
                                    thing.Rotation = Rot4.North;
                                    thing.Position = current;
                                    thing.SpawnSetup(pawn.Map, false);
                                }


                            }

                        }


                    }
                }

            }
           



        }
    }
}