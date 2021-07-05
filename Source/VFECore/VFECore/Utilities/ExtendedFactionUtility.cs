using RimWorld;
using System.Linq;
using Verse;

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