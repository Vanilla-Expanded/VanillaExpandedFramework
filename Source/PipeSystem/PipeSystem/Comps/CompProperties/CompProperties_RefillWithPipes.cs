using Verse;

namespace PipeSystem
{
    public class CompProperties_RefillWithPipes : CompProperties_Resource
    {
        public CompProperties_RefillWithPipes()
        {
            compClass = typeof(CompRefillWithPipes);
        }

        public int ratio = 1;
        public ThingDef thing;
    }
}
