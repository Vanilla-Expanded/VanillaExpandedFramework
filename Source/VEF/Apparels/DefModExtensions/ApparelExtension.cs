using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Apparels
{
    public class ApparelExtension : DefModExtension
    {
        public float? skillGainModifier;
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
    }

    public class PawnCapacityMinLevel
    {
        public PawnCapacityDef capacity;
        public float minLevel;
    }
}