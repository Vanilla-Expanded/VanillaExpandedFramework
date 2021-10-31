namespace VFECore.Abilities
{
    using System.Collections.Generic;
    using System.Linq;
    using Verse;

    public class Hediff_Abilities : Hediff_Level
    {
        public bool giveRandomAbilities = true;

        public override bool ShouldRemove => false;

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            this.GiveRandomAbilityAtLevel();
        }

        public override void ChangeLevel(int levelOffset)
        {
            int prevLevel = this.level;
            base.ChangeLevel(levelOffset);
            if (prevLevel != this.level && levelOffset > 0)
                for(; prevLevel < this.level; )
                    this.GiveRandomAbilityAtLevel(++prevLevel);
        }

        public virtual void GiveRandomAbilityAtLevel(int? forLevel = null)
        {
            if (!this.giveRandomAbilities) 
                return;

            forLevel = forLevel ?? this.level;

            CompAbilities comp = this.pawn.GetComp<CompAbilities>();
            List<AbilityDef> abilityDefs        = DefDatabase<AbilityDef>.AllDefsListForReading.Where(def => !comp.HasAbility(def) && def.requiredHediff != null && def.requiredHediff.hediffDef == this.def && def.requiredHediff.minimumLevel <= forLevel && (def.requiredTrait == null || this.pawn.story.traits.HasTrait(def.requiredTrait))).ToList();
            IEnumerable<AbilityDef> abilityDefsAtLevel = abilityDefs.Where(def => def.requiredHediff.minimumLevel == forLevel);

            if (!abilityDefsAtLevel.TryRandomElement(out AbilityDef abilityDef))
                abilityDef = abilityDefs.RandomElement();

            comp.GiveAbility(abilityDef);
        }

        public virtual IEnumerable<Gizmo> DrawGizmos()
        {
            yield break;
        }

        public virtual bool SatisfiesConditionForAbility(AbilityDef abilityDef) => 
            this.level >= abilityDef.requiredHediff.minimumLevel;
    }
}
