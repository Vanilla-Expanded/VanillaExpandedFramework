using RimWorld;
using UnityEngine;
using Verse;
using VFECore.UItils;

namespace VFECore.Misc
{
    using System.Linq;

    public class Dialog_ContractInfo : Window
    {
        private readonly HiringContractTracker contract;
        private          Vector2               pawnsScrollPos = new Vector2(0, 0);

        public Dialog_ContractInfo(HiringContractTracker tracker)
        {
            contract   = tracker;
            forcePause = true;
        }

        public override    Vector2 InitialSize => new Vector2(750f, 650f);
        protected override float   Margin      => 15f;

        public override void DoWindowContents(Rect inRect)
        {
            var font      = Text.Font;
            var anchor    = Text.Anchor;
            var iconRect  = new Rect(inRect.x, inRect.y, 50f, 50f);
            var titleRect = inRect.TakeTopPart(50f);
            titleRect.xMin += 60f;
            Text.Anchor    =  TextAnchor.MiddleLeft;
            Text.Font      =  GameFont.Medium;
            Widgets.Label(titleRect, "VEF.ContractTitle".Translate((contract.factionDef?.label ?? contract.hireable.Key).CapitalizeFirst()));
            if (contract.factionDef != null)
            {
                Widgets.DrawLightHighlight(iconRect);
                GUI.color = contract.factionDef.Color;
                Widgets.DrawTextureFitted(iconRect, contract.factionDef.Texture, 1f);
                GUI.color = Color.white;
            }

            var pawnsRect = inRect.LeftHalf().ContractedBy(3f);
            var infoRect  = inRect.RightHalf().ContractedBy(3f);
            infoRect.yMin += 20f;
            Text.Font     =  GameFont.Small;
            Widgets.Label(pawnsRect.TakeTopPart(20f), "VEF.PawnsList".Translate());
            Widgets.DrawMenuSection(pawnsRect);
            pawnsRect = pawnsRect.ContractedBy(5f);
            var pawns = contract.pawns.Where(x => x is not null).ToList();
            var viewRect = new Rect(0, 0, pawnsRect.width - 20f, pawns.Count * 40f);
            Widgets.BeginScrollView(pawnsRect, ref pawnsScrollPos, viewRect);
            foreach (var pawn in pawns)
            {
                var pawnRect = viewRect.TakeTopPart(33f);
                if (pawn != pawns.Last()) Widgets.DrawLineHorizontal(pawnRect.x, pawnRect.yMax, pawnRect.width);
                Widgets.DrawHighlightIfMouseover(pawnRect);
                if (Widgets.ButtonInvisible(pawnRect))
                {
                    Close(false);
                    CameraJumper.TryJumpAndSelect(pawn);
                }

                Widgets.ThingIcon(new Rect(pawnRect.x + 3f, pawnRect.y + 3f, 27f, 27f), pawn, 1f, Rot4.South);
                pawnRect.xMin += 35f;
                Widgets.Label(pawnRect.LeftHalf(),  pawn.NameFullColored);
                Widgets.Label(pawnRect.RightHalf(), pawn.health.summaryHealth.SummaryHealthPercent.ToStringPercent());
            }

            Widgets.EndScrollView();

            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font   = GameFont.Small;
            var textRect = infoRect.TakeTopPart(30f);
            Widgets.Label(textRect.LeftHalf(),  "VEF.Spent".Translate());
            Widgets.Label(textRect.RightHalf(), contract.price.ToStringMoney().Colorize(ColoredText.CurrencyColor));
            Widgets.DrawLineHorizontal(textRect.x, textRect.y + 30f, textRect.width);
            textRect.y    += 30;
            infoRect.yMin += 30;
            Widgets.Label(textRect.LeftHalf(), "VEF.TimeLeft".Translate());
            int remainingTicks = (this.contract.endTicks - Find.TickManager.TicksAbs);
            Widgets.Label(textRect.RightHalf(), (remainingTicks < 0 ? 0 : remainingTicks).ToStringTicksToPeriodVerbose().Colorize(ColoredText.DateTimeColor));
            if (Widgets.ButtonText(infoRect.TakeBottomPart(40f), "VEF.CancelContract".Translate()))
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("VEF.NoRefund".Translate(), () =>
                                                                                                      {
                                                                                                          Close();
                                                                                                          this.contract.endTicks = Find.TickManager.TicksAbs;
                                                                                                      }, true, "VEF.CancelContract".Translate()));
            Text.Anchor = anchor;
            Text.Font   = font;
        }
    }
}