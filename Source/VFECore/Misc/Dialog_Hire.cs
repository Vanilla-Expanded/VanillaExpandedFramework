using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using VFECore.UItils;

namespace VFECore.Misc
{
    using Verse.AI;

    public class Dialog_Hire : Window
    {
        private readonly float                                      availableSilver;
        private readonly Hireable                                   hireable;
        private readonly Dictionary<PawnKindDef, Pair<int, string>> hireData;
        private readonly float                                      riskMultiplier;
        private readonly Map                                        targetMap;
        private          HireableFactionDef                         curFaction;
        private          int                                        daysAmount;
        private          string                                     daysAmountBuffer;

        public Dialog_Hire(Thing negotiator, Hireable hireable)
        {
            targetMap     = negotiator.Map;
            this.hireable = hireable;
            hireData      = hireable.SelectMany(def => def.pawnKinds).ToDictionary(def => def, _ => new Pair<int, string>(0, ""));
            closeOnCancel = true;
            forcePause    = true;
            closeOnAccept = true;
            availableSilver = targetMap.listerThings.ThingsOfDef(ThingDefOf.Silver)
                                    .Where(x => !x.Position.Fogged(x.Map) && (targetMap.areaManager.Home[x.Position] || x.IsInAnyStorage())).Sum(t => t.stackCount);
            riskMultiplier = Find.World.GetComponent<HiringContractTracker>().GetFactorForHireable(hireable);
        }

        public override    Vector2 InitialSize => new Vector2(750f, 650f);
        protected override float   Margin      => 15f;

        private float CostBase => Mathf.Pow(daysAmount, 0.8f) * hireData.Select(kv => new Pair<PawnKindDef, int>(kv.Key, kv.Value.First)).Where(pair => pair.Second > 0)
                                                                     .Sum(pair => Mathf.Pow(pair.Second, 1.2f) * pair.First.combatPower);

        private float CostFinal => CostBase * (riskMultiplier + 1f);

        public override void OnAcceptKeyPressed()
        {
            base.OnAcceptKeyPressed();
            SoundDefOf.ExecuteTrade.PlayOneShotOnCamera();

            if (this.daysAmount > 0 && this.hireData.Any(kvp => kvp.Value.First > 0))
            {
                List<Pawn> pawns = new List<Pawn>();

                TradeUtility.LaunchSilver(this.targetMap, Mathf.RoundToInt(CostFinal));


                if (!RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 cell, this.targetMap, 1f))
                    cell = CellFinder.RandomEdgeCell(this.targetMap);

                foreach (KeyValuePair<PawnKindDef, Pair<int, string>> kvp in this.hireData)
                    for (int i = 0; i < kvp.Value.First; i++)
                    {
                        Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kvp.Key, mustBeCapableOfViolence: true, faction: Faction.OfPlayer));
                        pawns.Add(pawn);
                        IntVec3 loc = CellFinder.RandomClosewalkCellNear(cell, this.targetMap, 8);
                        GenSpawn.Spawn(pawn, loc, this.targetMap, Rot4.Random);
                        pawn.jobs.TryTakeOrderedJob(job: new Job(JobDefOf.GotoWander, this.targetMap.areaManager.Home.ActiveCells.Where(iv => iv.Standable(this.targetMap)).RandomElement()));
                    }

                Find.World.GetComponent<HiringContractTracker>().SetNewContract(this.daysAmount, pawns, this.hireable);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            var rect   = new Rect(inRect);
            var anchor = Text.Anchor;
            var font   = Text.Font;
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font   = GameFont.Medium;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 40f), hireable.GetCallLabel());
            Text.Font =  GameFont.Small;
            rect.yMin += 40f;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 20f), "VEF.AvailableSilver".Translate(availableSilver.ToStringMoney()));
            rect.yMin += 30f;
            foreach (var def in hireable) DoHireableFaction(ref rect, def);
            var breakDownRect = rect.TakeTopPart(100f);
            breakDownRect.xMin += 115f;
            Text.Anchor        =  TextAnchor.UpperLeft;
            Text.Font          =  GameFont.Small;
            var infoRect = breakDownRect.TopPartPixels(20f);
            Widgets.Label(infoRect.LeftHalf(), "VEF.Breakdown".Translate());
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font   = GameFont.Tiny;
            Widgets.Label(infoRect.RightHalf(), "VEF.LongTerm".Translate().Colorize(ColoredText.SubtleGrayColor));
            Text.Font  =  GameFont.Small;
            infoRect.y += 20f;
            Widgets.DrawLightHighlight(infoRect);
            Widgets.Label(infoRect.LeftHalf(), "VEF.DayAmount".Translate());
            UIUtility.DrawCountAdjuster(ref daysAmount, infoRect.RightHalf(), ref daysAmountBuffer, 0, 60);
            infoRect.y += 20f;
            Widgets.DrawHighlight(infoRect);
            Widgets.Label(infoRect.LeftHalf(),  "VEF.Cost".Translate());
            Widgets.Label(infoRect.RightHalf(), CostBase.ToStringMoney());
            infoRect.y += 20f;
            Widgets.DrawLightHighlight(infoRect);
            Widgets.Label(infoRect.LeftHalf(),  "VEF.RiskMult".Translate());
            Widgets.Label(infoRect.RightHalf(), riskMultiplier.ToStringPercent());
            infoRect.y += 20f;
            Widgets.DrawHighlight(infoRect);
            Widgets.Label(infoRect.LeftHalf(),  "VEF.TotalPrice".Translate());
            Widgets.Label(infoRect.RightHalf(), CostFinal.ToStringMoney());
            if (Widgets.ButtonText(rect.TakeLeftPart(120f).BottomPartPixels(40f), "Cancel".Translate())) OnCancelKeyPressed();
            if (Widgets.ButtonText(rect.TakeRightPart(120f).BottomPartPixels(40f), "Confirm".Translate()))
            {
                if (CostFinal > availableSilver)
                    Messages.Message("NotEnoughSilver".Translate(), MessageTypeDefOf.RejectInput);
                else
                    OnAcceptKeyPressed();
            }

            Text.Font = GameFont.Tiny;
            Widgets.Label(rect.ContractedBy(30f, 0f), "VEF.HiringDesc".Translate(hireable.Key).Colorize(ColoredText.SubtleGrayColor));
            Text.Anchor = anchor;
            Text.Font   = font;
        }

        private void DoHireableFaction(ref Rect inRect, HireableFactionDef def)
        {
            var rect = inRect.TopPartPixels(Mathf.Max(20f + def.pawnKinds.Count * 30f, 120f));
            inRect.yMin += rect.height;
            var titleRect = rect.TakeTopPart(20f);
            var iconRect  = rect.LeftPartPixels(105f).ContractedBy(5f);
            titleRect.x += 115f;
            Text.Anchor =  TextAnchor.MiddleLeft;
            Text.Font   =  GameFont.Tiny;
            var nameRect = new Rect(titleRect);
            Widgets.Label(titleRect, "VEF.Hire".Translate(def.LabelCap));
            titleRect.x     += 200f;
            titleRect.width =  60f;
            Text.Anchor     =  TextAnchor.MiddleCenter;
            var valueRect = new Rect(titleRect);
            Widgets.Label(titleRect, "VEF.Value".Translate());
            titleRect.x     += 100f;
            titleRect.width =  300f;
            var numRect = new Rect(titleRect);
            Text.Font = GameFont.Tiny;
            Widgets.Label(titleRect, "VEF.ChooseNumberOfUnits".Translate().Colorize(ColoredText.SubtleGrayColor));
            Text.Font = GameFont.Small;
            Widgets.DrawLightHighlight(iconRect);
            Widgets.DrawTextureFitted(iconRect, def.Texture, 1f, new Vector2(def.Texture.width, def.Texture.height), new Rect(0f, 0f, 1f, 1f), 0, def.Material);
            var highlight = true;
            foreach (var kind in def.pawnKinds)
            {
                nameRect.y  += 20f;
                valueRect.y += 20f;
                numRect.y   += 20f;
                var fullRect = new Rect(nameRect.x - 4f, nameRect.y, nameRect.width + valueRect.width + numRect.width, 20f);
                if (highlight) Widgets.DrawHighlight(fullRect);
                highlight   = !highlight;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(nameRect, kind.LabelCap);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(valueRect, kind.combatPower.ToStringByStyle(ToStringStyle.Integer));
                var data   = hireData[kind];
                var amount = data.First;
                var buffer = data.Second;
                UIUtility.DrawCountAdjuster(ref amount, numRect, ref buffer, 0, 999, curFaction != null && curFaction != def);
                if (amount != data.First || buffer != data.Second)
                {
                    hireData[kind] = new Pair<int, string>(amount, buffer);
                    if (amount > 0  && curFaction == null) curFaction                                                    = def;
                    if (amount == 0 && curFaction == def && def.pawnKinds.All(pk => hireData[pk].First == 0)) curFaction = null;
                }
            }
        }
    }
}