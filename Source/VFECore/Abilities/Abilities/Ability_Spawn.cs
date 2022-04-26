using RimWorld;
using Verse;

namespace VFECore.Abilities
{
    public class Ability_Spawn : Ability
    {
        public override float Chance => 0f;

        public override void Cast(LocalTargetInfo target)
        {
            base.Cast(target);

            var extension = def.GetModExtension<AbilityExtension_Spawn>();

            if (extension?.thing != null) GenSpawn.Spawn(extension.thing, target.Cell, pawn.Map);
        }

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            var extension = def.GetModExtension<AbilityExtension_Spawn>();
            if (target.Cell.Filled(pawn.Map) || target.Cell.GetFirstBuilding(pawn.Map) != null && !(extension?.allowOnBuildings ?? true))
            {
                if (showMessages)
                    Messages.Message("AbilityOccupiedCells".Translate(def.LabelCap), target.ToTargetInfo(pawn.Map),
                        MessageTypeDefOf.RejectInput, false);

                return false;
            }

            return base.ValidateTarget(target, showMessages);
        }
    }

    public class AbilityExtension_Spawn : DefModExtension
    {
        public ThingDef thing;
        public bool     allowOnBuildings;
    }
}
