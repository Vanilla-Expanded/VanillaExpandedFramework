using Verse;

namespace PipeSystem
{
    public class CompProperties_ResourceProcessor : CompProperties_Resource
    {
        public CompProperties_ResourceProcessor()
        {
            compClass = typeof(CompResourceProcessor);
        }

        public bool showBufferInfo = true;
        public float bufferSize;

        public string notWorkingKey;

        public int eachTicks;
        public Result result;
    }

    public class Result
    {
        public ThingDef thing;
        public int thingCount;

        public PipeNetDef net;
        public int netCount;
    }
}
