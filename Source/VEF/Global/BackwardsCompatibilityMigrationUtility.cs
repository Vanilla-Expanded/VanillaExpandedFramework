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

    internal static Dictionary<string, Type> abilityClasses = [];
    // We can either match defName first, or a type first.
    // Since types are less unique (there's a ton of ThingDefs), it should result in less dictionary lookups
    // if we check the defName first. If we, for example, match for ThingDef called "ModName_MyGun", there's
    // unlikely to be many defs named "ModName_MyGun" in the game. This means that we fail on the first check.
    // If we do it the other way around, we'll get a hit for every ThingDef, requiring a  defName check, meaning a second check.
    internal static Dictionary<string, Dictionary<Type, string>> defNameConverters = [];

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

        var vanillaRemovedDefs = removedDefs.Count;
        foreach (var def in DefDatabase<DefMigrationDef>.AllDefs.OrderByDescending(x => x.priority))
        {
            if (!def.migratedDefs.NullOrEmpty())
            {
                foreach (var migrationsByType in def.migratedDefs)
                {
                    foreach (var migration in migrationsByType.migrations)
                    {
                        // Check if we already have a migration ready. Don't do anything if we do, as the existing ones will have higher priority.
                        if (defNameConverters.TryGetValue(migration.original, out var typeToDef))
                        {
                            if (typeToDef.ContainsKey(migrationsByType.type))
                                continue;
                        }

                        if (migration.replacement == null)
                        {
                            // Don't add duplicates
                            if (!removedDefs.Any(x => x.Item1 == migration.original && x.Item2 == migrationsByType.type))
                                removedDefs.Add(new Tuple<string, Type>(migration.original, migrationsByType.type));
                        }
                        else
                        {
                            // We're guaranteed to have a replacement, so make sure we're not trying to also remove the def.
                            // Also, don't remove vanilla defs from the list.
                            var index = removedDefs.FindIndex(x => x.Item1 == migration.original && x.Item2 == migrationsByType.type);
                            if (index >= vanillaRemovedDefs)
                                removedDefs.RemoveAt(index);

                            // Initialize the dictionary if it's null
                            if(typeToDef == null)
                                defNameConverters[migration.original] = typeToDef = [];
                            // Guaranteed to not exists, no need to worry about distinct
                            typeToDef[migrationsByType.type] = migration.replacement;
                        }
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