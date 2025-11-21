using RimWorld;
using Verse;
using VEF.Global;

namespace VEF.AnimalBehaviours
{
    public class HediffComp_SpawnMote : HediffComp
    {
        public HediffCompProperties_SpawnMote Props => base.props as HediffCompProperties_SpawnMote;

        public Mote spawnedMote;
        public override void CompPostTick(ref float severityAdjustment)
        {
            if (spawnedMote is null)
            {
                spawnedMote = MoteMaker.MakeAttachedOverlay(Pawn, Props.moteDef, Props.offset);
                if (spawnedMote is MoteAttachedScaled scaled)
                {
                    scaled.maxScale = Props.maxScale;
                }
            }
            if (spawnedMote.def.mote.needsMaintenance)
            {
                spawnedMote.Maintain();
            }
            base.CompPostTick(ref severityAdjustment);
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_References.Look(ref spawnedMote, "spawnedMote");
        }
    }
}
