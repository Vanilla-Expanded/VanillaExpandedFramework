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

            List<Verb_CastAbility> verbs = compAbilities.LearnedAbilities.Where(ab => ab.CanAutoCast && ab.IsEnabledForPawn(out string _) && ab.CanHitTarget(target)).Select(ab => ab.verb).ToList();

            if (verbs.NullOrEmpty())
                return;

            if (!verbs.Select(ve => new Tuple<Verb, float>(ve, ve.ability.Chance)).AddItem(new Tuple<Verb, float>(__result, 1f)).TryRandomElementByWeight(t => t.Item2, out Tuple<Verb, float> result))
                return;

            __result = result.Item1;
        }
    }
}
