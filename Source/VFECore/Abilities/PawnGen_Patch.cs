using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFECore.Abilities
{
    using HarmonyLib;
    using RimWorld;
    using Verse;

    [HarmonyPatch(typeof(PawnGenerator), "GenerateNewPawnInternal")]
    public class PawnGen_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn __result)
        {
            PawnKindAbilityExtension abilityExtension = __result.kindDef?.GetModExtension<PawnKindAbilityExtension>();
            if (abilityExtension == null)
                return;

            if (abilityExtension.implantDef != null)
            {
                Hediff_Abilities implant = __result.health?.hediffSet?.GetFirstHediffOfDef(abilityExtension.implantDef) as Hediff_Abilities ??
                                           HediffMaker.MakeHediff(abilityExtension.implantDef, __result,
                                                                  __result.RaceProps?.body?.GetPartsWithDef(BodyPartDefOf.Brain).FirstOrFallback()) as Hediff_Abilities;
                if(implant != null)
                {
                    implant.giveRandomAbilities = abilityExtension.giveRandomAbilities;
                    __result.health?.AddHediff(implant);
                    implant.SetLevelTo(abilityExtension.initialLevel);
                }
            }

            CompAbilities comp = __result.GetComp<CompAbilities>();

            foreach (AbilityDef abilityDef in abilityExtension.giveAbilities)
                comp.GiveAbility(abilityDef);
        }
    }
}
