
using RimWorld;
using Verse;

namespace VanillaGenesExpanded
{

    public class Gene_DiseaseProgressionFactor : Gene
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
                if (extension?.diseaseProgressionFactor != null)
                {
                    StaticCollectionsClass.AddDiseaseProgressionFactorGenePawnToList(pawn, extension.diseaseProgressionFactor);
                }
                
            }
        }

        public void RemoveThings()
        {
            if (pawn != null)
            {
                GeneExtension extension = this.def.GetModExtension<GeneExtension>();
                if (extension?.diseaseProgressionFactor != null)
                {
                    StaticCollectionsClass.RemoveDiseaseProgressionFactorGenePawnFromList(pawn);

                }
               
            }
        }
    }
}
