using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace KCSG
{
    class KCSG_Settings : ModSettings
    {
        public bool enableLog = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.enableLog, "enableLog", true);
        }
    }

    class KCSG_Mod : Mod
    {
        public static KCSG_Settings settings;

        public KCSG_Mod(ModContentPack content) : base(content)
        {
            settings = GetSettings<KCSG_Settings>();
        }

        public override string SettingsCategory() => "Custom Structure Generation";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);
            listing_Standard.CheckboxLabeled("Enable structure generation debugging log: ",
                                                ref settings.enableLog);
            listing_Standard.End();
            settings.Write();
        }
    }
}
