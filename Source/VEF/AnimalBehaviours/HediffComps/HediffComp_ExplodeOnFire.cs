﻿
using Verse;
using RimWorld;


namespace VEF.AnimalBehaviours
{
    public class HediffComp_ExplodeOnFire : HediffComp
    {
        public bool onCoolDown = false;
        public int coolDownCounter = 0;

        public HediffCompProperties_ExplodeOnFire Props
        {
            get
            {
                return (HediffCompProperties_ExplodeOnFire)this.props;
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look<bool>(ref this.onCoolDown, "onCoolDown", false, true);
            Scribe_Values.Look<int>(ref this.coolDownCounter, "coolDownCounter", 0, false);

        }

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);

            if (!onCoolDown && this.parent.pawn.IsHashIntervalTick(Props.checkInterval, delta) && this.parent.pawn.Map!=null && this.parent.pawn.IsBurning()) {

                Fire fire = (Fire)this.parent.pawn.GetAttachment(ThingDefOf.Fire);

                if (fire.fireSize >= Props.minFireToExplode)
                {
                    GenExplosion.DoExplosion(this.parent.pawn.Position, this.parent.pawn.Map, Props.radius, Props.damageType, this.parent.pawn,Props.damageAmount, -1, null, null, null, null, null, 0f, 1, null, null, 255,false, null, 0f, 1);
                    onCoolDown = true;

                }

            }

            if (onCoolDown)
            {
                coolDownCounter += delta;
                if (coolDownCounter > Props.ticksToRecheck)
                {
                    onCoolDown = false;
                }


            }


        }




    }
}
