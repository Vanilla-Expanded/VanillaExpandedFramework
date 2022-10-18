
using Verse;
using RimWorld;
using System.Collections.Generic;
namespace AnimalBehaviours
{
    class HediffComp_Electrified : HediffComp
    {
        public HediffCompProperties_Electrified Props
        {
            get
            {
                return (HediffCompProperties_Electrified)this.props;
            }
        }
        public int tickCounter = 0;

       


        public override void CompPostTick(ref float severityAdjustment)
        {
            
            if (parent.pawn.Map != null)
            {
                tickCounter++;
                
                if (tickCounter >= Props.electroRate)
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
                    tickCounter = 0;
                }
            }
        }


    }
}
