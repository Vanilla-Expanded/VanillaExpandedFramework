using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFECore.Abilities
{
    using HarmonyLib;
    using Verse;

    [HarmonyBefore("legodude17.mvcf")]
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.TryGetAttackVerb))]
    public static class TryGetAttackVerb_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn __instance, ref Verb __result, Thing target)
        {
            CompAbilities compAbilities = __instance.TryGetComp<CompAbilities>();
            if (compAbilities == null)
                return;

            List<Verb_CastAbility> verbs = compAbilities.LearnedAbilities.Where(ab => ab.AutoCast && ab.IsEnabledForPawn(out string _) && (target == null || ab.CanHitTarget(target)))
                                                     .Select(ab => ab.verb).ToList();
            if (verbs.NullOrEmpty())
                return;

            if (target != null)
            {
                if (verbs.Where(x => x.ability.AICanUseOn(target))
                    .Select(ve => new Tuple<Verb, float>(ve, ve.ability.Chance)).AddItem(new Tuple<Verb, float>(__result, 1f))
                    .TryRandomElementByWeight(t => t.Item2, out Tuple<Verb, float> result))
                {
                    __result = result.Item1;
                }
            }
            else
            {
                Verb verb = verbs.AddItem(__result).MaxBy(ve => ve.verbProps.range);
                __result = verb;
            }
        }
    }
}
