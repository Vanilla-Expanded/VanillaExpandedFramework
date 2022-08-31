using RimWorld;
using UnityEngine;
using Verse;

namespace VFECore
{
    public class HediffCompProperties_Mote : HediffCompProperties
    {
        public ThingDef mote;

        public float scale;
        public HediffCompProperties_Mote()
        {
            this.compClass = typeof(HediffComp_Mote);
        }
    }
    public class HediffComp_Mote : HediffComp
    {
        public Mote mote;
        public HediffCompProperties_Mote Props => base.props as HediffCompProperties_Mote;
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (mote is null || mote.Destroyed)
            {
                mote = MoteMaker.MakeAttachedOverlay(this.Pawn, Props.mote, Vector3.zero, Props.scale);
            }
            else
            {
                mote.Maintain();
            }
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            if (mote != null)
            {
                mote.Destroy();
            }
        }
    }
}