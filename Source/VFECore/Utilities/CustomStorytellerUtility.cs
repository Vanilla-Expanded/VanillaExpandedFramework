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
            return false;
        }

    }

}
