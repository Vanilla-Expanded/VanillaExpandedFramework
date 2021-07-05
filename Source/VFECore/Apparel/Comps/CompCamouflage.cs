using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VanillaApparelExpanded
{
    public class CompCamouflage : ThingComp
    {




        public CompProperties_Camouflage Props
        {
            get
            {
                return (CompProperties_Camouflage)this.props;
            }
        }

        public override void Notify_Equipped(Pawn pawn)
        {

            ApparelCollectionClass.AddCamouflagedPawnToList(pawn);

        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            ApparelCollectionClass.RemoveCamouflagedPawnFromList(pawn);
        }

       



    }
}

