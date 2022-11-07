
using RimWorld;
using Verse;

namespace VanillaGenesExpanded
{

    public class Gene_AddsBloodType : Gene
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
                if (extension?.customBloodThingDef != null)
                {
                    StaticCollectionsClass.AddBloodtypeGenePawnToList(pawn, extension.customBloodThingDef);
                }
                if (extension?.customBloodIcon != null)
                {
                    StaticCollectionsClass.AddBloodIconGenePawnToList(pawn, extension.customBloodIcon);
                }
                if (extension?.customBloodEffect != null)
                {
                    StaticCollectionsClass.AddBloodEffectGenePawnToList(pawn, extension.customBloodEffect);
                }
            }
        }

        public void RemoveThings()
        {
            if (pawn != null)
            {
                GeneExtension extension = this.def.GetModExtension<GeneExtension>();
                if (extension?.customBloodThingDef != null)
                {
                    StaticCollectionsClass.RemoveBloodtypeGenePawnFromList(pawn);

                }
                if (extension?.customBloodIcon != null)
                {
                    StaticCollectionsClass.RemoveBloodIconGenePawnFromList(pawn);

                }
                if (extension?.customBloodEffect != null)
                {
                    StaticCollectionsClass.RemoveBloodEffectGenePawnFromList(pawn);

                }
            }
        }
    }
}
