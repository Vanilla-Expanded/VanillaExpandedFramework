using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace VEF.Weapons;

public class CompProperties_MultiVerbWeapon : CompProperties
{
    [NoTranslate] public string defaultVerbLabel;
    [MustTranslate] public string statExplanationLabel;
    public SwitchMode switchMode = SwitchMode.SingleSwitchGizmo;

    public List<VerbData> verbs = new();

    [MustTranslate] public string gizmoLabel;
    [MustTranslate] public string gizmoDescription;
    [NoTranslate] public string gizmoIconPath;
    [Unsaved] public Texture2D gizmoIcon;

    public CompProperties_MultiVerbWeapon() => compClass = typeof(CompMultiVerbWeapon);

    public override void PostLoadSpecial(ThingDef parent)
    {
        base.PostLoadSpecial(parent);

        LongEventHandler.ExecuteWhenFinished(() =>
        {
            gizmoIcon = ContentFinder<Texture2D>.Get(gizmoIconPath);

            foreach (var verb in verbs)
                verb.LoadIcons();
        });
    }

    public enum SwitchMode
    {
        DoubleVerbToggle,
        DoubleVerbToggleMirrored,
        SingleSwitchGizmo,
        MultiSwitchGizmo,
        FloatMenuGizmo
    }

    public class VerbData
    {
        [NoTranslate] public string verbLabel;
        [MustTranslate] public string statExplanationLabelOverride;

        public List<StatModifier> statOffsets;
        public List<StatModifier> statFactors;

        [MustTranslate] public string gizmoLabelOverride;
        [MustTranslate] public string gizmoDescriptionOverride;
        [NoTranslate] public string gizmoIconPathOverride;
        [Unsaved] public Texture2D gizmoIconOverride;

        public void LoadIcons()
        {
            if (!gizmoIconPathOverride.NullOrEmpty())
            {
                gizmoIconOverride = ContentFinder<Texture2D>.Get(gizmoIconPathOverride);
                if (gizmoIconOverride == BaseContent.BadTex)
                    gizmoIconOverride = null;
            }
        }
    }
}