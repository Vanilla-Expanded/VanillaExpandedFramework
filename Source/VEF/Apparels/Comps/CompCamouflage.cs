using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VEF.Apparels
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

            StaticCollectionsClass.AddCamouflagedPawnToList(pawn);

        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            StaticCollectionsClass.RemoveCamouflagedPawnFromList(pawn);
        }

       



    }
}

