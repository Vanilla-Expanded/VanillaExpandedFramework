using System.Text;
using RimWorld;
using Verse;

namespace VEF.AnimalBehaviours
{
    public class StatWorker_AnimalProductYieldFactor : StatWorker
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
                    if (comp.Props.resourceDef != null)
                    {
                        stringBuilder.AppendLine("VEF_AnimalProductYieldFactorDetails".Translate(comp.Props.resourceAmount, comp.Props.resourceDef.label, (int)(comp.Props.resourceAmount * finalVal)));
                    }
                    else if (!comp.Props.randomItems.NullOrEmpty() || !comp.Props.seasonalItems.NullOrEmpty())
                    {
                        stringBuilder.AppendLine("VEF_AnimalProductYieldFactorDetails_Random".Translate(comp.Props.resourceAmount, (int)(comp.Props.resourceAmount * finalVal)));
                    }
                   
                    stringBuilder.AppendLine();
                }
            }
            return stringBuilder.ToString();
        }
    }
}