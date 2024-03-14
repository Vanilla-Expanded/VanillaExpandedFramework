
using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse.Sound;
using UnityEngine;
using VFECore;

namespace AnimalBehaviours
{
    class HediffComp_DieAfterPeriod : HediffComp
    {


        public int tickCounter = 0;

        public HediffCompProperties_DieAfterPeriod Props
        {
            get
            {
                return (HediffCompProperties_DieAfterPeriod)this.props;
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look<int>(ref this.tickCounter, "tickCounter", 0, false);

        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            tickCounter++;

            if (tickCounter >= Props.timeToDieInTicks)
            {
                Pawn pawn = this.parent.pawn as Pawn;

                if (pawn != null && pawn.Map != null)
                {

                    if (Props.effect)
                    {
                        for (int i = 0; i < 20; i++)
                        {
                            IntVec3 c;
                            CellFinder.TryFindRandomReachableNearbyCell(this.parent.pawn.Position, this.parent.pawn.Map, 2, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), null, null, out c);
                            FilthMaker.TryMakeFilth(c, this.parent.pawn.Map, ThingDef.Named(Props.effectFilth));
                        }
                        VFEDefOf.Hive_Spawn.PlayOneShot(new TargetInfo(this.parent.pawn.Position, this.parent.pawn.Map, false));
                    }
                    if (Props.justVanish)
                    {
                        pawn.Destroy();
                    }
                    else { pawn.Kill(null); }

                }
                tickCounter = 0;
            }
        }


        public override string CompLabelInBracketsExtra => GetLabel();




        public string GetLabel()
        {
            string timeToLive = Props.DescriptionLabel.Translate((Props.timeToDieInTicks - tickCounter).ToStringTicksToPeriod(true, false, true, true));

            return timeToLive;


        }

       

    }
}
