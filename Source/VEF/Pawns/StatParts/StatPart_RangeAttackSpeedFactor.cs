using RimWorld;
using Verse;

namespace VEF.Pawns
{
    public class StatPart_RangeAttackSpeedFactor : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.Thing?.ParentHolder is Pawn_EquipmentTracker eq && eq.pawn != null)
            {
                val /= eq.pawn.GetStatValue(VEFDefOf.VEF_RangeAttackSpeedFactor);
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (req.Thing?.ParentHolder is Pawn_EquipmentTracker eq && eq.pawn != null)
            {
                return "VEF_RangeAttackSpeedFactor".Translate() + ": x" +
                    eq.pawn.GetStatValue(VEFDefOf.VEF_RangeAttackSpeedFactor).ToStringPercent();
            }
            return null;
        }
    }
}