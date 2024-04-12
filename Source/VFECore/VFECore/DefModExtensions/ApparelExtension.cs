using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VFECore
{
	public class PawnCapacityMinLevel
	{
		public PawnCapacityDef capacity;
        public float minLevel;
	}
	public class ApparelExtension : DefModExtension
    {
        public List<StatModifier> equippedStatFactors;
        public List<TraitDef> traitsOnEquip;
        public List<TraitDef> traitsOnUnequip;
        public List<PawnCapacityMinLevel> pawnCapacityMinLevels;
        public bool preventDowning;
        public bool preventKilling;
        public float preventKillingUntilHealthHPPercentage = 1f;
        public bool preventKillingUntilBrainMissing;
        public bool preventBleeding;
    }
}
