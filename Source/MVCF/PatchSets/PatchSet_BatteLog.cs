using System.Collections.Generic;
using HarmonyLib;
using Verse;
using Verse.Grammar;

namespace MVCF.PatchSets;

public class PatchSet_BatteLog : PatchSet
{
    public static IEnumerable<Rule> PlayLogEntryUtility_RulesForOptionalWeapon_Postfix(IEnumerable<Rule> __result,
        string prefix, ThingDef weaponDef, ThingDef projectileDef)
    {
        foreach (var rule in __result) yield return rule;
        if (weaponDef != null || projectileDef == null) yield break;

        foreach (var rule in GrammarUtility.RulesForDef(prefix + "_projectile", projectileDef))
            yield return rule;
    }

    public override IEnumerable<Patch> GetPatches()
    {
        yield return Patch.Postfix(AccessTools.Method(typeof(PlayLogEntryUtility), "RulesForOptionalWeapon"), AccessTools.Method(GetType(),
            nameof(PlayLogEntryUtility_RulesForOptionalWeapon_Postfix)));
    }
}
