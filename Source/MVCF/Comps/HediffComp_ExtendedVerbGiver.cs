using System.Linq;
using Verse;

namespace MVCF.Comps
{
    public class HediffComp_ExtendedVerbGiver : HediffComp_VerbGiver
    {
        public new HediffCompProperties_ExtendedVerbGiver Props => props as HediffCompProperties_ExtendedVerbGiver;

        public AdditionalVerbProps PropsFor(Verb verb)
        {
            var label = verb.verbProps.label;
            return string.IsNullOrEmpty(label)
                ? null
                : Props.verbProps?.FirstOrDefault(verbProps => verbProps.label == label);
        }
    }
}