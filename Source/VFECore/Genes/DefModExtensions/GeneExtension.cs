using RimWorld;
using System.Collections.Generic;
using Verse;
using UnityEngine;

namespace VanillaGenesExpanded
{
    using System.Xml;
    using JetBrains.Annotations;

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
        public Gender? forGenderOnly;

        //Custom blood thingDef, custom blood icon, custom flood splash effecter, custom wounds from a fleshtype
        public ThingDef customBloodThingDef = null;
        public string customBloodIcon = "";
        public EffecterDef customBloodEffect = null;
        public FleshTypeDef customWoundsFromFleshtype = null;
        //Custom vomit thingDef, custom vomit effecter
        public ThingDef customVomitThingDef = null;    
        public EffecterDef customVomitEffect = null;

        //Disease progression factor. Diseases will advance (when not immunized) by this factor
        public float diseaseProgressionFactor = 1f;

        //Caravan carrying factor. Pawns will have their caravan carrying capacity multiplied by this factor
        public float caravanCarryingFactor = 1f;

        //Hide gene from appearing on the xenotype creation screen. Useful for special genes that only appear as rewards during gameplay.
        public bool hideGene = false;

        //Makes this gene cause no skill loss for a given skill.
        public SkillDef noSkillLoss = null;
        //Makes this gene give the pawn recreation when gaining XP on a given skill.
        public SkillDef skillRecreation = null;

        //Makes genes scale body and head
        public Vector2 bodyScaleFactor = new Vector2(1f, 1f);
        public Vector2 headScaleFactor = new Vector2(1f, 1f);

        public BodyTypeDef forcedBodyType;
        public string bodyNakedGraphicPath;
        public string bodyDessicatedGraphicPath;
        public string headDessicatedGraphicPath;
        public string skullGraphicPath;


        // specific rotations
        public GeneOffsets offsets = new();

        public class GeneOffsets
        {
            public Vector3 GetOffset(Pawn pawn, Rot4 rotation)
            {
                Vector3 vector3 = this.GetOffset(rotation)?.GetOffset(pawn.story?.bodyType) ?? Vector3.zero;

                if (rotation == Rot4.East)
                    vector3.x = -vector3.x;
                else if (rotation == Rot4.North && this.layerInvert)
                    vector3.y = -vector3.y;

                return vector3;
            }

            public RotationOffset GetOffset(Rot4 rotation) =>
                rotation == Rot4.South ? this.south :
                rotation == Rot4.North ? this.north :
                rotation == Rot4.East  ? this.east ?? this.west : 
                                         this.west;

            public RotationOffset south = new();
            public RotationOffset north = new();
            public RotationOffset east  = new();
            public RotationOffset west;

            public bool layerInvert = true;
        }

        public class RotationOffset
        {
            public Vector3 GetOffset(BodyTypeDef bodyType)
            {
                Vector3 bodyOffset = this.bodyTypes?.FirstOrDefault(predicate: to => to.bodyType == bodyType)?.offset ?? Vector3.zero;
                return new Vector3(bodyOffset.x, bodyOffset.y, bodyOffset.z);
            }

            public List<BodyTypeOffset> bodyTypes;
        }

        public class BodyTypeOffset
        {
            public BodyTypeDef bodyType;
            public Vector3     offset = Vector3.zero;

            [UsedImplicitly]
            public void LoadDataFromXmlCustom(XmlNode xmlRoot)
            {
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, nameof(this.bodyType), xmlRoot.Name);
                this.offset = (Vector3)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(Vector3));
            }
        }
    }
}