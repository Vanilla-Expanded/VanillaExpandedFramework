using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VFECore.Misc
{
    public class Dialog_Hire : Window
    {
        private readonly float availableSilver;
        private readonly Hireable hireable;
        private readonly Dictionary<PawnKindDef, Pair<int, string>> hireData;
        private readonly float riskMultiplier;
        private readonly Map targetMap;
        private HireableFactionDef curFaction;
        private int daysAmount;
        private string daysAmountBuffer;

        public Dialog_Hire(Thing negotiator, Hireable hireable)
        {
            targetMap = negotiator.Map;
            this.hireable = hireable;
            hireData = hireable.SelectMany(def => def.pawnKinds).ToDictionary(def => def, _ => new Pair<int, string>(0, ""));
            closeOnCancel = true;
            forcePause = true;
            closeOnAccept = true;
            availableSilver = targetMap.listerThings.ThingsOfDef(ThingDefOf.Silver)
                .Where(x => !x.Position.Fogged(x.Map) && (targetMap.areaManager.Home[x.Position] || x.IsInAnyStorage())).Sum(t => t.stackCount);
            riskMultiplier = 0.1f; // TODO: Get actual risk multiplier from tracker
        }

        public override Vector2 InitialSize => new Vector2(750f, 650f);
        protected override float Margin => 15f;

        private float CostBase => Mathf.Pow(daysAmount, 0.8f) * hireData.Select(kv => new Pair<PawnKindDef, int>(kv.Key, kv.Value.First)).Where(pair => pair.Second > 0)
            .Sum(pair => Mathf.Pow(pair.Second, 1.2f) * pair.First.combatPower);

        private float CostFinal => CostBase * (riskMultiplier + 1f);

        public override void OnAcceptKeyPressed()
        {
            base.OnAcceptKeyPressed();
            SoundDefOf.ExecuteTrade.PlayOneShotOnCamera();
            // erdelf: This is where you use hireData and daysAmount to actually do the hiring of the mercenaries
        }

        public override void DoWindowContents(Rect inRect)
        {
            var rect = new Rect(inRect);
            var anchor = Text.Anchor;
            var font = Text.Font;
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 40f), hireable.GetCallLabel());
            Text.Font = GameFont.Small;
            rect.yMin += 40f;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 20f), "VEF.AvailableSilver".Translate(availableSilver.ToStringMoney()));
            rect.yMin += 30f;
            foreach (var def in hireable) DoHireableFaction(ref rect, def);
            var breakDownRect = rect.TopPartPixels(100f);
            breakDownRect.xMin += 115f;
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            var infoRect = breakDownRect.TopPartPixels(20f);
            Widgets.Label(infoRect.LeftHalf(), "VEF.Breakdown".Translate());
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Tiny;
            Widgets.Label(infoRect.RightHalf(), "VEF.LongTerm".Translate().Colorize(ColoredText.SubtleGrayColor));
            Text.Font = GameFont.Small;
            infoRect.y += 20f;
            Widgets.DrawLightHighlight(infoRect);
            Widgets.Label(infoRect.LeftHalf(), "VEF.DayAmount".Translate());
            CountAdjuster(ref daysAmount, infoRect.RightHalf(), ref daysAmountBuffer, 0, 60);
            infoRect.y += 20f;
            Widgets.DrawHighlight(infoRect);
            Widgets.Label(infoRect.LeftHalf(), "VEF.Cost".Translate());
            Widgets.Label(infoRect.RightHalf(), CostBase.ToStringMoney());
            infoRect.y += 20f;
            Widgets.DrawLightHighlight(infoRect);
            Widgets.Label(infoRect.LeftHalf(), "VEF.RiskMult".Translate());
            Widgets.Label(infoRect.RightHalf(), riskMultiplier.ToStringPercent());
            infoRect.y += 20f;
            Widgets.DrawHighlight(infoRect);
            Widgets.Label(infoRect.LeftHalf(), "VEF.TotalPrice".Translate());
            Widgets.Label(infoRect.RightHalf(), CostFinal.ToStringMoney());
            rect.yMin += 100f;
            if (Widgets.ButtonText(rect.LeftPartPixels(120f).BottomPartPixels(40f), "Cancel".Translate())) OnCancelKeyPressed();
            if (Widgets.ButtonText(rect.RightPartPixels(120f).BottomPartPixels(40f), "Confirm".Translate()))
            {
                if (CostFinal > availableSilver)
                    Messages.Message("NotEnoughSilver".Translate(), MessageTypeDefOf.RejectInput);
                else
                    OnAcceptKeyPressed();
            }

            rect.xMin += 150f;
            rect.xMax -= 150f;
            Text.Font = GameFont.Tiny;
            Widgets.Label(rect, "VEF.HiringDesc".Translate(hireable.Key).Colorize(ColoredText.SubtleGrayColor));
            Text.Anchor = anchor;
            Text.Font = font;
        }

        private void DoHireableFaction(ref Rect inRect, HireableFactionDef def)
        {
            var rect = inRect.TopPartPixels(Mathf.Max(20f + def.pawnKinds.Count * 30f, 120f));
            inRect.yMin += rect.height;
            var titleRect = rect.TopPartPixels(20f);
            var iconRect = rect.LeftPartPixels(105f).ContractedBy(5f);
            rect.y += 20f;
            titleRect.x += 115f;
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font = GameFont.Tiny;
            var nameRect = new Rect(titleRect);
            Widgets.Label(titleRect, "VEF.Hire".Translate(def.LabelCap));
            titleRect.x += 200f;
            titleRect.width = 30f;
            Text.Anchor = TextAnchor.MiddleCenter;
            var valueRect = new Rect(titleRect);
            Widgets.Label(titleRect, "VEF.Value".Translate());
            titleRect.x += 100f;
            titleRect.width = 300f;
            var numRect = new Rect(titleRect);
            Text.Font = GameFont.Tiny;
            Widgets.Label(titleRect, "VEF.ChooseNumberOfUnits".Translate().Colorize(ColoredText.SubtleGrayColor));
            Text.Font = GameFont.Small;
            Widgets.DrawLightHighlight(iconRect);
            Widgets.DrawTextureFitted(iconRect, def.Texture, 1f, new Vector2(def.Texture.width, def.Texture.height), new Rect(0f, 0f, 1f, 1f), 0, def.Material);
            var highlight = true;
            foreach (var kind in def.pawnKinds)
            {
                nameRect.y += 20f;
                valueRect.y += 20f;
                numRect.y += 20f;
                var fullRect = new Rect(nameRect.x - 4f, nameRect.y, nameRect.width + valueRect.width + numRect.width, 20f);
                if (highlight) Widgets.DrawHighlight(fullRect);
                highlight = !highlight;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(nameRect, kind.LabelCap);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(valueRect, kind.combatPower.ToStringByStyle(ToStringStyle.Integer));
                var data = hireData[kind];
                var amount = data.First;
                var buffer = data.Second;
                CountAdjuster(ref amount, numRect, ref buffer, 0, 999, curFaction != null && curFaction != def);
                if (amount != data.First || buffer != data.Second)
                {
                    hireData[kind] = new Pair<int, string>(amount, buffer);
                    if (amount > 0 && curFaction == null) curFaction = def;
                    if (amount == 0 && curFaction == def && def.pawnKinds.All(pk => hireData[pk].First == 0)) curFaction = null;
                }
            }
        }

        private static void CountAdjuster(ref int value, Rect inRect, ref string buffer, int min, int max, bool readOnly = false)
        {
            var temp = value;
            var rect = inRect.ContractedBy(50f, 0);
            var leftBigRect = rect.LeftPartPixels(30f);
            rect.xMin += 30f;
            var leftSmallRect = rect.LeftPartPixels(30f);
            rect.xMin += 30f;
            var rightBigRect = rect.RightPartPixels(30f);
            rect.xMax -= 30f;
            var rightSmallRect = rect.RightPartPixels(30f);
            rect.xMax -= 30f;
            var mult = GenUI.CurrentAdjustmentMultiplier();
            if (!readOnly && value != min && Widgets.ButtonText(leftBigRect, "<<")) value = min;
            if (!readOnly && value - mult >= min && Widgets.ButtonText(leftSmallRect, "<")) value -= mult;
            if (!readOnly && value != max && Widgets.ButtonText(rightBigRect, ">>")) value = max;
            if (!readOnly && value + mult <= max && Widgets.ButtonText(rightSmallRect, ">")) value += mult;
            if (value != temp || readOnly) buffer = value.ToString();
            Widgets.TextFieldNumeric(rect.ContractedBy(3f, 0f), ref temp, ref buffer, min, max);
            if (!readOnly) value = temp;
        }
    }
}