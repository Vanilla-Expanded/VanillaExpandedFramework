using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VEF.Apparels
{
    public class ApparelExtension : DefModExtension, IMergeable
    {
        public float skillGainModifier = 1f;
        public WorkTags workDisables = WorkTags.None;
        public List<SkillDef> skillDisables;

        public List<StatModifier> equippedStatFactors;
        public List<TraitDef> traitsOnEquip;
        public List<TraitDef> traitsOnUnequip;
        public List<PawnCapacityMinLevel> pawnCapacityMinLevels;
        public bool preventDowning;
        public bool preventKilling;
        public float preventKillingUntilHealthHPPercentage = 1f;
        public bool preventKillingUntilBrainMissing;
        public bool preventBleeding;

        public List<ThingDef> secondaryApparelGraphics;
        public bool isUnifiedApparel;
        public bool hideHead;
        public bool showBodyInBedAlways;

        // Order doesn't matter.
        public float Priority => 0f;

        // Always possible to merge.
        public bool CanMerge(object other) => other != null && other.GetType() == typeof(ApparelExtension);

        public void Merge(object extension)
        {
            var other = (ApparelExtension)extension;

            skillGainModifier *= other.skillGainModifier;
            workDisables |= other.workDisables;
            CombineLists(ref skillDisables, other.skillDisables);

            CombineStatModifiers(ref equippedStatFactors, other.equippedStatFactors);
            CombineLists(ref traitsOnEquip, other.traitsOnEquip);
            CombineLists(ref traitsOnUnequip, other.traitsOnUnequip);
            CombineStatCapacityMinLevels(ref pawnCapacityMinLevels, other.pawnCapacityMinLevels);
            preventDowning |= other.preventDowning;
            preventKilling |= other.preventKilling;
            preventKillingUntilHealthHPPercentage *= other.preventKillingUntilHealthHPPercentage;
            preventKillingUntilBrainMissing |= other.preventKillingUntilBrainMissing;
            preventBleeding |= other.preventBleeding;

            CombineLists(ref secondaryApparelGraphics, other.secondaryApparelGraphics);
            isUnifiedApparel |= other.isUnifiedApparel;
            hideHead |= other.hideHead;
            showBodyInBedAlways |= other.showBodyInBedAlways;
        }

        private static void CombineLists<T>(ref List<T> original, List<T> other)
        {
            if (original == null)
                original = other;
            else if (other != null)
                original.AddRangeUnique(other);
        }

        private static void CombineStatModifiers(ref List<StatModifier> original, List<StatModifier> other)
        {
            if (original == null)
            {
                original = other;
                return;
            }

            if (other == null)
                return;

            foreach (var otherFactor in other)
            {
                var factor = original.FirstOrDefault(f => f.stat == otherFactor.stat);
                // The original doesn't include the current one, add it
                if (factor == null)
                    original.Add(otherFactor);
                // The original includes the current one, merge them (multiply)
                else
                    factor.value *= otherFactor.value;
            }
        }

        private static void CombineStatCapacityMinLevels(ref List<PawnCapacityMinLevel> original, List<PawnCapacityMinLevel> other)
        {
            if (original == null)
            {
                original = other;
                return;
            }

            if (other == null)
                return;

            foreach (var otherMinLevel in other)
            {
                var minLevel = original.FirstOrDefault(f => f.capacity == otherMinLevel.capacity);
                // The original doesn't include the current one, add it
                if (minLevel == null)
                    original.Add(otherMinLevel);
                // The original includes the current one, merge them (multiply)
                else
                    minLevel.minLevel = Math.Max(minLevel.minLevel, otherMinLevel.minLevel);
            }
        }
    }

    public class PawnCapacityMinLevel
    {
        public PawnCapacityDef capacity;
        public float minLevel;
    }
}