using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.Sound;
using VFECore;

namespace AnimalBehaviours
{
    public class CompSummonOnSpawn : ThingComp
    {
        private bool summonOnce = true;

        public void ExposeData()
        {
            Scribe_Values.Look<bool>(ref this.summonOnce, "summonOnce", true, false);
        }

        public CompProperties_SummonOnSpawn Props
        {
            get
            {
                return (CompProperties_SummonOnSpawn)this.props;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            //SummonOnce guarantees it only happens once when the animal spawns
            if (summonOnce)
            {
                if (this.parent.Map != null)
                {
                    int numToSpawn = Rand.RangeInclusive(Props.groupMinMax[0], Props.groupMinMax[1]);
                    for (int i = 0; i < numToSpawn; i++)
                    {
                        PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDef.Named(Props.pawnDef), Find.FactionManager.FirstFactionOfDef(FactionDefOf.AncientsHostile), PawnGenerationContext.NonPlayer, -1, false, false, false, false, true, 1f, false, false, true, true, false, false);
                        Pawn pawn = PawnGenerator.GeneratePawn(request);
                        GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(parent.Position, parent.Map, 3, null), parent.Map, WipeMode.Vanish);
                        if (Props.summonsAreManhunters)
                        {
                            pawn.mindState.mentalStateHandler.TryStartMentalState(DefDatabase<MentalStateDef>.GetNamed("ManhunterPermanent", true), null, true, false, false,null, false);
                        }
                    }
                    VFEDefOf.Hive_Spawn.PlayOneShot(new TargetInfo(this.parent.Position, this.parent.Map, false));
                    summonOnce = false;
                }
            }
        }
    }
}

