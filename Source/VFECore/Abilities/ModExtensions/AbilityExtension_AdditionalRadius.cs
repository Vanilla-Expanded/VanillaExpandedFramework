namespace VFECore.Abilities
{
    using RimWorld;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Verse;

    public class AbilityExtension_AdditionalRadius : AbilityExtension_AbilityMod
    {
        public float radius = 0f;
        public List<StatModifier> radiusStatFactors = new List<StatModifier>();
        public float GetRadiusFor(Pawn pawn) =>
            this.radiusStatFactors.Aggregate(this.radius, (current, statFactor) => current * (pawn.GetStatValue(statFactor.stat) * statFactor.value));

        public override void GizmoUpdateOnMouseover(Ability ability)
        {
            base.GizmoUpdateOnMouseover(ability);
            var radius = GetRadiusFor(ability.pawn);
            GenDraw.DrawRadiusRing(ability.pawn.Position, radius);
        }
    }
}