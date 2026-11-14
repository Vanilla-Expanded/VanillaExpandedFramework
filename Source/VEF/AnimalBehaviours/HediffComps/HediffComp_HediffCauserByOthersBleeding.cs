

using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using static UnityEngine.GraphicsBuffer;

namespace VEF.AnimalBehaviours
{
    public class HediffComp_HediffCauserByOthersBleeding : HediffComp
    {
        public List<Pawn> pawnList = new List<Pawn>();    

        public HediffCompProperties_HediffCauserByOthersBleeding Props
        {
            get
            {
                return (HediffCompProperties_HediffCauserByOthersBleeding)this.props;
            }
        }

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);

            //Only do anything every tickInterval
            if (Pawn.IsHashIntervalTick(Props.tickInterval, delta))
            {
                Pawn thisPawn = this.parent.pawn as Pawn;
                //Null map check. Also will only work if pawn is not dead or downed, and if needsToBeTamed is true, that the animal is tamed
                if (thisPawn != null && thisPawn.Map != null && !thisPawn.Dead && !thisPawn.Downed && (!Props.needsToBeTamed || (Props.needsToBeTamed && thisPawn.Faction != null && thisPawn.Faction.IsPlayer)))
                {
                    bool anyoneBleeding = false;

                    foreach (Thing thing in GenRadial.RadialDistinctThingsAround(thisPawn.Position, thisPawn.Map, Props.radius, true))
                    {
                        if (anyoneBleeding) break;

                        Pawn pawn = thing as Pawn;
                       
                        if (pawn != null && pawn.RaceProps.IsFlesh && pawn != this.parent.pawn)
                        {
                            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                            {
                                if (hediff.Bleeding)
                                {
                                    anyoneBleeding = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (anyoneBleeding)
                    {
                        Hediff hediff = thisPawn.health.hediffSet.GetFirstHediffOfDef(Props.newHediff);
                        if (hediff == null)
                        {
                            hediff = thisPawn.health.AddHediff(Props.newHediff);
                            hediff.Severity = 1f;
                            
                        }
                        else
                        {
                            HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
                            if (hediffComp_Disappears != null)
                            {
                                hediffComp_Disappears.ticksToDisappear = hediffComp_Disappears.Props.disappearsAfterTicks.RandomInRange;
                            }
                           
                        }

                    }
                }
            }
        }
    }
}
