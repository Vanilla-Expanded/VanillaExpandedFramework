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

    public List<Replacements> replacedDefs;
    // Add stuff to the list of things that were removed so they won't be loaded.
    // Doesn't apply to some defs and some situations - for example, removing ResearchProjectDef will throw a different error.
    // Before adding something here - make sure it actually has a positive effect.
    public List<Removals> removedDefs;

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

    public override void PostLoad()
    {
        base.PostLoad();

        if (!replacedDefs.NullOrEmpty())
        {
            foreach (var replacements in replacedDefs)
                replacements.replacements?.RemoveAll(x => !IsDefNameValid(x.original) || !IsDefNameValid(x.replacement));

            replacedDefs.RemoveAll(x => x.replacements.NullOrEmpty() || !typeof(Def).IsAssignableFrom(x.type));
        }
        if (!removedDefs.NullOrEmpty())
        {
            foreach (var removals in removedDefs)
                removals.removals.RemoveAll(x => !IsDefNameValid(x));

            removedDefs.RemoveAll(x => x.removals.NullOrEmpty() || !typeof(Def).IsAssignableFrom(x.type));
        }
    }

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (var configError in base.ConfigErrors())
            yield return configError;

        if (!replacedDefs.NullOrEmpty())
        {
            // Make sure all types are subtypes of Def
            if (replacedDefs.Any(x => !typeof(Def).IsAssignableFrom(x.type)))
                yield return $"all elements in {nameof(replacedDefs)} must use def types";

            // Make sure none of the defNames are null, use the default name, or use the "null" text (invalid names)
            if (replacedDefs.SelectMany(x => x.replacements).Any(x => x.original is null or DefaultDefName or "null" || x.replacement is null or DefaultDefName or "null"))
                yield return $"a defName in {nameof(replacedDefs)} is not specified, is '{DefaultDefName}', or is 'null'";
            // Make sure all defs only use allowed characters in their defNames
            else if (replacedDefs.SelectMany(x => x.replacements).Any(x => !AllowedDefNamesRegex.IsMatch(x.original) || !AllowedDefNamesRegex.IsMatch(x.replacement)))
                yield return $"all defNames in {nameof(replacedDefs)} should only contain letters, numbers, underscores, or dashes";
        }

        if (!removedDefs.NullOrEmpty())
        {
            // Make sure all types are subtypes of Def
            if (removedDefs.Any(x => !typeof(Def).IsAssignableFrom(x.type)))
                yield return $"all elements in {nameof(removedDefs)} must use def types";

            // Make sure none of the defNames are null, use the default name, or use the "null" text (invalid names)
            if (removedDefs.SelectMany(x => x.removals).Any(x => x is null or DefaultDefName or "null"))
                yield return $"a defName in {nameof(removedDefs)} is not specified, is '{DefaultDefName}', or is 'null'";
            // Make sure all defs only use allowed characters in their defNames
            else if (removedDefs.SelectMany(x => x.removals).Any(x => !AllowedDefNamesRegex.IsMatch(x)))
                yield return $"all defNames in {nameof(removedDefs)} should only contain letters, numbers, underscores, or dashes";
        }
    }

    private static bool IsDefNameValid(string defName) => defName is not (null or DefaultDefName or "null") && AllowedDefNamesRegex.IsMatch(defName);

    public class Replacements
    {
        public Type type;
        public List<(string original, string replacement)> replacements = [];

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            // Load the name of the node as the type
            type = ParseHelper.ParseType(xmlRoot.Name);
            // Load the first value of each node
            foreach (XmlNode node in xmlRoot.ChildNodes)
                replacements.Add((node["original"]?.InnerText, node["replacement"]?.InnerText));
        }
    }

    public class Removals
    {
        public Type type;
        public List<string> removals = [];

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            // Load the name of the node as the type
            type = ParseHelper.ParseType(xmlRoot.Name);
            // Load the first value of each node (presumably "li")
            foreach (XmlNode node in xmlRoot.ChildNodes)
                removals.Add(ParseHelper.ParseString(node.InnerText));
        }
    }
}