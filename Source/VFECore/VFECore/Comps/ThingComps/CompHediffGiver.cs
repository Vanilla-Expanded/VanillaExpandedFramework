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

        public override void CompTick()
        {
            base.CompTick();

            if (!parent.IsHashIntervalTick(Props.tickRate) || !parent.Spawned)
            {
                return;
            }

            IReadOnlyList<Pawn> pawnList = parent.Map.mapPawns.AllPawnsSpawned;

            for (int i = pawnList.Count - 1; i >= 0; i--)
            {
                Pawn pawn = pawnList[i];

                if (pawn.Position.DistanceToSquared(parent.Position) > Props.radius * Props.radius)
                {
                    continue;
                }

                float adjustedSeverity = Props.severityIncrease;

                if (!Props.stats.NullOrEmpty())
                {
                    for (int j = 0; j < Props.stats.Count; j++)
                    {
                        adjustedSeverity *= pawn.GetStatValue(Props.stats[j]);
                    }
                }

                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef);

                if (hediff != null)
                {
                    hediff.Severity += adjustedSeverity;
                    continue;
                }

                hediff = HediffMaker.MakeHediff(Props.hediffDef, pawn);
                hediff.Severity = adjustedSeverity;
                pawn.health.AddHediff(hediff);
            }
        }
    }
}