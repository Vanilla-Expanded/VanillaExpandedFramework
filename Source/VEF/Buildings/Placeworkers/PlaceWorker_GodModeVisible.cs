using Verse;

namespace VEF.Buildings
{
    public class PlaceWorker_GodModeVisible : PlaceWorker
    {
        public override bool IsBuildDesignatorVisible(BuildableDef def)
        {
            return DebugSettings.godMode;
        }
    }
}
