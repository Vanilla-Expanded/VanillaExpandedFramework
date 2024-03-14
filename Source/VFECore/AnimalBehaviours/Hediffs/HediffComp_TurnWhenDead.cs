using RimWorld;
using Verse;
using Verse.Sound;
using System;
using VFECore;

namespace AnimalBehaviours
{
    public class HediffComp_TurnWhenDead : HediffComp
    {


        public HediffCompProperties_TurnWhenDead Props
        {
            get
            {
                return (HediffCompProperties_TurnWhenDead)this.props;
            }
        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {

            float severityToTurn = Props.severityToTurn;

            Map map = this.parent.pawn.Corpse.Map;
            if (map != null && this.parent.Severity > severityToTurn)
            {
                Gender oldGender = this.parent.pawn.gender;
                Faction faction = null;
                if (Props.isHostile)
                {
                    faction = Find.FactionManager.FirstFactionOfDef(FactionDef.Named(Props.factionToTurnTo));
                }
                int numToSpawn = Rand.RangeInclusive(Props.numberOfSpawn[0], Props.numberOfSpawn[1]);
                for (int i = 0; i < numToSpawn; i++)
                {
                    PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDef.Named(Props.thingToTurnTo), faction, PawnGenerationContext.NonPlayer, -1, false, true, false, false, true, 1f, false, false, true, true, false, false);
                    Pawn pawn = PawnGenerator.GeneratePawn(request);
                    PawnUtility.TrySpawnHatchedOrBornPawn(pawn, this.parent.pawn.Corpse);
                    if (Props.keepGender)
                    {
                        pawn.gender = oldGender;
                    }
                    if (Props.isHostile)
                    {
                        pawn.mindState.mentalStateHandler.TryStartMentalState(DefDatabase<MentalStateDef>.GetNamed("ManhunterPermanent", true), null, true, false);
                    }

                }

                for (int i = 0; i < 20; i++)
                {
                    IntVec3 c;
                    CellFinder.TryFindRandomReachableNearbyCell(this.parent.pawn.Corpse.Position, map, 2, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), null, null, out c);

                    FilthMaker.TryMakeFilth(c, this.parent.pawn.Corpse.Map, ThingDefOf.Filth_Blood);

                }


                VFEDefOf.Hive_Spawn.PlayOneShot(new TargetInfo(this.parent.pawn.Corpse.Position, map, false));
                this.parent.pawn.Corpse.Destroy();

            }

        }


    }
}
