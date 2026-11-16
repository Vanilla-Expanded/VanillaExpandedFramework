
using RimWorld;
using Verse;
namespace VEF.AnimalBehaviours
{
    public class CompProperties_AbilityConicalSpew : CompProperties_AbilityEffect
    {
        public float range;

        public float lineWidthEnd;

        public ThingDef filthDef;

        public int damAmount = -1;

        public EffecterDef effecterDef;

        public DamageDef damageDef;

        public float penetration = -1f;

        public float filthChance = 1f;

        public bool canHitFilledCells;

        public bool scaleByVerbDamage = false;
        public float verbDamageScale = 1f;
        public string verbLabelForScaling;

        public CompProperties_AbilityConicalSpew()
        {
            compClass = typeof(CompAbilityConicalSpew);
        }
    }
}