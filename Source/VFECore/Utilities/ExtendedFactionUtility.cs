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

    public static class ExtendedFactionUtility
    {

        public static Faction RandomFactionOfTechLevel(TechLevel level, bool allowHidden = false, bool allowDefeated = false, bool allowNonHumanlike = false)
        {
            if (Find.FactionManager.AllFactions.Where(f => !f.def.isPlayer && f.def.techLevel == level && (allowHidden || !f.def.hidden) && (allowDefeated || !f.defeated) && (allowNonHumanlike || f.def.humanlikeFaction)).TryRandomElement(out var fac))
                return fac;
            return null;
        }

    }

}
