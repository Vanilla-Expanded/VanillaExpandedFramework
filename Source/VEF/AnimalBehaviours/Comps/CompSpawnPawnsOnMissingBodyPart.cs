using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace VEF.AnimalBehaviours
{
    class CompSpawnPawnsOnMissingBodyPart : ThingComp
    {
        public int existingMissingBodyParts = 0;

        public CompProperties_SpawnPawnsOnMissingBodyPart Props
        {
            get
            {
                return (CompProperties_SpawnPawnsOnMissingBodyPart)this.props;
            }
        }

        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {

            Pawn pawn = this.parent as Pawn;

            if (pawn != null && !pawn.Dead)
            {
                List<BodyPartRecord> allParts = pawn.def.race.body.AllParts;
                List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
                int missingPartCount = 0;
                for (int i = 0; i < hediffs.Count; i++)
                {
                    Hediff_MissingPart hediff_MissingPart = hediffs[i] as Hediff_MissingPart;
                    if (hediff_MissingPart != null && allParts.Contains(hediff_MissingPart.Part))
                    {
                        missingPartCount++;
                    }
                }
                if(missingPartCount> existingMissingBodyParts)
                {
                    existingMissingBodyParts=missingPartCount;

                    for (int i = 0; i < Props.pawnKindOptions.Count; i++)
                    {
                        PawnKindDef kind = Props.pawnKindOptions[i];
                        Faction faction = pawn.Faction;
                        float? fixedBiologicalAge = 0f;
                        Pawn child = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, faction, PawnGenerationContext.NonPlayer, fixedBiologicalAge: fixedBiologicalAge));
                        SpawnPawn(child, pawn, pawn.PositionHeld, pawn.MapHeld, pawn.lord);
                    }

                    for (int i = 0; i < Props.filthCountRange.RandomInRange; i++)
                    {
                        IntVec3 c;
                        CellFinder.TryFindRandomReachableNearbyCell(pawn.PositionHeld, pawn.MapHeld, 2, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), null, null, out c);
                        FilthMaker.TryMakeFilth(c, pawn.MapHeld, Props.filthCreated);
                    }
                    if (Props.sound != null)
                    {
                        Props.sound.PlayOneShot(new TargetInfo(pawn.PositionHeld, pawn.MapHeld, false));
                    }

                }

            }

        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref this.existingMissingBodyParts, "existingMissingBodyParts", 0, false);
        }

        private void SpawnPawn(Pawn child, Pawn parent, IntVec3 position, Map map, Lord lord)
        {
            GenSpawn.Spawn(child, position, map, WipeMode.VanishOrMoveAside);
            lord?.AddPawn(child);
            FleshbeastUtility.SpawnPawnAsFlyer(child, map, position);
        }

    }
}
