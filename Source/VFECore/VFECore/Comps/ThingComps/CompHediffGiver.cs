using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VFECore
{
    public class CompProperties_HediffGiver : CompProperties
    {
        public CompProperties_HediffGiver()
        {
            compClass = typeof(CompHediffGiver);
        }

        public HediffDef hediffDef;
        public float severityIncrease = 0f;
        public float radius;

        public List<StatDef> stats;

        public int tickRate = 500;
    }

    public class CompHediffGiver : ThingComp
    {
        public CompProperties_HediffGiver Props
        {
            get
            {
                return (CompProperties_HediffGiver)props;
            }
        }

        private int nextTest = 0;

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref nextTest, "nextTest");
            base.PostExposeData();
        }

        public override void PostPostMake()
        {
            nextTest = Find.TickManager.TicksGame + Props.tickRate;
            base.PostPostMake();
        }

        public override void CompTick()
        {
            base.CompTick();

            if (Find.TickManager.TicksGame == nextTest)
            {
                foreach (var thing in GenRadial.RadialDistinctThingsAround(parent.Position, parent.Map, Props.radius, true))
                {
                    if (thing is Pawn pawn)
                    {
                        float adjustedSeverity = Props.severityIncrease;
                        if (!Props.stats.NullOrEmpty())
                        {
                            foreach (StatDef stat in Props.stats)
                            {
                                adjustedSeverity *= pawn.GetStatValue(stat);
                            }
                        }

                        if (pawn.health.hediffSet.HasHediff(Props.hediffDef) && adjustedSeverity > 0f)
                        {
                            pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef).Severity += adjustedSeverity;
                        }
                        else if (adjustedSeverity > 0f)
                        {
                            Hediff hediff = HediffMaker.MakeHediff(Props.hediffDef, pawn);
                            hediff.Severity = adjustedSeverity;
                            pawn.health.AddHediff(hediff);
                        }
                    }
                }
                nextTest += Props.tickRate;
            }
        }
    }
}