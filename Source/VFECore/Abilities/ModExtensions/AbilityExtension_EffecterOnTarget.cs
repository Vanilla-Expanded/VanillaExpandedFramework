namespace VFECore.Abilities
{
    using RimWorld;
    using System.Collections.Generic;
    using UnityEngine;
    using Verse;
    using Verse.Sound;
    public class AbilityExtension_EffecterOnTarget : AbilityExtension_AbilityMod
    {
        public bool onCaster;
		public EffecterDef effecterDef;
		public int maintainForTicks = -1;
		public float scale = 1f;
        public override void Cast(LocalTargetInfo target, Ability ability)
        {
            base.Cast(target, ability);
            Effecter effecter = null;
            IntVec3 cell;
            if (onCaster)
            {
                cell = ability.pawn.Position;
                effecter = effecterDef.Spawn(cell, ability.pawn.Map, scale);
            }
            else
            {
                cell = target.Cell;
                effecter = ((!target.HasThing) ? effecterDef.Spawn(cell, ability.pawn.Map, scale) : effecterDef.Spawn(target.Thing, ability.pawn.Map, scale));
            }

            if (maintainForTicks > 0)
            {
                ability.AddEffecterToMaintain(effecter, cell, maintainForTicks);
            }
            else
            {
                effecter.Cleanup();
            }
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if (maintainForTicks > 0 && !this.abilityDef.needsTicking)
            {
                yield return this.abilityDef.defName + " has AbilityExtension_EffecterOnTarget mod extension with maintainForTicks set to " 
                    + maintainForTicks + " but doesn't have needsTicking set to true. It will not work without ticking.";
            }
        }
    }
}