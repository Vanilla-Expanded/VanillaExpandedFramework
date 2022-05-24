using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VFECore.Abilities
{
    public class Ability_Spawn : Ability
    {
        public override float Chance => 0f;

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);

            var extension = def.GetModExtension<AbilityExtension_Spawn>();

            if (extension?.thing != null)
                for (var i = 0; i < targets.Length; i++)
                {
                    var thing = GenSpawn.Spawn(extension.thing, targets[i].Cell, pawn.Map);
                    if (thing.TryGetComp<CompDuration>() is CompDuration comp) comp.durationTicksLeft = GetDurationForPawn();
                }
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
        public bool     allowOnBuildings;
        public ThingDef thing;
    }

    public class CompDuration : ThingComp
    {
        public int durationTicksLeft;

        public override void CompTick()
        {
            base.CompTick();
            durationTicksLeft--;
            if (durationTicksLeft <= 0) parent.Destroy();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref durationTicksLeft, nameof(durationTicksLeft));
        }
    }
}