using Verse;
using Verse.AI;
using Verse.Sound;

namespace VFECore
{
    public class CompProperties_AmbientSound : CompProperties
    {
        public SoundDef ambientSound;
        public CompProperties_AmbientSound()
        {
            this.compClass = typeof(CompAmbientSound);
        }
    }
    public class CompAmbientSound : ThingComp
    {
        private Sustainer sustainerAmbient;
        public CompProperties_AmbientSound Props => base.props as CompProperties_AmbientSound;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                SoundInfo info = SoundInfo.InMap(this.parent);
                if (this.parent is Pawn pawn)
                {
                    if (pawn.pather is null) pawn.pather = new Pawn_PathFollower(pawn);
                    if (pawn.stances is null) pawn.stances = new Pawn_StanceTracker(pawn);
                }
                sustainerAmbient = Props.ambientSound.TrySpawnSustainer(info);
            });
        }
        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            if (sustainerAmbient != null)
            {
                sustainerAmbient.End();
            }
        }
    }
}

