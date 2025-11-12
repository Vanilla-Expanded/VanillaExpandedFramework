using RimWorld;
using Verse;

namespace PipeSystem
{
    [StaticConstructorOnStartup]
    public class Building_PipeValve : Building
    {
        private CompFlickable flickableComp;
        public override Graphic Graphic => flickableComp.CurrentGraphic;

        public override void PostMake()
        {
            base.PostMake();
            InitializeValveComps();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.LoadingVars)
                InitializeValveComps();
        }

        private void InitializeValveComps()
        {
            flickableComp = GetComp<CompFlickable>();
        }
    }
}
