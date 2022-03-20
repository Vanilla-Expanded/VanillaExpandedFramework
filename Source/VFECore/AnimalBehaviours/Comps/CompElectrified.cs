
using UnityEngine;
using Verse;
using System.Collections.Generic;
using RimWorld;


namespace AnimalBehaviours
{
    public class CompElectrified : ThingComp
    {
        public int tickCounter = 0;

        public CompProperties_Electrified Props
        {
            get
            {
                return (CompProperties_Electrified)this.props;
            }
        }

        protected int electroRate
        {
            get
            {
                return this.Props.electroRate;
            }
        }

        protected int electroRadius
        {
            get
            {
                return this.Props.electroRadius;
            }
        }

        protected int electroChargeAmount
        {
            get
            {
                return this.Props.electroChargeAmount;
            }
        }

        public override void CompTick()
        {
            //null map check
            if (this.parent.Map != null && AnimalBehaviours_Settings.flagChargeBatteries)
            {
                tickCounter++;
                //Only do every electroRate ticks
                if (tickCounter >= electroRate)
                {
                    Pawn pawn = this.parent as Pawn;
                    CellRect rect = GenAdj.OccupiedRect(pawn.Position, pawn.Rotation, IntVec2.One);
                    rect = rect.ExpandedBy(electroRadius);
                    List<Building> batteriesInRange = new List<Building>();
                    foreach (IntVec3 current in rect.Cells)
                    {
                        if (current.InBounds(pawn.Map))
                        {
                            Building edifice = current.GetEdifice(pawn.Map);
                            //If any buildings were found in the requested area
                            if (edifice != null)
                            {
                                //Check their defNames, and see if they are in the list of provided batteries
                                //in the XML. They can be modded batteries. Example nice list of modded batteries:
                                //< li > Battery </ li >
                                //< li > Battery_Silver </ li >
                                //< li > Battery_Gold </ li >
                                //< li > Battery_Plasteel </ li >
                                //< li > Battery_Uranium </ li >
                                //< li > Battery_Advanced </ li >
                                //< li > Battery_Vanometric </ li >
                                //< li > Battery4k </ li >
                                //< li > Battery16k </ li >
                                //< li > Battery64k </ li >
                                //< li > Battery256k </ li >
                                //< li > ChargeBack_Battery_Prototype </ li >
                                //< li > ChargeBack_Battery </ li >
                                //< li > SpeedCharge_Battery </ li >
                                //< li > ResonanceCell_Battery </ li >
                                //< li > VoidCell_Battery </ li >
                                //< li > VFE_LargeBattery </ li >
                                //< li > VFE_SmallBattery </ li >
                                //< li > VFE_AdvancedBattery </ li >
                                //< li > VFE_LargeAdvancedBattery </ li >
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
                    //If any battery was found
                    if (batteriesInRange.Count > 0)
                    {
                        //Affect a random one if more than one was found
                        Building batteryToAffect = batteriesInRange.RandomElement();
                        FleckMaker.ThrowMicroSparks(batteryToAffect.Position.ToVector3(), batteryToAffect.Map);
                        foreach (CompPowerBattery current2 in batteryToAffect.GetComps<CompPowerBattery>())
                        {
                            //Add specified amount of energy. The rate it fills is thus defined by electroRate and electroChargeAmount
                            current2.AddEnergy(electroChargeAmount);
                            break;
                        }

                        //This is for achievements
                        if (ModLister.HasActiveModWithName("Alpha Animals")&&pawn.Faction == Faction.OfPlayer)
                        {
                            pawn.health.AddHediff(HediffDef.Named("AA_RechargingBatteries"));
                        }

                        
                    }
                    tickCounter = 0;
                }
            }
        }
    }
}

