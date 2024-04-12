using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.Sound;
using UnityEngine;
using System.Collections;
using VFECore;

namespace AnimalBehaviours
{
    public class CompMetamorphosis : ThingComp
    {

        public int metamorphosisTick = 0;

        public int rareTicksInAYear = 14400;

        public CompProperties_Metamorphosis Props
        {
            get
            {
                return (CompProperties_Metamorphosis)this.props;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look<int>(ref this.metamorphosisTick, "metamorphosisTick", 0, false);

        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            if (parent.Map != null)
            {
                metamorphosisTick++;
                //Only do it ater rareTicksInAYear * Props.timeInYears time has passed
                if (metamorphosisTick > rareTicksInAYear * Props.timeInYears)
                {
                    //Keep parent's faction, BUT only faction
                    Faction faction = this.parent.Faction;

                    PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDef.Named(Props.pawnToTurnInto), faction, PawnGenerationContext.NonPlayer, -1, false, true, false, false, true, 1f, false, false, true, true, false, false);
                    Pawn pawn = PawnGenerator.GeneratePawn(request);
                    GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(parent.Position, parent.Map, 3, null), parent.Map, WipeMode.Vanish);

                    //Produce some filth
                    for (int i = 0; i < 20; i++)
                    {
                        IntVec3 c;
                        CellFinder.TryFindRandomReachableNearbyCell(this.parent.Position, this.parent.Map, 2, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), null, null, out c);
                        FilthMaker.TryMakeFilth(c, this.parent.Map, ThingDefOf.Filth_AmnioticFluid);
                    }
                    //Play insect spawn sound effect
                    VFEDefOf.Hive_Spawn.PlayOneShot(new TargetInfo(this.parent.Position, this.parent.Map, false));
                    //Destroy the parent
                    this.parent.Destroy();
                }
            }
        }

        public override string CompInspectStringExtra()
        {
            int tickNumber = (int)(((rareTicksInAYear * Props.timeInYears) - metamorphosisTick) * 250);
            return (Props.reportString).Translate(tickNumber.ToStringTicksToPeriod(false, false, false, true));
        }


    }
}
