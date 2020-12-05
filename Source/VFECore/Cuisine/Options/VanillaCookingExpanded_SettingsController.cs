using RimWorld;
using UnityEngine;
using Verse;


namespace VanillaCookingExpanded
{



    public class VanillaCookingExpanded_Mod : Mod
    {


        public VanillaCookingExpanded_Mod(ModContentPack content) : base(content)
        {
            GetSettings<VanillaCookingExpanded_Settings>();
        }
        public override string SettingsCategory() => "Vanilla Cooking Expanded";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            VanillaCookingExpanded_Settings.DoWindowContents(inRect);
        }
    }


}
