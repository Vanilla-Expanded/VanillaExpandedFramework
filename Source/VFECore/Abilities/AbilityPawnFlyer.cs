using HarmonyLib;

namespace VFECore.Abilities
{
    using RimWorld;
    using RimWorld.Planet;
    using Verse;

    public class AbilityPawnFlyer : PawnFlyer
    {
        private static readonly AccessTools.FieldRef<PawnFlyer, IntVec3> DestCellField
            = AccessTools.FieldRefAccess<IntVec3>(typeof(PawnFlyer), "destCell");

        public Abilities.Ability ability;

        public ref IntVec3 DestinationCell => ref DestCellField(this);

        protected override void RespawnPawn()
        {
            Pawn pawn = this.FlyingPawn;
            base.RespawnPawn();
            if (pawn != null && this.ability != null)
            {
                this.ability.ApplyHediffs(new GlobalTargetInfo(pawn));

                int? staggerTicks = this.ability.def.GetModExtension<AbilityExtension_Hediff>()?.hediff.CompProps<HediffCompProperties_Disappears>()?.disappearsAfterTicks.RandomInRange;
                if (staggerTicks.HasValue)
                {
                    pawn.stances.SetStance(new Stance_Cooldown(staggerTicks.Value + 1, this.ability.CasterPawn, null));
                    pawn.stances.stagger.StaggerFor(staggerTicks.Value);
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref this.ability, nameof(this.ability));
        }
    }
}