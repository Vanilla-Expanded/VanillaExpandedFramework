﻿using RimWorld;
using Verse;
using Verse.Sound;
using VEF;

namespace VEF.AnimalBehaviours

{
    public class CompDieAfterPeriod : ThingComp
    {
        public int tickCounter = 0;

        public CompProperties_DieAfterPeriod Props
        {
            get
            {
                return (CompProperties_DieAfterPeriod)this.props;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref this.tickCounter, "tickCounter", 0, false);

        }

        public override void CompTickInterval(int delta)
        {
            base.CompTickInterval(delta);
            tickCounter += delta;

            if (tickCounter >= Props.timeToDieInTicks)
            {
                if (this.parent is Pawn pawn && pawn.Map != null)
                {

                    if (Props.effect)
                    {
                        for (int i = 0; i < 20; i++)
                        {
                            IntVec3 c;
                            CellFinder.TryFindRandomReachableNearbyCell(this.parent.Position, this.parent.Map, 2, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), null, null, out c);
                            FilthMaker.TryMakeFilth(c, this.parent.Map, ThingDef.Named(Props.effectFilth));
                        }
                        VEFDefOf.Hive_Spawn.PlayOneShot(new TargetInfo(this.parent.Position, this.parent.Map, false));
                    }
                    if (Props.justVanish)
                    {
                        pawn.Destroy();
                    }
                    else { pawn.Kill(null);}
                    
                }
                tickCounter = 0;
            }
        }

        public override string CompInspectStringExtra()
        {


            string text = base.CompInspectStringExtra();
            string timeToLive = "VEF_TimeToDie".Translate((Props.timeToDieInTicks - tickCounter).ToStringTicksToPeriod(true, false, true, true));



            return text + timeToLive;
        }

    }
}
