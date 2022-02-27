using System.Collections.Generic;
using HarmonyLib;
using MVCF.HarmonyPatches;
using Verse;
using Verse.Grammar;

namespace MVCF.Features.PatchSets
{
    public class PatchSet_BatteLog : PatchSet
    {
        public static void FixFakeCaster(ref Thing initiator)
        {
            if (initiator is IFakeCaster fc) initiator = fc.RealCaster();
        }

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
            yield return Patch.Prefix(AccessTools.Constructor(typeof(BattleLogEntry_RangedImpact), new[]
            {
                typeof(Thing), typeof(Thing),
                typeof(Thing), typeof(ThingDef), typeof(ThingDef), typeof(ThingDef)
            }), AccessTools.Method(GetType(), nameof(FixFakeCaster)));
            yield return Patch.Postfix(AccessTools.Method(typeof(PlayLogEntryUtility), "RulesForOptionalWeapon"), AccessTools.Method(GetType(),
                nameof(PlayLogEntryUtility_RulesForOptionalWeapon_Postfix)));
        }
    }
}