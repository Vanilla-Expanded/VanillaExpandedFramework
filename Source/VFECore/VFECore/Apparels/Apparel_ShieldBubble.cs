using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VFECore
{
    [StaticConstructorOnStartup]
    public class Gizmo_EnergyCompShieldStatus : Gizmo
    {
        public CompShieldBubble shield;

        private static readonly Texture2D FullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));

        private static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

        public Gizmo_EnergyCompShieldStatus()
        {
            Order = -100f;
        }

        public override float GetWidth(float maxWidth)
        {
            return 140f;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Rect rect2 = rect.ContractedBy(6f);
            Widgets.DrawWindowBackground(rect);
            Rect rect3 = rect2;
            rect3.height = rect.height / 2f;
            Text.Font = GameFont.Tiny;
            Widgets.Label(rect3, shield.parent.LabelCap);
            Rect rect4 = rect2;
            rect4.yMin = rect2.y + rect2.height / 2f;
            float fillPercent = shield.Energy / shield.EnergyMax;
            Widgets.FillableBar(rect4, fillPercent, FullShieldBarTex, EmptyShieldBarTex, doBorder: false);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect4, shield.Energy.ToString("F0") + " / " + shield.EnergyMax.ToString("F0"));
            Text.Anchor = TextAnchor.UpperLeft;
            return new GizmoResult(GizmoState.Clear);
        }
    }

    public class Apparel_ShieldBubble : Apparel
    {
        private CompShieldBubble shieldComp;
        CompShieldBubble ShieldComp
        {
            get
            {
                if (shieldComp is null)
                {
                    shieldComp = this.TryGetComp<CompShieldBubble>();
                }
                return shieldComp;
            }
        }

        public override void PostPostMake()
        {
            base.PostPostMake();
            var comp = ShieldComp;
            if (comp.Props.chargeFullyWhenMade)
            {
                comp.Energy = comp.EnergyMax;
            }
        }
        public override bool CheckPreAbsorbDamage(DamageInfo dinfo)
        {
            var shield = ShieldComp;
            if (shield != null)
            {
                return shield.AbsorbingDamage(dinfo, out bool absorbed);
            }
            else
            {
                return base.CheckPreAbsorbDamage(dinfo);
            }
        }
        public override void DrawWornExtras()
        {
            base.DrawWornExtras();
            Comps_PostDraw();
        }
        public override bool AllowVerbCast(Verb verb)
        {
            var comp = shieldComp;
            if (comp != null)
            {
                if (verb.IsMeleeAttack && comp.Props.dontAllowMeleeAttack)
                {
                    return false;
                }
                else if (!verb.IsMeleeAttack && comp.Props.dontAllowRangedAttack)
                {
                    return false;
                }
            }
            return base.AllowVerbCast(verb);
        }
        public override IEnumerable<Gizmo> GetWornGizmos()
        {
            foreach (Gizmo wornGizmo in base.GetWornGizmos())
            {
                yield return wornGizmo;
            }
            if (Find.Selector.SingleSelectedThing == base.Wearer && base.Wearer.IsColonistPlayerControlled)
            {
                Gizmo_EnergyCompShieldStatus gizmo_EnergyShieldStatus = new Gizmo_EnergyCompShieldStatus();
                gizmo_EnergyShieldStatus.shield = ShieldComp;
                yield return gizmo_EnergyShieldStatus;
            }
        }
    }
}
