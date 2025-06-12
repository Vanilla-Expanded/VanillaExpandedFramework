using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VEF.Graphics
{
    public class FloorGraphicExtension : DefModExtension
    {
        public GraphicData graphicData;
        public override IEnumerable<string> ConfigErrors()
        {
            graphicData.ResolveReferencesSpecial();
            return base.ConfigErrors();
        }
    }
  
}
