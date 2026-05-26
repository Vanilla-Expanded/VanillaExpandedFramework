using RimWorld;
using Verse;
using VEF.Global;
using Verse.AI.Group;
using Verse.Sound;

namespace VEF.AnimalBehaviours
{
    public class HediffComp_SpawnPawnOnMaxSeverity : HediffComp
    {
        public HediffCompProperties_SpawnPawnOnMaxSeverity Props => base.props as HediffCompProperties_SpawnPawnOnMaxSeverity;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (parent.Severity >= 0.99f)
            {
                for (int i = 0; i < Props.pawnKindOptions.Count; i++)
                {
                    PawnKindDef kind = Props.pawnKindOptions[i];
                    Faction faction = Faction.OfInsects;
                    float? fixedBiologicalAge = 0f;
                    Pawn child = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, faction, PawnGenerationContext.NonPlayer, fixedBiologicalAge: fixedBiologicalAge));
                    SpawnPawn(child, Pawn, Pawn.PositionHeld, Pawn.MapHeld);
                }

                for (int i = 0; i < Props.filthCountRange.RandomInRange; i++)
                {
                    IntVec3 c;
                    CellFinder.TryFindRandomReachableNearbyCell(Pawn.PositionHeld, Pawn.MapHeld, 2, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), null, null, out c);
                    FilthMaker.TryMakeFilth(c, Pawn.MapHeld, Props.filthCreated);
                }
                if (Props.sound != null)
                {
                    Props.sound.PlayOneShot(new TargetInfo(Pawn.PositionHeld, Pawn.MapHeld, false));
                }
                Pawn.TakeDamage(new DamageInfo(Props.damage, Props.damageAmount.RandomInRange, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null));

                Pawn.health.RemoveHediff(this.parent);
            }
        } 
        

        private void SpawnPawn(Pawn child, Pawn parent, IntVec3 position, Map map)
        {
            GenSpawn.Spawn(child, position, map, WipeMode.VanishOrMoveAside);
            
            FleshbeastUtility.SpawnPawnAsFlyer(child, map, position);
        }
    }
}
