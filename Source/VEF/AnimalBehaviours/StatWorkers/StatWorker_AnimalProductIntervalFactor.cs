using System;
using System.Text;
using RimWorld;
using Verse;

namespace VEF.AnimalBehaviours
{
    public class StatWorker_AnimalProductIntervalFactor : StatWorker
    {
        public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(base.GetExplanationFinalizePart(req, numberSense, finalVal));
            stringBuilder.AppendLine();

            Pawn pawn = req.Pawn ?? (req.Thing as Pawn);
            if (pawn != null)
            {
                CompAnimalProduct comp = pawn.TryGetComp<CompAnimalProduct>();
                if (comp != null)
                {
                    stringBuilder.AppendLine("VEF_AnimalProductIntervalFactorDetails".Translate(comp.Props.gatheringIntervalDays, Math.Max((int)(comp.Props.gatheringIntervalDays * finalVal), 1)));
                    stringBuilder.AppendLine();
                }
            }
            return stringBuilder.ToString();
        }
    }
}