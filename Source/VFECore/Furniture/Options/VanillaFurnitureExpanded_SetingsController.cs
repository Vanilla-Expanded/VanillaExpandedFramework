using RimWorld;
using UnityEngine;
using Verse;


namespace VanillaFurnitureExpanded
{



    public class VanillaFurnitureExpanded_Mod : Mod
    {


        public VanillaFurnitureExpanded_Mod(ModContentPack content) : base(content)
        {
            GetSettings<VanillaFurnitureExpanded_Settings>();
        }
        public override string SettingsCategory()
        {
            if (ModLister.HasActiveModWithName("Vanilla Textures Expanded - Variations"))
            {
                return "Vanilla Textures Expanded - Variations";
            }
            else return "";


        }



        public override void DoSettingsWindowContents(Rect inRect)
        {
            VanillaFurnitureExpanded_Settings.DoWindowContents(inRect);
        }
    }


}
