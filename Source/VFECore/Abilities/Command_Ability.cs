using UnityEngine;
using Verse;

namespace VFECore.Abilities
{
    [StaticConstructorOnStartup]
    public class Command_Ability : Command_Action
    {
        public static readonly Texture2D CooldownTex =
            SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.1f));

        public static readonly Texture2D AutoCastTex = ContentFinder<Texture2D>.Get("UI/CheckAuto");

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
            this.order          = 10f + (ability.def.requiredHediff?.hediffDef?.index ?? 0) + (ability.def.requiredHediff?.minimumLevel ?? 0);
            //this.shrinkable     = true;
        }

        public override void GizmoUpdateOnMouseover()
        {
            base.GizmoUpdateOnMouseover();
            this.ability.GizmoUpdateOnMouseover();
        }

        public override bool GroupsWith(Gizmo other) => 
            other is Command_Ability ca && ca.ability.def == this.ability.def;

        protected override GizmoResult GizmoOnGUIInt(Rect butRect, GizmoRenderParms parms)
        {
            GizmoResult result = base.GizmoOnGUIInt(butRect, parms);

            if (this.ability.AutoCast)
            {
                Rect      position = new Rect(butRect.x + butRect.width - 24f, butRect.y, 24f, 24f);
                GUI.DrawTexture(position, AutoCastTex);
            }

            if(this.disabled && this.ability.cooldown > Find.TickManager.TicksGame)
                GUI.DrawTexture(butRect.RightPartPixels(butRect.width * ((float) (this.ability.cooldown - Find.TickManager.TicksGame) / this.ability.GetCooldownForPawn())), CooldownTex);
            
            if (Mouse.IsOver(butRect))
            {
                if (this.ability.def.targetModes[0] == AbilityTargetingMode.Self && this.ability.def.targetCount == 1)
                {
                    this.ability.OnGUI(this.pawn);
                }
            }

            return result;
        }
    }
}