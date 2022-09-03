using RimWorld;
using Verse;

namespace PipeSystem
{
    [StaticConstructorOnStartup]
    public class Building_PipeValve : Building
    {
        private CompFlickable flickableComp;
        public override Graphic Graphic => flickableComp.CurrentGraphic;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            flickableComp = GetComp<CompFlickable>();
            base.SpawnSetup(map, respawningAfterLoad);
        }
    }
}
