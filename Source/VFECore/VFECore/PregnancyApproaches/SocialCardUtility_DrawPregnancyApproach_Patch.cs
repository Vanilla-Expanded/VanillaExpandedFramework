using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace VFECore
{
    [HarmonyPatch(typeof(SocialCardUtility), "DrawPregnancyApproach")]
    public static class SocialCardUtility_DrawPregnancyApproach_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var get_WindowStackInfo = AccessTools.PropertyGetter(typeof(Find), "WindowStack");
            var drawTextureInfo = AccessTools.Method(typeof(GUI), "DrawTexture", new Type[] { typeof(Rect), typeof(Texture) });
            var tooltipHandlerTipRegionInfo = AccessTools.Method(typeof(TooltipHandler), "TipRegion", new Type[] { typeof(Rect), typeof(TipSignal) });
            var messageInfo = AccessTools.Method(typeof(Messages), "Message", new Type[] { typeof(string), typeof(LookTargets), typeof(MessageTypeDef), typeof(bool) });
            var socialCardUtilityType = typeof(SocialCardUtility);
            var type = socialCardUtilityType.GetNestedType("CachedSocialTabEntry", AccessTools.all);
            var otherPawnField = AccessTools.Field(type, "otherPawn");

            var codes = codeInstructions.ToList();
            for (var i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                if (code.Calls(drawTextureInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, otherPawnField);
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(SocialCardUtility_DrawPregnancyApproach_Patch), "InterceptTexture"));
                }
                if (code.Calls(tooltipHandlerTipRegionInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, otherPawnField);
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Ldloc_2);
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(SocialCardUtility_DrawPregnancyApproach_Patch), "InterceptTooltip"));
                }
                if (code.Calls(messageInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, otherPawnField);
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(SocialCardUtility_DrawPregnancyApproach_Patch), "InterceptMessage"));
                }
                else
                {
                    yield return new CodeInstruction(code);
                }
                if (code.Calls(get_WindowStackInfo) && codes[i + 1].opcode == OpCodes.Ldloc_3)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, otherPawnField);
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(SocialCardUtility_DrawPregnancyApproach_Patch), "AddPregnancyApproachOptions"));
                }
            }
        }

        public static void InterceptMessage(string message, LookTargets targets, MessageTypeDef messageTypeDef, bool historical, Pawn selPawnForSocialInfo, Pawn otherPawn)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            AddPregnancyApproachOptions(otherPawn, selPawnForSocialInfo, list);
            if (list.Any())
            {
                Find.WindowStack.Add(new FloatMenu(list));
            }
            else
            {
                Messages.Message(message, targets, messageTypeDef, historical: historical);
            }
        }
        public static TipSignal InterceptTooltip(TipSignal tipSignal, Pawn otherPawn, Pawn selPawnForSocialInfo, AcceptanceReport acceptanceReport)
        {
            var data = selPawnForSocialInfo.relations.GetAdditionalPregnancyApproachData();
            if (data.partners.TryGetValue(otherPawn, out var def))
            {
                return acceptanceReport ? ("PregnancyApproach".Translate().Colorize(ColoredText.TipSectionTitleColor) + "\n" +
                    def.label + "\n\n" + "ClickToChangePregnancyApproach".Translate().Colorize
                    (ColoredText.SubtleGrayColor)) : ("PregnancyNotPossible".Translate().Resolve() + ": "
                    + acceptanceReport.Reason.CapitalizeFirst());
            }
            return tipSignal;
        }

        public static Texture2D InterceptTexture(Texture2D texture, Pawn otherPawn, Pawn selPawnForSocialInfo)
        {
            if (DefDatabase<PregnancyApproachDef>.AllDefs.Any(def => PawnsSatisfyPregnancyApproachRequirements(def, otherPawn, selPawnForSocialInfo)))
            {
                GUI.color = Color.white;
            }
            var data = selPawnForSocialInfo.relations.GetAdditionalPregnancyApproachData();
            if (data.partners.TryGetValue(otherPawn, out var def))
            {
                return def.icon.Texture;
            }
            return texture;
        }

        public static void AddPregnancyApproachOptions(Pawn otherPawn, Pawn selPawnForSocialInfo, List<FloatMenuOption> list)
        {
            foreach (var def in DefDatabase<PregnancyApproachDef>.AllDefs)
            {
                if (PawnsSatisfyPregnancyApproachRequirements(def, otherPawn, selPawnForSocialInfo))
                {
                    list.Add(new FloatMenuOption(def.label, delegate
                    {
                        selPawnForSocialInfo.relations.GetAdditionalPregnancyApproachData().partners[otherPawn] = def;
                        otherPawn.relations.GetAdditionalPregnancyApproachData().partners[selPawnForSocialInfo] = def;
                    }, def.icon.Texture, Color.white));
                }
            }
            if (list.Any(x => x.Label == PregnancyApproach.Normal.GetDescription()) is false)
            {
                var data = selPawnForSocialInfo.relations.GetAdditionalPregnancyApproachData();
                if (data.partners.TryGetValue(otherPawn, out var pregnancyApporachDef))
                {
                    list.Add(new FloatMenuOption(pregnancyApporachDef.cancelLabel, delegate
                    {
                        selPawnForSocialInfo.relations.GetAdditionalPregnancyApproachData().partners.Remove(otherPawn);
                        otherPawn.relations.GetAdditionalPregnancyApproachData().partners.Remove(selPawnForSocialInfo);
                    }, pregnancyApporachDef.icon.Texture, Color.white));
                }
            }
        }

        public static bool PawnsSatisfyPregnancyApproachRequirements(PregnancyApproachDef def, Pawn otherPawn, Pawn selPawnForSocialInfo)
        {
            if (def.requireDifferentGender)
            {
                if (otherPawn.gender == selPawnForSocialInfo.gender)
                {
                    return false;
                }
            }
            if (def.requiredGene != null)
            {
                if (selPawnForSocialInfo.genes.HasGene(def.requiredGene) is false && otherPawn.genes.HasGene(def.requiredGene) is false)
                {
                    return false;
                }
            }
            if (def.requireFertility)
            {
                if (selPawnForSocialInfo.Sterile() || otherPawn.Sterile())
                {
                    return false;
                }
            }
            return true;
        }
    }
}
