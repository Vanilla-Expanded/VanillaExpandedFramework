using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VEF.Storyteller
{
    public class QuestPart_KeepSite : QuestPart
    {
        public MapParent mapParent;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref mapParent, "mapParent");
        }
    }
}
