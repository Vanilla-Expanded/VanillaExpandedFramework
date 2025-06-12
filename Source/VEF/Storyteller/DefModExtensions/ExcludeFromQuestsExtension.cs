using Verse;

namespace VEF.Storyteller
{
    internal class ExcludeFromQuestsExtension : DefModExtension
    {
        private static readonly ExcludeFromQuestsExtension DefaultValues = new ExcludeFromQuestsExtension();

        public static ExcludeFromQuestsExtension Get(Def def) => def.GetModExtension<ExcludeFromQuestsExtension>() ?? DefaultValues;
    }
}