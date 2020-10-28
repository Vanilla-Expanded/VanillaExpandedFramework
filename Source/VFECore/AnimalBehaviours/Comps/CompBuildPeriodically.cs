using RimWorld;
using Verse;
using System.Collections.Generic;

namespace AnimalBehaviours
{
    public class CompBuildPeriodically : ThingComp
    {

        private Effecter effecter;

        public CompProperties_BuildPeriodically Props
        {
            get
            {
                return (CompProperties_BuildPeriodically)this.props;
            }
        }


        public override void CompTick()
        {
            base.CompTick();
            if (this.parent.IsHashIntervalTick(Props.ticksToBuild))
            {
                this.TryCreateBuilding();
            }
        }

        public void TryCreateBuilding()
        {
            if (this.parent.Map!=null&&this.parent.Map.listerThings.ThingsOfDef(ThingDef.Named(Props.defOfBuilding)).Count < Props.maxBuildingsPerMap)
            {
                Pawn pawn = this.parent as Pawn;

                if ((pawn.Map != null) && (pawn.Awake()) && (!pawn.Downed))
                {
                    if (Props.acceptedTerrains != null)
                    {
                        if (Props.acceptedTerrains.Contains(pawn.Position.GetTerrain(pawn.Map).defName))
                        {
                            ThingDef newThing = ThingDef.Named(this.Props.defOfBuilding);
                            Thing newbuilding = GenSpawn.Spawn(newThing, pawn.Position, pawn.Map, WipeMode.Vanish);

                            if (this.effecter == null)
                            {
                                this.effecter = EffecterDefOf.Mine.Spawn();
                            }
                            this.effecter.Trigger(pawn, newbuilding);
                        }
                    }
                    else
                    {
                        ThingDef newThing = ThingDef.Named(this.Props.defOfBuilding);
                        Thing newbuilding = GenSpawn.Spawn(newThing, pawn.Position, pawn.Map, WipeMode.Vanish);

                        if (this.effecter == null)
                        {
                            this.effecter = EffecterDefOf.Mine.Spawn();
                        }
                        this.effecter.Trigger(pawn, newbuilding);

                    }

                }
            }

            
        }





    }
}
