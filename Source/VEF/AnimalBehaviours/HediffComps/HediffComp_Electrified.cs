﻿
using Verse;
using RimWorld;
using System.Collections.Generic;
namespace VEF.AnimalBehaviours
{
    public class HediffComp_Electrified : HediffComp
    {
        public HediffCompProperties_Electrified Props
        {
            get
            {
                return (HediffCompProperties_Electrified)this.props;
            }
        }


        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            if (parent.pawn.Map != null)
            {
                if (Pawn.IsHashIntervalTick(Props.electroRate, delta))
                {
                    Pawn pawn = this.parent.pawn;
                    CellRect rect = GenAdj.OccupiedRect(pawn.Position, pawn.Rotation, IntVec2.One);
                    rect = rect.ExpandedBy(Props.electroRadius);
                    List<Building> batteriesInRange = new List<Building>();
                    foreach (IntVec3 current in rect.Cells)
                    {
                        if (current.InBounds(pawn.Map))
                        {
                            Building edifice = current.GetEdifice(pawn.Map);
                           
                            if (edifice != null)
                            {
                               
                                foreach (string defNameOfBattery in Props.batteriesToAffect)
                                {
                                    if (edifice.def.defName == defNameOfBattery)
                                    {
                                        batteriesInRange.Add(edifice);
                                    }
                                }
                            }
                        }
                    }
                  
                    if (batteriesInRange.Count > 0)
                    {
                       
                        Building batteryToAffect = batteriesInRange.RandomElement();
                        FleckMaker.ThrowMicroSparks(batteryToAffect.Position.ToVector3(), batteryToAffect.Map);
                        foreach (CompPowerBattery current2 in batteryToAffect.GetComps<CompPowerBattery>())
                        {
                           
                            current2.AddEnergy(Props.electroChargeAmount);
                            break;
                        }

                       
                        if (ModLister.HasActiveModWithName("Alpha Animals") && pawn.Faction == Faction.OfPlayer)
                        {
                            pawn.health.AddHediff(HediffDef.Named("AA_RechargingBatteries"));
                        }


                    }
                }
            }
        }


    }
}
