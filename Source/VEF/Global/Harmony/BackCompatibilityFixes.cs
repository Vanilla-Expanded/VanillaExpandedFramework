using System;
using System.Reflection;
using System.Xml;
using HarmonyLib;
using Verse;

namespace VEF;

[HarmonyPatch(typeof(BackCompatibility), nameof(BackCompatibility.BackCompatibleDefName))]
[HarmonyPatchCategory(VEF_HarmonyCategories.LateHarmonyPatchCategory)]
public static class VanillaExpandedFramework_BackCompatibility_BackCompatibleDefName_Patch
{
    internal static readonly Func<bool> CheckSaveIdenticalToCurrentEnvironmentMethod =
        NonPublicMethods.MakeDelegate<Func<bool>>(typeof(BackCompatibility).DeclaredMethod("CheckSaveIdenticalToCurrentEnvironment"));

    private static bool Prepare(MethodBase method)
    {
        if (method != null)
            return true;
        return BackwardsCompatibilityMigrationUtility.converter != null && !BackwardsCompatibilityMigrationUtility.defNameConverters.NullOrEmpty();
    }

    private static void Postfix(Type defType, string defName, bool forDefInjections, XmlNode node, ref string __result)
    {
        try
        {
            // Environment changed, don't replace stuff.
            if (!CheckSaveIdenticalToCurrentEnvironmentMethod())
                return;

            // Environment identical, so back compat fixer won't run. We need to manually trigger it to migrate defNames.
            var newDefName = BackwardsCompatibilityMigrationUtility.converter.BackCompatibleDefName(defType, defName, forDefInjections, node);
            if (newDefName != null)
                __result = newDefName;
        }
        catch (Exception e)
        {
            Log.Error($"[VEF] Error running defName migration on {defName}, exception:\n{e}");
        }
    }
}

[HarmonyPatch(typeof(BackCompatibility), nameof(BackCompatibility.GetBackCompatibleType))]
[HarmonyPatchCategory(VEF_HarmonyCategories.LateHarmonyPatchCategory)]
public static class VanillaExpandedFramework_BackCompatibility_GetBackCompatibleType_Patch
{
    private static bool Prepare(MethodBase method)
    {
        if (method != null)
            return true;
        return BackwardsCompatibilityMigrationUtility.converter != null && !BackwardsCompatibilityMigrationUtility.abilityClasses.NullOrEmpty();
    }

    private static bool Prefix(Type baseType, string providedClassName, XmlNode node, ref Type __result)
    {
        try
        {
            // Environment changed, don't replace stuff.
            if (!VanillaExpandedFramework_BackCompatibility_BackCompatibleDefName_Patch.CheckSaveIdenticalToCurrentEnvironmentMethod())
                return true;

            // Environment identical, so back compat fixer won't run. We need to manually trigger it to migrate types.
            var newType = BackwardsCompatibilityMigrationUtility.converter.GetBackCompatibleType(baseType, providedClassName, node);
            if (newType != null)
            {
                __result = newType;
                return false;
            }
        }
        catch (Exception e)
        {
            Log.Error($"[VEF] Error running Ability class migration with provided class name {providedClassName}, exception:\n{e}");
        }

        return true;
    }
}