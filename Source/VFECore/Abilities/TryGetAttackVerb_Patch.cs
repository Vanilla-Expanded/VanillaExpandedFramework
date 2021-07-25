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
        [HarmonyPrefix]
        public static bool Prefix(Pawn __instance, ref Verb __result, Thing target)
        {
            CompAbilities compAbilities = __instance.TryGetComp<CompAbilities>();
            if (compAbilities == null)
                return true;

            List<Verb_CastAbility> verbs = compAbilities.LearnedAbilities.Where(ab => ab.CanAutoCast && ab.IsEnabledForPawn(out string _) && ab.CanHitTarget(target)).Select(ab => ab.verb).ToList();

            if (verbs.NullOrEmpty())
                return true;

            if (!verbs.TryRandomElementByWeight(ve => ve.ability.Chance, out Verb_CastAbility verb))
                return true;

            __result = verb;
            return false;
        }
    }
}
