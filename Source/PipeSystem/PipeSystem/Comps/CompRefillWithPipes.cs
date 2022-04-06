using RimWorld;

namespace PipeSystem
{
    public class CompRefillWithPipes : CompResource
    {
        public CompRefuelable compRefuelable;

        public new CompProperties_RefillWithPipes Props => (CompProperties_RefillWithPipes)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compRefuelable = parent.GetComp<CompRefuelable>();
        }
    }
}
