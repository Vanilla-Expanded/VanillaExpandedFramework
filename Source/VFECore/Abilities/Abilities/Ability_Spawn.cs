using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VFECore.Abilities
{
    public class Ability_Spawn : Ability
    {
        public override bool CanAutoCast => false;

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);

            var extension = def.GetModExtension<AbilityExtension_Spawn>();

            if (extension?.thing != null)
                for (var i = 0; i < targets.Length; i++)
                    Spawn(targets[i], extension.thing, this);
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

        public static void Spawn(GlobalTargetInfo target, ThingDef def, Ability ability)
        {
            var thing                                                                  = ThingMaker.MakeThing(def);
            if (thing.TryGetComp<CompDuration>() is { } comp1) comp1.durationTicksLeft = ability.GetDurationForPawn();
            if (thing.TryGetComp<CompAbilitySpawn>() is { } comp2)
            {
                comp2.pawn   = ability.pawn;
                comp2.source = ability;
            }
            if (thing.def.CanHaveFaction) thing.SetFactionDirect(ability.pawn.Faction);
            GenSpawn.Spawn(thing, target.Cell, target.Map);
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

    public class CompAbilitySpawn : ThingComp
    {
        public Pawn    pawn;
        public Ability source;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref pawn,   "spawningPawn");
            Scribe_References.Look(ref source, "abilitySource");
        }
    }
}