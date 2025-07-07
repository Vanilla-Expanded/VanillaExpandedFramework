
using UnityEngine;
using Verse;
using System.Collections.Generic;
using RimWorld;


namespace VEF.AnimalBehaviours
{
    public class CompRefueling : ThingComp
    {
        public CompProperties_Refueling Props
        {
            get
            {
                return (CompProperties_Refueling)this.props;
            }
        }


        public override void CompTickInterval(int delta)
        {
            //null map check
            if (this.parent.Map != null && AnimalBehaviours_Settings.flagChargeBatteries && (!Props.mustBeTamed || (this.parent.Faction!=null &&this.parent.Faction.IsPlayer)))
            {
                //Only do every fuelingRate ticks
                if (parent.IsHashIntervalTick(Props.fuelingRate, delta))
                {
                    Pawn pawn = this.parent as Pawn;
                    CellRect rect = GenAdj.OccupiedRect(pawn.Position, pawn.Rotation, IntVec2.One);
                    rect = rect.ExpandedBy(Props.fuelingRadius);
                    List<Building> buildingsInRange = new List<Building>();
                    foreach (IntVec3 current in rect.Cells)
                    {
                        if (current.InBounds(pawn.Map))
                        {
                            Building edifice = current.GetEdifice(pawn.Map);
                            //If any buildings were found in the requested area
                            if (edifice != null)
                            {
                               
                                foreach (string defNameOfBuilding in Props.buildingsToAffect)
                                {
                                    if (edifice.def.defName == defNameOfBuilding)
                                    {
                                        buildingsInRange.Add(edifice);
                                    }
                                }
                            }
                        }
                    }
                    //If any building was found
                    if (buildingsInRange.Count > 0)
                    {
                        //Affect a random one if more than one was found
                        Building buildingToAffect = buildingsInRange.RandomElement();
                        CompRefuelable comp = buildingToAffect.TryGetComp<CompRefuelable>();
                        if (comp != null)
                        {
                            comp.Refuel(1);
                        }
                       
                    }
                }
            }
        }
    }
}

