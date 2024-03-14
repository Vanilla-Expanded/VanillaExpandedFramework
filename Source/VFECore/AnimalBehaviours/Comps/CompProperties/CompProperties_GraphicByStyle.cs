
using Verse;
using System.Collections.Generic;

namespace AnimalBehaviours
{
    public class CompProperties_GraphicByStyle : CompProperties
    {

        public CompProperties_GraphicByStyle()
        {
            this.compClass = typeof(CompGraphicByStyle);
        }

        public List<StyleGraphics> styleGraphics;
        public int changeGraphicsInterval = 2000;

    }

    public class StyleGraphics
    {
        public StyleCategoryDef style;
        public string styleImageSuffix;
     
    }
}