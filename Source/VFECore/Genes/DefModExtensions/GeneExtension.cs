using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VanillaGenesExpanded
{

    public class GeneExtension : DefModExtension
    {

        //Custom gene backgrounds
        public string backgroundPathEndogenes;
        public string backgroundPathXenogenes;

        //Applies hediffs to existing body parts when the gene is acquired
        public List<HediffToBodyparts> hediffsToBodyParts;
        //Applies a hediff to the whole body when the gene is acquired
        public HediffDef hediffToWholeBody;

        //Makes "fur" associated with this gene use the body's colour, instead of the hair
        public bool useSkinColorForFur = false;
        //Keeps "fur" untinted
        public bool dontColourFur = false;

        //Reference to a thingsetmaker that makes pawns with this gene start play with some things (only on game start)
        public ThingSetMakerDef thingSetMaker = null;

        //Gender bender
        public bool forceMale = false;
        public bool forceFemale = false;

        //Makes genes scale body and head
        public float bodyScaleFactor = 1f;
        public float headScaleFactor = 1f;

        //Disables the scaling for adult and child
        public bool disableAdultScaling = false;
        public bool disableChildScaling = false;

        //Allows offsets based on body
        public GeneBodyOffset bodyOffsetNorth;
        public GeneBodyOffset bodyOffsetSouth;
        public GeneBodyOffset bodyOffsetEast;

        public BodyTypeDef forcedBodyType;
        public string bodyNakedGraphicPath;
        public string bodyDessicatedGraphicPath;
        public string headDessicatedGraphicPath;
        public string skullGraphicPath;
    }

}
