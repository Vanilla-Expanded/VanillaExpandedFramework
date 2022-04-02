using Verse;

namespace PipeSystem
{
    /// <summary>
    /// Use custom linked graphic
    /// </summary>
    public class Building_Pipe : Building
    {
        public CompResource compResource;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            compResource = GetComp<CompResource>();
        }

        public override Graphic Graphic => LinkedPipes.GetPipeFor(def);
    }
}