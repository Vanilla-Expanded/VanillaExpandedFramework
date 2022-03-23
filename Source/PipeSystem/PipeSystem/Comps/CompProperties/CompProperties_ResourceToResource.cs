namespace PipeSystem
{
    public class CompProperties_ResourceToResource : CompProperties_Resource
    {
        public CompProperties_ResourceToResource()
        {
            compClass = typeof(CompResourceToResource);
        }

        public PipeNet toNet;
    }
}
