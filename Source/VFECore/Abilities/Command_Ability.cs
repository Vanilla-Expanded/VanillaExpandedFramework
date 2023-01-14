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
            this.disabledReason = reason.Colorize(ColorLibrary.RedReadable);
            this.action         = ability.DoAction;
            this.Order          = 10f + (ability.def.requiredHediff?.hediffDef?.index ?? 0) + (ability.def.requiredHediff?.minimumLevel ?? 0);
            this.shrinkable     = true;
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
            if (parms.shrunk)
            {
                this.defaultDesc = $"{ability.def.LabelCap}\n\n{this.defaultDesc}";
            }

            GizmoResult result = base.GizmoOnGUIInt(butRect, parms);

            if (this.ability.AutoCast)
            {
                var  size     = parms.shrunk ? 12f : 24f;
                Rect position = new Rect(butRect.x + butRect.width - size, butRect.y, size, size);
                GUI.DrawTexture(position, AutoCastTex);
            }

            if (this.disabled && this.ability.cooldown > Find.TickManager.TicksGame)
            {
                float num = ((float)(this.ability.cooldown - Find.TickManager.TicksGame) / this.ability.GetCooldownForPawn());
                GUI.DrawTexture(butRect.RightPartPixels(butRect.width * num), CooldownTex);
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.UpperCenter;
                Widgets.Label(butRect, (1f - num).ToStringPercent("F0"));
                Text.Anchor = TextAnchor.UpperLeft;
            }

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