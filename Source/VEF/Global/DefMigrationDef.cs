using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using HarmonyLib;
using Verse;

namespace VEF;

public class DefMigrationDef : Def
{
    public float priority = 0;

    // Add stuff to the list of things that were removed so they won't be loaded.
    // Doesn't apply to some defs and some situations - for example, removing ResearchProjectDef will throw a different error.
    // Before adding something here - make sure it actually has a positive effect.
    public List<DefMigrationsByType> migratedDefs;

    private static readonly Regex AllowedDefNamesRegex;

    static DefMigrationDef()
    {
        var type = typeof(Def).DeclaredField("AllowedDefNamesRegex");
        if (type != null && type.IsStatic)
            AllowedDefNamesRegex = type.GetValue(null) as Regex;

        if (AllowedDefNamesRegex == null)
        {
            Log.Warning("[VEF] Failed getting value from Def:AllowedDefNamesRegex field. Using a fallback.");
            AllowedDefNamesRegex = new Regex("^[a-zA-Z0-9\\-_]*$");
        }
    }

    public override void ResolveReferences()
    {
        base.ResolveReferences();

        if (!migratedDefs.NullOrEmpty())
        {
            migratedDefs.RemoveAll(x => !typeof(Def).IsAssignableFrom(x.type));

            foreach (var migrations in migratedDefs)
            {
                migrations.migrations.RemoveAll(x =>
                {
                    if (!IsDefNameValid(x.original))
                        return true;
                    if (x.replacement == null)
                        return false;

                    var invalidEntry = false;
                    if (!IsDefNameValid(x.replacement))
                    {
                        if (x.replacement != null && !x.removeIfMissingReplacement)
                            invalidEntry = true;
                        x.replacement = null;
                    }
                    else
                    {
                        var validEntry = GenDefDatabase.GetAllDefsInDatabaseForDef(migrations.type).Any(def => def.defName == x.replacement);
                        if (!validEntry)
                        {
                            x.replacement = null;
                            if (!x.removeIfMissingReplacement)
                                invalidEntry = true;
                        }
                    }

                    return invalidEntry;
                });
            }

            migratedDefs.RemoveAll(x => x.migrations.NullOrEmpty());
        }
    }

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (var configError in base.ConfigErrors())
            yield return configError;

        if (!migratedDefs.NullOrEmpty())
        {
            // Make sure all types are subtypes of Def
            if (migratedDefs.Any(x => !typeof(Def).IsAssignableFrom(x.type)))
                yield return $"all elements in {nameof(migratedDefs)} must use def types";

            // Make sure none of the defNames are null, use the default name, or use the "null" text (invalid names)
            if (migratedDefs.SelectMany(x => x.migrations).Any(x => x.original is null or DefaultDefName or "null" || x.replacement is DefaultDefName or "null"))
                yield return $"a defName in {nameof(migratedDefs)} is not specified, is '{DefaultDefName}', or is 'null'";
            // Make sure all defs only use allowed characters in their defNames
            else if (migratedDefs.SelectMany(x => x.migrations).Any(x => !AllowedDefNamesRegex.IsMatch(x.original) || (x.replacement != null && !AllowedDefNamesRegex.IsMatch(x.replacement))))
                yield return $"all defNames in {nameof(migratedDefs)} should only contain letters, numbers, underscores, or dashes";
        }
    }

    private static bool IsDefNameValid(string defName) => defName is not (null or DefaultDefName or "null") && AllowedDefNamesRegex.IsMatch(defName);

    public class DefMigrationsByType
    {
        public Type type;
        public List<DefMigration> migrations = [];

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            // Load the name of the node as the type
            type = ParseHelper.ParseType(xmlRoot.Name);

            foreach (XmlNode node in xmlRoot.ChildNodes)
            {
                if (node is XmlComment)
                    continue;
                
                // Load each field directly
                var migration = new DefMigration
                {
                    original = node[nameof(DefMigration.original)]?.InnerText,
                    replacement = node[nameof(DefMigration.replacement)]?.InnerText,
                };

                var removeIfMissingNode = node[nameof(DefMigration.removeIfMissingReplacement)];
                if (removeIfMissingNode != null)
                    migration.removeIfMissingReplacement = ParseHelper.ParseBool(removeIfMissingNode.InnerText);

                // If original is still null (or empty) and node's name isn't "li", use node name as original
                if (migration.original.NullOrEmpty() && node.Name != "li")
                    migration.original = node.Name;

                migrations.Add(migration);
            }
        }
    }

    public class DefMigration
    {
        public string original;
        public string replacement;
        public bool removeIfMissingReplacement = false;
    }
}