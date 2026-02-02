using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Things;

[HarmonyPatch]
public class StatPart_Quality_Offset_AsPercentage : StatPart_Quality_Offset
{
    // Those methods are private in the base method.
    // We can either:
    // - Copy-paste original methods (Harmony patches on original will be ignored, as well as any future changes in Vanilla)
    // - Copy-paste the original class (same as above, other mods accessing it won't be able to access this as easily)
    // - Harmony reverse patcher on original ExplanationPart (probably an overkill)
    // - Call the original methods directly through delegates like how It's done here (simple solution)
    private static Func<StatPart_Quality_Offset, StatRequest, bool> applyToMethod =
        NonPublicMethods.MakeDelegate<Func<StatPart_Quality_Offset, StatRequest, bool>>(typeof(StatPart_Quality_Offset).Method("ApplyTo"));
    private static Func<StatPart_Quality_Offset, QualityCategory, float> qualityOffsetMethod =
        NonPublicMethods.MakeDelegate<Func<StatPart_Quality_Offset, QualityCategory, float>>(typeof(StatPart_Quality_Offset).Method("QualityOffset"));

    public override string ExplanationPart(StatRequest req)
    {
        if (!applyToMethod(this, req))
            return null;
        return $"{"StatsReport_QualityOffset".Translate()}: {qualityOffsetMethod(this, req.QualityCategory).ToStringPercentSigned()}";
    }
}