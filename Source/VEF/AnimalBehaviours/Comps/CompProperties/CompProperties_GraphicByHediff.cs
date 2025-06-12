using Verse;
using UnityEngine;

namespace AnimalBehaviours
{
    public class CompProperties_GraphicByHediff : CompProperties
    {
        public CompProperties_GraphicByHediff()
        {
            this.compClass = typeof(CompGraphicByHediff);
        }

        public HediffDef hediffDef;
        [NoTranslate]
        public string newImagePath;
        [NoTranslate]
        public string maskPath = null;
        public bool changeDesiccatedGraphic = false;
        public string dessicatedTxt;
        public int changeGraphicsInterval = 2000;

        //If true graphicData uses CutoutComplex instead of Cutout

        public bool useCutoutComplex = false;
    }
}
