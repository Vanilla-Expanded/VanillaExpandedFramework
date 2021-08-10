namespace VFECore.Abilities
{
    using System;
    using UnityEngine;
    using Verse;

    [StaticConstructorOnStartup]
    public class Command_Ability : Command_Action
    {
        public static readonly Texture2D CooldownTex =
            SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.1f));

        public Pawn    pawn;
        public Ability ability;

        public Command_Ability(Pawn pawn, Ability ability) : base()
        {
            this.pawn    = pawn;
            this.ability = ability;

            this.defaultLabel   = ability.def.LabelCap;
            this.defaultDesc    = ability.GetDescriptionForPawn();
            this.icon           = ability.def.icon;
            this.disabled       = !ability.IsEnabledForPawn(out string reason);
            this.disabledReason = reason.Colorize(Color.red);
            this.action         = ability.DoAction;
            //this.shrinkable     = true;
        }

        public override void GizmoUpdateOnMouseover()
        {
            base.GizmoUpdateOnMouseover();

            float radius;
            switch (this.ability.def.targetMode)
            {
                case AbilityTargetingMode.Self:
                    radius = this.ability.GetRadiusForPawn();
                    break;
                case AbilityTargetingMode.Location:
                case AbilityTargetingMode.Thing:
                case AbilityTargetingMode.Pawn:
                    radius = this.ability.GetRangeForPawn();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (GenRadial.MaxRadialPatternRadius > radius && radius >= 1)
                GenDraw.DrawRadiusRing(this.pawn.Position, radius, Color.cyan);
        }

        public override bool GroupsWith(Gizmo other) => false;// other is Command_Ability;

        protected override GizmoResult GizmoOnGUIInt(Rect butRect, GizmoRenderParms parms)
        {
            GizmoResult result = base.GizmoOnGUIInt(butRect, parms);

            if (this.ability.Chance > 0f)
            {
                Texture2D texture  = this.ability.AutoCast ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex;
                Rect      position = new Rect(butRect.x + butRect.width - 24f, butRect.y, 24f, 24f);
                GUI.DrawTexture(position, texture);
            }

            if(this.disabled && this.ability.cooldown > Find.TickManager.TicksGame)
                GUI.DrawTexture(butRect.RightPartPixels(butRect.width * ((float) (this.ability.cooldown - Find.TickManager.TicksGame) / this.ability.GetCooldownForPawn())), CooldownTex);
            return result;
        }
    }
}