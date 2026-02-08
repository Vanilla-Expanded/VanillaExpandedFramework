using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VEF.Apparels
{
    public class ApparelExtension : DefModExtension, IMergeable
    {
        public float priority = 0f;

        public float skillGainModifier = 1f;
        public WorkTags workDisables = WorkTags.None;
        public List<SkillDef> skillDisables;

        public List<StatModifier> equippedStatFactors;
        // Using TraitRequirement. Specifying traits without a degree is still possible and done
        // exactly the same as before, but it's now possible to include degree data as well.
        public List<TraitRequirement> traitsOnEquip;
        public List<TraitRequirement> traitsOnUnequip;
        public List<PawnCapacityMinLevel> pawnCapacityMinLevels;
        public bool preventDowning;
        public bool preventKilling;
        public float preventKillingUntilHealthHPPercentage = 1f;
        public bool preventKillingUntilBrainMissing;
        public bool preventBleeding;
        public bool destroyedOnDeath = false;

        // Apparel-only properties.
        public List<ThingDef> secondaryApparelGraphics;
        public bool isUnifiedApparel;
        public bool hideHead;
        public bool showBodyInBedAlways;

        // Order matters for traitsOnEquip and traitsOnUnequip - they
        // may have different degrees, so we need to pick 1.
        public float Priority => priority;

        // Always possible to merge.
        public bool CanMerge(object other) => other != null && other.GetType() == typeof(ApparelExtension);

        public void Merge(object extension)
        {
            var other = (ApparelExtension)extension;

            skillGainModifier *= other.skillGainModifier;
            workDisables |= other.workDisables;
            CombineLists(ref skillDisables, other.skillDisables);

            CombineStatModifiers(ref equippedStatFactors, other.equippedStatFactors);
            CombineTraits(ref traitsOnEquip, other.traitsOnEquip);
            CombineTraits(ref traitsOnUnequip, other.traitsOnUnequip);
            CombineStatCapacityMinLevels(ref pawnCapacityMinLevels, other.pawnCapacityMinLevels);
            preventDowning |= other.preventDowning;
            preventKilling |= other.preventKilling;
            preventKillingUntilHealthHPPercentage *= other.preventKillingUntilHealthHPPercentage;
            preventKillingUntilBrainMissing |= other.preventKillingUntilBrainMissing;
            preventBleeding |= other.preventBleeding;
            destroyedOnDeath |= other.destroyedOnDeath;

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

        public static void CombineTraits(ref List<TraitRequirement> original, List<TraitRequirement> other)
        {
            if (original == null)
            {
                original = other;
                return;
            }

            if (other == null)
                return;

            // May be different degrees, so we need to handle this like that.
            foreach (var otherTrait in other)
            {
                if (!original.Any(trait => trait.def == otherTrait.def))
                    original.Add(otherTrait);
            }
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

    public class CapacityImpactorGearMinLevel : PawnCapacityUtility.CapacityImpactor
    {
        public Thing gear;
        public ApparelExtension extension;
        public PawnCapacityDef capacity;

        public override bool IsDirect => false;

        public override string Readable(Pawn pawn)
        {
            var minLevel = extension.pawnCapacityMinLevels?.FirstOrDefault(x => x.capacity == capacity);
            if (minLevel == null)
                return gear.LabelCap;

            return $"{gear.LabelCap}: {"VEF.MinCapacityLevel".Translate((GenMath.RoundedHundredth(minLevel.minLevel) * 100f).Named("MIN"))}";
        }
    }
}