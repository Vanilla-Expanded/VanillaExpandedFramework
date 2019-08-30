using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Harmony;

namespace VFECore
{

    public static class CustomStorytellerUtility
    {

        public static bool TechLevelAllowed(TechLevel level)
        {
            // Paranoid nullcheck
            var storyteller = Find.Storyteller;
            if (storyteller != null)
            {
                return StorytellerDefExtension.Get(storyteller.def).allowedTechLevels.Includes(level);
            }
            return true;
        }

        public static IEnumerable<ResearchProjectDef> AllowedResearchProjectDefs()
        {
            return DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where(r => TechLevelAllowed(r.techLevel));
        }

        public static bool TryGetRandomUnfinishedResearchProject(out ResearchProjectDef research)
        {
            return DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where(r => !r.IsFinished).TryRandomElementByWeight(r => Mathf.Pow(1f / ((int)r.techLevel + 1), 2), out research);
        }

    }

}
