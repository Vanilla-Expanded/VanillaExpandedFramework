using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace VFECore.Misc
{
    [StaticConstructorOnStartup]
    public class StatPart_Ammo : StatPart
    {
        private static StatDef statRangedCooldownFactor;

        static StatPart_Ammo()
        {
            LongEventHandler.ExecuteWhenFinished(() =>
                statRangedCooldownFactor = StatDef.Named("VEF_RangedCooldownFactor"));
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!req.HasThing || !(req.Thing.ParentHolder is Pawn_EquipmentTracker eq)) return;
            foreach (var thing in GetAllEquipment(eq))
                if (!(thing.def.GetModExtension<EquipmentOffsetConditions>() is EquipmentOffsetConditions conds) || conds.IsValid(req.Thing, thing.def))
                    val *= thing.GetStatValue(statRangedCooldownFactor);
        }

        private static IEnumerable<ThingWithComps> GetAllEquipment(Pawn_EquipmentTracker eq)
        {
            var equipments = eq.AllEquipmentListForReading.ToList();
            if (eq.pawn?.apparel?.WornApparel != null)
            {
                equipments.AddRange(eq.pawn.apparel.WornApparel.ToList());
            }
            return equipments;
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!req.HasThing || !(req.Thing.ParentHolder is Pawn_EquipmentTracker eq))
                return "";
            var builder = new StringBuilder();
            foreach (var (thing, conds) in from apparel in GetAllEquipment(eq)
                let conds = apparel.def
                    .GetModExtension<EquipmentOffsetConditions>()
                where conds != null || Math.Abs(apparel.GetStatValue(statRangedCooldownFactor) - 1f) > 0.001f
                select (apparel, conds))
                builder.AppendLine(
                    conds?.IsValid(req.Thing, thing.def) ?? true
                        ? $"{thing.def.LabelCap}: {statRangedCooldownFactor.Worker.ValueToString(thing.GetStatValue(statRangedCooldownFactor), true, ToStringNumberSense.Factor)}"
                        : $"{thing.def.LabelCap}: {req.Thing.def.LabelCap} {"VFECore.IsToo".Translate()} {(req.Thing.def.techLevel > conds.techLevels.Max() ? "VFECore.Advanced".Translate() : "VFECore.Primitive".Translate())}");
            return builder.ToString();
        }
    }
}