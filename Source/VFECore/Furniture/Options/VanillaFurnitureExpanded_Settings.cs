using RimWorld;
using UnityEngine;
using Verse;


namespace VanillaFurnitureExpanded
{


    public class VanillaFurnitureExpanded_Settings : ModSettings

    {


        public static bool isRandomGraphic = true;
        public static bool hideRandomizeButton = false;




        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref isRandomGraphic, "isRandomGraphic", true, true);
            Scribe_Values.Look(ref hideRandomizeButton, "hideRandomizeButton", false, true);


        }

        public static void DoWindowContents(Rect inRect)
        {
            Listing_Standard ls = new Listing_Standard();


            ls.Begin(inRect);
          
            ls.CheckboxLabeled("VFE_RandomOrSequentially".Translate(), ref isRandomGraphic, null);
            ls.Gap(12f);
            ls.CheckboxLabeled("VFE_HideRandomizeButton".Translate(), ref hideRandomizeButton, null);
            ls.Gap(12f);

            ls.End();
        }



    }










}
