using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using HarmonyLib;
using VEF.Abilities;
using Verse;

namespace VEF;

[StaticConstructorOnStartup]
internal class BackwardsCompatibilityMigrationUtility
{
    internal static BackCompatabilityConverter_VEF converter;

    internal static Dictionary<string, Type> abilityClasses = new();
    // We can either match defName first, or a type first.
    // Since types are less unique (there's a ton of ThingDefs), it should result in less dictionary lookups
    // if we check the defName first. If we, for example, match for ThingDef called "ModName_MyGun", there's
    // unlikely to be many defs named "ModName_MyGun" in the game. This means that we fail on the first check.
    // If we do it the other way around, we'll get a hit for every ThingDef, requiring a  defName check, meaning a second check.
    internal static Dictionary<string, Dictionary<Type, string>> defNameConverters = new();

    static BackwardsCompatibilityMigrationUtility()
    {
        var conversionChain = (List<BackCompatibilityConverter>)typeof(BackCompatibility).Field("conversionChain").GetValue(null);
        var removedDefs = (List<Tuple<string, Type>>)typeof(BackCompatibility).Field("RemovedDefs").GetValue(null);

        converter = new BackCompatabilityConverter_VEF();
        conversionChain.Add(converter);

        foreach (var def in DefDatabase<AbilityDef>.AllDefs)
        {
            foreach (var extension in def.modExtensions.OfType<AbilityExtension_ClassMigration>())
            {
                abilityClasses.Add(extension.oldClass, def.abilityClass);
            }
        }

        foreach (var def in DefDatabase<DefMigrationDef>.AllDefs.OrderByDescending(x => x.priority))
        {
            if (!def.removedDefs.NullOrEmpty())
            {
                removedDefs.AddRangeUnique(def.removedDefs.SelectMany(
                    removals => removals.removals,
                    (removals, removedDefName) => new Tuple<string, Type>(removedDefName, removals.type)));
            }

            if (!def.replacedDefs.NullOrEmpty())
            {
                foreach (var replacements in def.replacedDefs)
                {
                    foreach (var defNameReplacements in replacements.replacements)
                    {
                        if(!defNameConverters.TryGetValue(defNameReplacements.original, out var typeToDef))
                            defNameConverters[defNameReplacements.original] = typeToDef = [];
                        typeToDef.AddDistinct(replacements.type, defNameReplacements.replacement);
                    }
                }
            }
        }
    }

    public class BackCompatabilityConverter_VEF : BackCompatibilityConverter
    {
        public override bool AppliesToVersion(int majorVer, int minorVer) => true;

        public override string BackCompatibleDefName(Type defType, string defName, bool forDefInjections = false, XmlNode node = null)
        {
            // Make sure we have any converters before doing anything
            if (defNameConverters.Count <= 0)
                return null;
            // Check if we have any converters for a given defName
            if (!defNameConverters.TryGetValue(defName, out var types))
                return null;
            // If the defName has converters, make sure that we also have a type match
            if (!types.TryGetValue(defType, out var newDefName))
                return null;

            return newDefName;
        }

        public override Type GetBackCompatibleType(Type baseType, string providedClassName, XmlNode node)
        {
            if (baseType == typeof(Ability) && abilityClasses.TryGetValue(providedClassName, out var newType)) return newType;
            return null;
        }

        public override void PostExposeData(object obj)
        {
        }
    }
}