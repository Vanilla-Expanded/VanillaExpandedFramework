
using RimWorld;
using Verse;

namespace VEF.AnimalBehaviours
{
    class CompDraftable : ThingComp
    {
        public CompProperties_Draftable Props
        {
            get
            {
                return (CompProperties_Draftable)this.props;
            }
        }

        public override void CompTickInterval(int delta)
        {
            if (parent.IsHashIntervalTick(Props.checkingInterval, delta))
            {
                Pawn pawn = this.parent as Pawn;
                if (Props.conditionalOnTrainability && (!ModsConfig.OdysseyActive || pawn.training?.HasLearned(InternalDefOf.VEF_Beastmastery) != true))
                {
                    StaticCollectionsClass.RemoveDraftableAnimalFromList(this.parent);
                    if (Props.makeNonFleeingToo)
                    {
                        StaticCollectionsClass.RemoveNotFleeingAnimalFromList(this.parent);
                    }
                }
                else
                {
                   
                    if (pawn.drafter == null) { pawn.drafter = new Pawn_DraftController(pawn); }
                    if (pawn.equipment == null) { pawn.equipment = new Pawn_EquipmentTracker(pawn); }
                    if (pawn.workSettings == null) { pawn.workSettings = new Pawn_WorkSettings(pawn); pawn.workSettings.EnableAndInitialize(); }
                    StaticCollectionsClass.AddDraftableAnimalToList(this.parent);
                    if (Props.makeNonFleeingToo)
                    {
                        StaticCollectionsClass.AddNotFleeingAnimalToList(this.parent);
                    }
                }
            }
        }

       

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            Pawn pawn = this.parent as Pawn;
            if(!Props.conditionalOnTrainability || (ModsConfig.OdysseyActive && pawn.training?.HasLearned(InternalDefOf.VEF_Beastmastery) == true))
            {
                StaticCollectionsClass.AddDraftableAnimalToList(this.parent);
                if (Props.makeNonFleeingToo)
                {
                    StaticCollectionsClass.AddNotFleeingAnimalToList(this.parent);
                }
            }
            

        }

        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            StaticCollectionsClass.RemoveDraftableAnimalFromList(this.parent);
            if (Props.makeNonFleeingToo)
            {
                StaticCollectionsClass.RemoveNotFleeingAnimalFromList(this.parent);
            }
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            StaticCollectionsClass.RemoveDraftableAnimalFromList(this.parent);
            if (Props.makeNonFleeingToo)
            {
                StaticCollectionsClass.RemoveNotFleeingAnimalFromList(this.parent);
            }
        }

       
    }
}
