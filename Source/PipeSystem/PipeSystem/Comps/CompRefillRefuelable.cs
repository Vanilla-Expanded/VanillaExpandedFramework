using RimWorld;

namespace PipeSystem
{
    public class CompRefillRefuelable : CompResource
    {
        public CompRefuelable compRefuelable;

        public new CompProperties_RefillRefuelable Props => (CompProperties_RefillRefuelable)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compRefuelable = parent.GetComp<CompRefuelable>();
        }
    }
}
