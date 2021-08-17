using System.Collections.Generic;
using HarmonyLib;
using Verse;
using Verse.Grammar;

namespace MVCF.Harmony
{
    public class BatteLog
    {
        public static void DoPatches(HarmonyLib.Harmony harm)
        {
            harm.Patch(AccessTools.Constructor(typeof(BattleLogEntry_RangedImpact), new[]
            {
                typeof(Thing), typeof(Thing),
                typeof(Thing), typeof(ThingDef), typeof(ThingDef), typeof(ThingDef)
            }), new HarmonyMethod(typeof(BatteLog), nameof(FixFakeCaster)));
            harm.Patch(AccessTools.Method(typeof(PlayLogEntryUtility), "RulesForOptionalWeapon"),
                postfix: new HarmonyMethod(typeof(BatteLog), nameof(PlayLogEntryUtility_RulesForOptionalWeapon_Postfix)));
        }

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
    }
}