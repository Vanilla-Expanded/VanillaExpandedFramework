using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace KCSG.UndergroundRoom
{
    public class CompProperties_UndergroundPassage : CompProperties
    {
        public CompProperties_UndergroundPassage() => compClass = typeof(CompUndergroundPassage);

        public string cannotGoKey = "KCSG.CantEnter";
        public string goKey = "KCSG.Enter";

        public List<StructureLayoutDef> mapLayouts = new List<StructureLayoutDef>();
    }

    public class CompUndergroundPassage : ThingComp
    {
        private static readonly List<Pawn> tmpAllowedPawns = new List<Pawn>();

        public CompUndergroundPassage otherSide;

        public CompProperties_UndergroundPassage Props => (CompProperties_UndergroundPassage)props;

        public Map Map => parent.Map;

        public override void PostDeSpawn(Map map)
        {
            Find.World.GetComponent<UndergroundManager>().Despawn(this);
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref otherSide, "otherSide");
        }

        public override IEnumerable<FloatMenuOption> CompMultiSelectFloatMenuOptions(List<Pawn> selPawns)
        {
            tmpAllowedPawns.Clear();
            for (int i = 0; i < selPawns.Count; i++)
            {
                if (selPawns[i].CanReach(parent, PathEndMode.Touch, Danger.Deadly))
                {
                    tmpAllowedPawns.Add(selPawns[i]);
                }
            }
            if (tmpAllowedPawns.Count <= 0)
            {
                yield return new FloatMenuOption(Props.cannotGoKey.Translate(parent.Label) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
                yield break;
            }

            yield return new FloatMenuOption(Props.goKey.Translate(parent.Label), delegate
            {
                for (int l = 0; l < tmpAllowedPawns.Count; l++)
                    tmpAllowedPawns[l].jobs.TryTakeOrderedJob(JobMaker.MakeJob(AllDefOf.KCSG_GoToPassage, parent), JobTag.Misc);
            });
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if (!selPawn.CanReach(parent, PathEndMode.Touch, Danger.Deadly))
            {
                yield return new FloatMenuOption(Props.cannotGoKey.Translate(parent.Label) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
                yield break;
            }

            yield return new FloatMenuOption(Props.goKey.Translate(parent.Label), delegate
            {
                selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(AllDefOf.KCSG_GoToPassage, parent), JobTag.Misc);
            });
        }
    }
}
