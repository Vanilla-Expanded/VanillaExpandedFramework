using RimWorld;
using UnityEngine;
using Verse;


namespace VEF.Buildings
{



    public class Furniture_Mod : Mod
    {


        public Furniture_Mod(ModContentPack content) : base(content)
        {
            var settings = GetSettings<Furniture_Settings>();
            BackwardsCompatibilityFixer.FixSettingsNameOrNamespace(this, settings, "VanillaFurnitureExpanded", "VanillaFurnitureExpanded_Settings");
        }
        public override string SettingsCategory()
        {
            return "";

        }



        public override void DoSettingsWindowContents(Rect inRect)
        {
            Furniture_Settings.DoWindowContents(inRect);
        }
    }


}
