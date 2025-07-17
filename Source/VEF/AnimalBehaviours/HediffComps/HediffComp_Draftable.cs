
using Verse;
using RimWorld;

namespace VEF.AnimalBehaviours
{
    public class HediffComp_Draftable : HediffComp
    {

        public int tickCounter = 0;

        public HediffCompProperties_Draftable Props
        {
            get
            {
                return (HediffCompProperties_Draftable)this.props;
            }
        }

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            if (Pawn.IsHashIntervalTick(Props.checkingInterval, delta))
            {
                if (this.parent.pawn.drafter == null) { this.parent.pawn.drafter = new Pawn_DraftController(this.parent.pawn); }
                if (this.parent.pawn.equipment == null) { this.parent.pawn.equipment = new Pawn_EquipmentTracker(this.parent.pawn); }
                StaticCollectionsClass.AddDraftableAnimalToList(this.parent.pawn);
                if (Props.makeNonFleeingToo)
                {
                    StaticCollectionsClass.AddNotFleeingAnimalToList(this.parent.pawn);
                }
                if (Props.canHandleWeapons)
                {
                    StaticCollectionsClass.AddCanEquipWeaponsAnimalToList(this.parent.pawn);
                }
            }
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            if (this.parent.pawn.drafter == null) { this.parent.pawn.drafter = new Pawn_DraftController(this.parent.pawn); }
            if (this.parent.pawn.equipment == null) { this.parent.pawn.equipment = new Pawn_EquipmentTracker(this.parent.pawn); }
            StaticCollectionsClass.AddDraftableAnimalToList(this.parent.pawn);
            if (Props.makeNonFleeingToo)
            {
                StaticCollectionsClass.AddNotFleeingAnimalToList(this.parent.pawn);
            }
            if (Props.canHandleWeapons)
            {
                StaticCollectionsClass.AddCanEquipWeaponsAnimalToList(this.parent.pawn);
            }
        }

        public override void CompPostPostRemoved()
        {
            StaticCollectionsClass.RemoveDraftableAnimalFromList(this.parent.pawn);
            if (Props.makeNonFleeingToo)
            {
                StaticCollectionsClass.RemoveNotFleeingAnimalFromList(this.parent.pawn);
            }
            if (Props.canHandleWeapons)
            {
                StaticCollectionsClass.RemoveCanEquipWeaponsAnimalFromList(this.parent.pawn);
            }
        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            StaticCollectionsClass.RemoveDraftableAnimalFromList(this.parent.pawn);
            if (Props.makeNonFleeingToo)
            {
                StaticCollectionsClass.RemoveNotFleeingAnimalFromList(this.parent.pawn);
            }
            if (Props.canHandleWeapons)
            {
                StaticCollectionsClass.RemoveCanEquipWeaponsAnimalFromList(this.parent.pawn);
            }
        }

        public override void Notify_PawnKilled()
        {
            StaticCollectionsClass.RemoveDraftableAnimalFromList(this.parent.pawn);
            if (Props.makeNonFleeingToo)
            {
                StaticCollectionsClass.RemoveNotFleeingAnimalFromList(this.parent.pawn);
            }
            if (Props.canHandleWeapons)
            {
                StaticCollectionsClass.RemoveCanEquipWeaponsAnimalFromList(this.parent.pawn);
            }
        }
    }
}
