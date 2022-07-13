namespace VFECore.Abilities
{
    using System.Collections.Generic;
    using Verse;

    public class PawnKindAbilityExtension : DefModExtension
    {
        public List<AbilityDef> giveAbilities = new List<AbilityDef>();

        public HediffDef implantDef;
        public int       initialLevel = 1;
        public bool      giveRandomAbilities;
    }
}