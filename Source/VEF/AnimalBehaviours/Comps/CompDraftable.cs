
using RimWorld;
using Verse;

namespace VEF.AnimalBehaviours
{
    class CompDraftable : ThingComp
    {
        public int tickCounter = 0;


        public CompProperties_Draftable Props
        {
            get
            {
                return (CompProperties_Draftable)this.props;
            }
        }

        public override void CompTick()
        {
            tickCounter--;
            if (tickCounter < 0)
            {
                Pawn pawn = this.parent as Pawn;
                if (Props.conditionalOnTrainability && pawn.training?.HasLearned(InternalDefOf.VEF_Beastmastery) != true)
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
                    StaticCollectionsClass.AddDraftableAnimalToList(this.parent);
                    if (Props.makeNonFleeingToo)
                    {
                        StaticCollectionsClass.AddNotFleeingAnimalToList(this.parent);
                    }
                }
                tickCounter = Props.checkingInterval;
            }
        }

       

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            Pawn pawn = this.parent as Pawn;
            if(!Props.conditionalOnTrainability || pawn.training?.HasLearned(InternalDefOf.VEF_Beastmastery) == true)
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
