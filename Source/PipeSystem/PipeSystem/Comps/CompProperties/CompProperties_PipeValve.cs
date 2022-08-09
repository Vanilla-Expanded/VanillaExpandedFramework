namespace PipeSystem
{
    public class CompProperties_PipeValve : CompProperties_Resource
    {
        public CompProperties_PipeValve()
        {
            compClass = typeof(CompPipeValve);
        }

        public bool alwaysLinkToPipes = false;
    }
}
