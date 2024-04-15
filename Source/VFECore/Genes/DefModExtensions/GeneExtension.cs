using RimWorld;
using System.Collections.Generic;
using Verse;
using UnityEngine;

namespace VanillaGenesExpanded
{
    using System.Linq;
    using System.Xml;
    using JetBrains.Annotations;

    public class GeneExtension : DefModExtension
    {
        public bool renderCacheOff = false;

        //Custom gene backgrounds
        public string backgroundPathEndogenes;
        public string backgroundPathXenogenes;
        public string backgroundPathArchite;

        //Applies hediffs to existing body parts when the gene is acquired
        public List<HediffToBodyparts> hediffsToBodyParts;
        //Applies a hediff to the whole body when the gene is acquired
        public HediffDef hediffToWholeBody;

        //Makes "fur" associated with this gene use the body's colour, instead of the hair
        public bool useSkinColorForFur = false;
        //Makes "fur" associated with this gene use the body's and hair's colours
        public bool useSkinAndHairColorsForFur = false;
        //Keeps "fur" untinted
        public bool dontColourFur = false;
        //Switches "fur" shader to be CutoutComplex rather than skin shader.
        public bool useMaskForFur = false;
        //Fur hides the body graphic underneath completely
        public bool furHidesBody = false;

        //Reference to a thingsetmaker that makes pawns with this gene start play with some things (only on game start)
        public ThingSetMakerDef thingSetMaker = null;

        //Gender bender
        public bool forceMale = false;
        public bool forceFemale = false;

        // Gene will be active for a given gender only
        public Gender forGenderOnly;

        //Custom blood thingDef, custom blood icon, custom flood splash effecter, custom wounds from a fleshtype
        public ThingDef customBloodThingDef = null;
        public string customBloodIcon = "";
        public EffecterDef customBloodEffect = null;
        public FleshTypeDef customWoundsFromFleshtype = null;
        public ThingDef customBloodSmearThingDef = null;

        //Custom vomit thingDef, custom vomit effecter
        public ThingDef customVomitThingDef = null;
        public EffecterDef customVomitEffect = null;
        //Custom meat thingDef when butchered. Looks like meat is back on the menu, boys!
        public ThingDef customMeatThingDef = null;
        //Custom leather thingDef when butchered
        public ThingDef customLeatherThingDef = null;

        //Disease progression factor. Diseases will advance (when not immunized) by this factor
        public float diseaseProgressionFactor = 1f;

        //Hide gene from appearing on the xenotype creation screen. Useful for special genes that only appear as rewards during gameplay.
        public bool hideGene = false;

        //Makes this gene cause no skill loss for a given skill.
        public SkillDef noSkillLoss = null;
        //Makes this gene give the pawn recreation when gaining XP on a given skill.
        public SkillDef skillRecreation = null;
        //Makes this gene multiply skill loss of all skills by this factor
        public float globalSkillLossMultiplier = 1f;
        //Makes this gene make pawns lose skill when below 10
        public bool skillDegradation = false;

        //Makes pregancies advance faster or slower
        public float pregnancySpeedFactor = 1f;

        // Makes pawns with this gene have a higher chance of getting food-binge mental break.
        public float foodBingeMentalBreakSelectionChanceFactor = 1;

        public bool doubleNegativeFoodThought = false;

        // Size by age
        public SizeByAge sizeByAge = null;

        //Makes genes scale body per lifestages, only works for gene graphics currently
        public Dictionary<LifeStageDef, Vector2> bodyScaleFactorsPerLifestages;

        public BodyTypeDef forcedBodyType;
        public string bodyNakedGraphicPath;
        public string bodyDessicatedGraphicPath;
        public string headDessicatedGraphicPath;
        public string skullGraphicPath;

        public List<GeneDef> applySkinColorWithGenes;

        public float? bodyScaleFactor, headScaleFactor;

        public override IEnumerable<string> ConfigErrors()
        {
            if (bodyScaleFactor.HasValue)
            {
                yield return "GeneExtension.bodyScaleFactor is obsoleted and unused, use StatDef VEF_CosmeticBodySize_Multiplier instead.";
            }
            if (headScaleFactor.HasValue)
            {
                yield return "GeneExtension.headScaleFactor is obsoleted and unused, use StatDef VEF_CosmeticBodySize_Multiplier instead.";
            }
        }

        public class SizeByAge
        {
            // Size of the pawn at the bottom of the range.
            public float minOffset = 0;
            // Size of the pawn at the top of the range.
            public float maxOffset = 0;
            // The float range
            public FloatRange range = new(0, 0);

            public float GetSize(float? age)
            {
                if (age == null) return 0;
                return Mathf.Lerp(minOffset, maxOffset, range.InverseLerpThroughRange(age.Value));
            }
        }
    }

    public static class GeneExtensionMethods
    {
        public static List<GeneExtension> GetActiveGeneExtensions(this Pawn_GeneTracker geneTracker)
        {
            var gExtensions = geneTracker?.GenesListForReading?
                .Select(gene => gene.def.GetModExtension<GeneExtension>())
                .Where(extension => extension != null)
                .ToList();
            if (gExtensions == null) return new List<GeneExtension>();
            else return gExtensions;
        }
    }
}