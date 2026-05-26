
using RimWorld;
using Verse;
using Verse.AI.Group;
using Verse.Noise;
using Verse.Sound;

namespace VEF.AnimalBehaviours

{
    public class DeathActionWorker_DivideAndCreateFilth : DeathActionWorker
    {
        public DeathActionProperties_DivideAndCreateFilth Props => (DeathActionProperties_DivideAndCreateFilth)props;

        public override void PawnDied(Corpse corpse, Lord prevLord)
        {

            Pawn innerPawn = corpse.InnerPawn;
            if (innerPawn == null)
            {
                return;
            }

            for (int i = 0; i < Props.dividePawnKindOptions.Count; i++)
            {
                PawnKindDef kind = Props.dividePawnKindOptions[i];
                Faction faction = corpse.InnerPawn.Faction;
                float? fixedBiologicalAge = 0f;
                Pawn child = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, faction, PawnGenerationContext.NonPlayer, fixedBiologicalAge: fixedBiologicalAge));
                SpawnPawn(child, innerPawn, corpse.PositionHeld, corpse.MapHeld, prevLord);
            }

            for (int i = 0; i < Props.filthCountRange.RandomInRange; i++)
            {
                IntVec3 c;
                CellFinder.TryFindRandomReachableNearbyCell(corpse.PositionHeld, corpse.MapHeld, 2, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), null, null, out c);
                FilthMaker.TryMakeFilth(c, corpse.MapHeld, Props.filthCreated);
            }
            if (Props.sound != null) {
                Props.sound.PlayOneShot(new TargetInfo(corpse.PositionHeld, corpse.MapHeld, false));
            }

            corpse.Destroy();
        }

        private void SpawnPawn(Pawn child, Pawn parent, IntVec3 position, Map map, Lord lord)
        {
            GenSpawn.Spawn(child, position, map, WipeMode.VanishOrMoveAside);
            lord?.AddPawn(child);

            FleshbeastUtility.SpawnPawnAsFlyer(child, map, position);
        }
    }
}