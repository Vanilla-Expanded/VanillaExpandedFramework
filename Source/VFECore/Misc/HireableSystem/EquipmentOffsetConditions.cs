using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VFECore.Misc
{
    public class EquipmentOffsetConditions : DefModExtension
    {
        public List<TechLevel> techLevels;

        public bool IsValid(Thing weapon, ThingDef apparelDef)
        {
            var weaponType = weapon.def.Verbs?.Any(v =>
                v.verbClass == typeof(Verb_Shoot) || v.verbClass.IsSubclassOf(typeof(Verb_Shoot))) ?? false;
            var techLevel = techLevels?.Contains(weapon.def.techLevel) ?? true;
            return weaponType && techLevel;
        }
    }
}