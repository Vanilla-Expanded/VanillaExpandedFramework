using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using VFE.Mechanoids;
using VFE.Mechanoids.Needs;

namespace VFECore
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
    public class ThingWithFloorGraphic : ThingWithComps
    {
		public Graphic graphicIntOverride;
		public Graphic FloorGraphic(FloorGraphicExtension floorGraphicExtension)
		{
			if (graphicIntOverride == null)
			{
				if (floorGraphicExtension.graphicData == null)
				{
					return BaseContent.BadGraphic;
				}
				graphicIntOverride = floorGraphicExtension.graphicData.GraphicColoredFor(this);
			}
			return graphicIntOverride;
		}
		public override Graphic Graphic
		{
			get
			{
				if (this.ParentHolder is Map)
                {
					var extension = this.def.GetModExtension<FloorGraphicExtension>();
					if (extension != null)
					{
						return FloorGraphic(extension);
					}
				}
				return base.Graphic;
			}
		}
	}
}
