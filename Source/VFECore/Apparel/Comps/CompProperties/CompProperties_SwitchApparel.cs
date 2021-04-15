using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace VanillaApparelExpanded
{
    public class CompProperties_SwitchApparel : CompProperties
    {
        public CompProperties_SwitchApparel() { this.compClass = typeof(CompSwitchApparel); }

        public ThingDef SwitchTo;

        public string Label;

        public string graphicPath = "UI/Gizmo/Switch";

    }
}
