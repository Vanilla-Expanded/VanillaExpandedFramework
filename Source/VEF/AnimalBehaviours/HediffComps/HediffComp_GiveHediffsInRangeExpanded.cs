using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace VEF.AnimalBehaviours
{
    public class HediffComp_GiveHediffsInRangeExpanded : HediffComp
    {
        private Mote mote;

        public HediffCompProperties_GiveHediffsInRangeExpanded Props => (HediffCompProperties_GiveHediffsInRangeExpanded)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (!parent.pawn.Awake() || parent.pawn.health == null || parent.pawn.health.InPainShock || !parent.pawn.Spawned)
            {
                return;
            }
            if (!Props.hideMoteWhenNotDrafted || parent.pawn.Drafted)
            {
                if (Props.mote != null && (mote == null || mote.Destroyed))
                {
                    mote = MoteMaker.MakeAttachedOverlay(parent.pawn, Props.mote, Vector3.zero);
                }
                if (mote != null)
                {
                    mote.Maintain();
                }
            }
            List<Pawn> list = null;
            if (Props.onlyItsFaction)
            {
                list = parent.pawn.Map.mapPawns.PawnsInFaction(parent.pawn.Faction);
              
            }
            else
            {
                list = parent.pawn.Map.mapPawns.AllPawns;
            }
            
            foreach (Pawn item in list)
            {
                
                if (item.Dead || item.health == null || item == parent.pawn || !(item.Position.DistanceTo(parent.pawn.Position) <= Props.range) 
                    || !Props.targetingParameters.CanTarget(item) || ((Props.affectSameDef) && (item.def != parent.pawn.def)) ||
                    (!Props.needLOS || (Props.needLOS &&  !GenSight.LineOfSight(item.Position,parent.pawn.Position,parent.pawn.Map)) ))
                {
                    
                    continue;
                }
                Hediff hediff = item.health.hediffSet.GetFirstHediffOfDef(Props.hediff);
                if (hediff == null)
                {
                    hediff = item.health.AddHediff(Props.hediff, item.health.hediffSet.GetBrain());
                    hediff.Severity = Props.initialSeverity;
                    HediffComp_Link hediffComp_Link = hediff.TryGetComp<HediffComp_Link>();
                    if (hediffComp_Link != null)
                    {
                        hediffComp_Link.drawConnection = true;
                        hediffComp_Link.other = parent.pawn;
                    }
                }
                HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
                if (hediffComp_Disappears == null)
                {
                    Log.Error("HediffComp_GiveHediffsInRange has a hediff in props which does not have a HediffComp_Disappears");
                }
                else
                {
                    hediffComp_Disappears.ticksToDisappear = 5;
                }
            }
        }
    }
}
