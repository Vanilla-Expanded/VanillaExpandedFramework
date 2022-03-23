using Verse;

namespace PipeSystem
{
    public class CompProperties_RefillRefuelable : CompProperties_Resource
    {
        public CompProperties_RefillRefuelable()
        {
            compClass = typeof(CompRefillRefuelable);
        }

        public int ratio = 1;
        public ThingDef thing;
    }
}
