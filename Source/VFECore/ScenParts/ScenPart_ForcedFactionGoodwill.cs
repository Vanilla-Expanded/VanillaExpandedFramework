using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VFECore
{

    public class ScenPart_ForcedFactionGoodwill : ScenPart
    {

        private const string ForcedFactionGoodwillTag = "VFE_ForcedFactionGoodwill";
        private const float OptionRectPartWidth = 0.45f;
        private const float GoodwillRangeSliderWidth = 0.65f;

        private FactionDef factionDef;
        public bool alwaysHostile;
        private bool affectHiddenFactions;
        public bool affectStartingGoodwill;
        public IntRange startingGoodwillRange = IntRange.zero;
        public bool affectNaturalGoodwill;
        public IntRange naturalGoodwillRange = IntRange.zero;

        private IEnumerable<FactionDef> EligibleFactionDefs => DefDatabase<FactionDef>.AllDefsListForReading.Where(f => !f.isPlayer && (affectHiddenFactions || !f.hidden));

        public bool AffectsFaction(FactionDef faction)
        {
            return !faction.isPlayer && (factionDef == null || faction == factionDef) && (affectHiddenFactions || !faction.hidden);
        }

        public bool AffectsFaction(Faction faction)
        {
            return AffectsFaction(faction.def);
        }

        private string LabelText
        {
            get
            {
                // Name of faction
                if (factionDef != null)
                    return factionDef.LabelCap;

                // All factions including hidden
                else if (affectHiddenFactions)
                    return "VanillaFactionsExpanded.AllFactionsIncludingHidden".Translate();

                // All factions (except hidden)
                return "VanillaFactionsExpanded.AllFactions".Translate();
            }
        }

        public override void DoEditInterface(Listing_ScenEdit listing)
        {
            var scenPartRect = listing.GetScenPartRect(this, RowHeight * 4);

            // Faction selection
            var selectionRect = new Rect(scenPartRect.x, scenPartRect.y, scenPartRect.width, scenPartRect.height / 4);
            if (Widgets.ButtonText(selectionRect, LabelText))
            {
                var floatMenuOptions = new List<FloatMenuOption>();
                floatMenuOptions.Add(new FloatMenuOption("VanillaFactionsExpanded.AllFactions".Translate(), () => factionDef = null));
                var eligibleFactions = EligibleFactionDefs.ToList();
                for (int i = 0; i < eligibleFactions.Count; i++)
                {
                    var faction = eligibleFactions[i];
                    floatMenuOptions.Add(new FloatMenuOption(faction.LabelCap, () => factionDef = faction));
                }
                Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
            }

            // Faction options
            var optionsRect = new Rect(scenPartRect.x, scenPartRect.y + (scenPartRect.height / 4), scenPartRect.width, scenPartRect.height / 4);
            Widgets.CheckboxLabeled(optionsRect.LeftPart(OptionRectPartWidth), "VanillaFactionsExpanded.AlwaysHostile".Translate(), ref alwaysHostile);
            Widgets.CheckboxLabeled(optionsRect.RightPart(OptionRectPartWidth), "VanillaFactionsExpanded.AffectHiddenFactions".Translate(), ref affectHiddenFactions);

            // No point showing these if the faction is set to always be hostile
            if (!alwaysHostile)
            {
                // Starting goodwill range
                var startingRangeRect = new Rect(scenPartRect.x, scenPartRect.y + (scenPartRect.height * 2 / 4), scenPartRect.width, scenPartRect.height / 4);
                var startingRangeLeftPart = startingRangeRect.LeftPart(1 - GoodwillRangeSliderWidth);
                Widgets.CheckboxLabeled(startingRangeLeftPart, "VanillaFactionsExpanded.StartingGoodwill".Translate().Truncate(startingRangeLeftPart.width), ref affectStartingGoodwill);
                if (affectStartingGoodwill)
                    Widgets.IntRange(startingRangeRect.RightPart(GoodwillRangeSliderWidth), 76823, ref startingGoodwillRange, DiplomacyTuning.MinGoodwill, DiplomacyTuning.MaxGoodwill);

                // Natural goodwill range
                var naturalRangeRect = new Rect(scenPartRect.x, scenPartRect.y + (scenPartRect.height * 3 / 4), scenPartRect.width, scenPartRect.height / 4);
                var naturalRangeLeftPart = naturalRangeRect.LeftPart(1 - GoodwillRangeSliderWidth);
                Widgets.CheckboxLabeled(naturalRangeLeftPart, "VanillaFactionsExpanded.NaturalGoodwill".Translate().Truncate(naturalRangeLeftPart.width), ref affectNaturalGoodwill);
                if (affectNaturalGoodwill)
                    Widgets.IntRange(naturalRangeRect.RightPart(GoodwillRangeSliderWidth), -238948923, ref naturalGoodwillRange, DiplomacyTuning.MinGoodwill, DiplomacyTuning.MaxGoodwill);
            }
        }

        public override void Randomize()
        {
            affectHiddenFactions = Rand.Bool;
            alwaysHostile = Rand.Bool;
            factionDef = Rand.Bool ? null : EligibleFactionDefs.RandomElement();

            // Starting goodwill
            affectStartingGoodwill = Rand.Bool;
            int minStartingGoodwill = Rand.RangeInclusive(DiplomacyTuning.MinGoodwill, DiplomacyTuning.MaxGoodwill);
            int maxStartingGoodwill = Rand.RangeInclusive(minStartingGoodwill, DiplomacyTuning.MaxGoodwill);
            startingGoodwillRange = new IntRange(minStartingGoodwill, maxStartingGoodwill);

            // Natural goodwill
            affectNaturalGoodwill = Rand.Bool;
            int minNaturalGoodwill = Rand.RangeInclusive(DiplomacyTuning.MinGoodwill, DiplomacyTuning.MaxGoodwill);
            int maxNaturalGoodwill = Rand.RangeInclusive(minNaturalGoodwill, DiplomacyTuning.MaxGoodwill);
            naturalGoodwillRange = new IntRange(minNaturalGoodwill, maxNaturalGoodwill);
        }

        public override string Summary(Scenario scen)
        {
            return ScenSummaryList.SummaryWithList(scen, ForcedFactionGoodwillTag, "VanillaFactionsExpanded.ScenPart_ForcedFactionRelations".Translate());
        }

        private string GoodwillModifierString(string translationKey, IntRange range)
        {
            string entrySection = $"{translationKey.Translate()}: ";

            // If range's min and max are the same, just add a single number
            if (range.min == range.max)
                entrySection += range.min.ToString();

            // Otherwise add the range
            else
                entrySection += "VanillaFactionsExpanded.NumberRange".Translate(range.min, range.max);

            return entrySection;
        }

        public override IEnumerable<string> GetSummaryListEntries(string tag)
        {
            if (tag == ForcedFactionGoodwillTag)
            {
                // Always hostile
                if (alwaysHostile)
                    yield return $"{LabelText} ({"VanillaFactionsExpanded.AlwaysHostile".Translate().RawText.UncapitalizeFirst()})";

                // Affects goodwill in some way
                else if (affectStartingGoodwill || affectNaturalGoodwill)
                {
                    var goodwillModifierStrings = new List<string>();

                    // Starting goodwill
                    if (affectStartingGoodwill)
                        goodwillModifierStrings.Add(GoodwillModifierString("VanillaFactionsExpanded.StartingGoodwill", startingGoodwillRange));

                    // Natural goodwill
                    if (affectNaturalGoodwill)
                        goodwillModifierStrings.Add(GoodwillModifierString("VanillaFactionsExpanded.NaturalGoodwill", naturalGoodwillRange));

                    yield return $"{LabelText} ({goodwillModifierStrings.ToCommaList()})";
                }
            }
        }

        public override void PostWorldGenerate()
        {
            // Modify starting goodwills of NPC factions
            if (affectStartingGoodwill)
            {
                var factionList = Find.FactionManager.AllFactions.ToList();
                for (int i = 0; i < factionList.Count; i++)
                {
                    var faction = factionList[i];
                    if (!faction.IsPlayer && AffectsFaction(faction))
                        faction.TryAffectGoodwillWith(Faction.OfPlayer, startingGoodwillRange.RandomInRange - faction.PlayerGoodwill, false, false);
                }
            }
        }

        public override void ExposeData()
        {
            Scribe_Defs.Look(ref factionDef, "factionDef");
            Scribe_Values.Look(ref alwaysHostile, "alwaysHostile");
            Scribe_Values.Look(ref affectHiddenFactions, "affectHiddenFactions");
            Scribe_Values.Look(ref affectStartingGoodwill, "affectStartingGoodwill");
            Scribe_Values.Look(ref startingGoodwillRange, "startingGoodwillRange", IntRange.zero);
            Scribe_Values.Look(ref affectNaturalGoodwill, "affectNaturalGoodwill");
            Scribe_Values.Look(ref naturalGoodwillRange, "naturalGoodwillRange", IntRange.zero);

            base.ExposeData();
        }

    }

}
