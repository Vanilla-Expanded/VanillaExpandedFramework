
using RimWorld;
using Verse;

namespace VanillaGenesExpanded
{

    public class Gene_ModifyCaravanCarrying : Gene
    {
        public override void PostAdd()
        {
            base.PostAdd();
            AddThings();
        }

        public override void PostMake()
        {
            base.PostMake();
            AddThings();
        }

        public override void PostRemove()
        {
            base.PostRemove();
            RemoveThings();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            AddThings();
        }

        public void AddThings()
        {
            if (pawn != null)
            {
                GeneExtension extension = this.def.GetModExtension<GeneExtension>();
                if (extension?.caravanCarryingFactor != null)
                {
                    StaticCollectionsClass.AddCaravanCarryingFactorGenePawnToList(pawn, extension.caravanCarryingFactor);
                }

            }
        }

        public void RemoveThings()
        {
            if (pawn != null)
            {
                GeneExtension extension = this.def.GetModExtension<GeneExtension>();
                if (extension?.caravanCarryingFactor != null)
                {
                    StaticCollectionsClass.RemoveCaravanCarryingFactorGenePawnFromList(pawn);

                }

            }
        }
    }
}
