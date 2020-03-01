using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VFECore
{

    public class SiegeParameterSetDef : Def
    {

        public override void ResolveReferences()
        {
            var thingDefs = DefDatabase<ThingDef>.AllDefsListForReading;
            for (int i = 0; i < thingDefs.Count; i++)
            {
                var thingDef = thingDefs[i];
                if (thingDef.building != null && !thingDef.building.buildingTags.NullOrEmpty() && thingDef.building.buildingTags.Any(t => artilleryBuildingTags.Contains(t)))
                {
                    // Min blueprint points
                    var thingDefExtension = ThingDefExtension.Get(thingDef);
                    if (thingDefExtension.siegeBlueprintPoints < lowestArtilleryBlueprintPoints)
                        lowestArtilleryBlueprintPoints = thingDefExtension.siegeBlueprintPoints;

                    // Skill prerequisite
                    if (thingDef.constructionSkillPrerequisite > maxArtilleryConstructionSkill)
                        maxArtilleryConstructionSkill = thingDef.constructionSkillPrerequisite;
                }
            }
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if (coverDef != null && coverDef.size != IntVec2.One)
                yield return $"coverDef must be a 1x1 building. {coverDef}'s size is {coverDef.size.x}x{coverDef.size.z}";
        }

        public ThingDef coverDef;
        public List<string> artilleryBuildingTags;
        public IntRange artilleryCountRange;
        public ThingDef mealDef;

        [Unsaved]
        public List<ThingDef> artilleryDefs;
        [Unsaved]
        public float lowestArtilleryBlueprintPoints = Int32.MaxValue;
        [Unsaved]
        public int maxArtilleryConstructionSkill;

    }

}
