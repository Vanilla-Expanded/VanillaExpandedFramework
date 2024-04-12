namespace VFECore.Abilities
{
    using HarmonyLib;
    using RimWorld;
    using RimWorld.Planet;
    using UnityEngine;
    using Verse;

    public class AbilityPawnFlyer : PawnFlyer
    {
        private static readonly AccessTools.FieldRef<PawnFlyer, IntVec3> DestCellField
            = AccessTools.FieldRefAccess<IntVec3>(typeof(PawnFlyer), "destCell");
        private static readonly AccessTools.FieldRef<PawnFlyer, Vector3> EffectivePosField
            = AccessTools.FieldRefAccess<Vector3>(typeof(PawnFlyer), "effectivePos");
        private static readonly AccessTools.FieldRef<PawnFlyer, Vector3> GroundPosField
            = AccessTools.FieldRefAccess<Vector3>(typeof(PawnFlyer), "groundPos");
        private static readonly AccessTools.FieldRef<PawnFlyer, float> EffectiveHeightField
            = AccessTools.FieldRefAccess<float>(typeof(PawnFlyer), "effectiveHeight");

        public Ability ability;
        public bool selectOnSpawn = false;

        public ref IntVec3 DestinationCell => ref DestCellField(this);

        /// <summary>
        /// Used as the position of the flying pawn and the pawn/thing they are carrying.
        /// </summary>
        public ref Vector3 EffectivePos => ref EffectivePosField(this);
        /// <summary>
        /// Used as the position of the flying pawn's shadow.
        /// </summary>
        public ref Vector3 GroundPos => ref GroundPosField(this);
        /// <summary>
        /// Used as the size of the flying pawn's shadow (clamped to 0-1, bigger value means smaller shadow).
        /// </summary>
        public ref float EffectiveHeight => ref EffectiveHeightField(this);

        /// <summary>
        /// Used to replace vanilla position calculation.
        /// Use <see cref="EffectivePos"/>, <see cref="GroundPos"/>, and <see cref="EffectiveHeight"/> to achieve this.
        /// </summary>
        /// <returns>true if the vanilla RecomputePosition method should be cancelled, or false if it should run normally.</returns>
        protected internal virtual bool CustomRecomputePosition() => false;

        /// <summary>
        /// Used to check if the pawn should be re-selected when spawning a new flyer.
        /// The flyer is not yet initialized at this point, with only its constructor being called.
        /// </summary>
        /// <param name="target">The pawn for which this flyer is being created for.</param>
        /// <returns>True if the pawn should be automatically reselected, false otherwise.</returns>
        protected internal virtual bool AutoSelectPawn(Pawn target) => true;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            if (selectOnSpawn)
            {
                selectOnSpawn = false;
                // Select the flying thing (pawn), with the flyer itself as fallback
                Find.Selector.Select(FlyingThing ?? this);
            }
        }

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