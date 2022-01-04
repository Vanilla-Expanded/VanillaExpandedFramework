using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Outposts
{
    public class Outpost_ChooseResult : Outpost
    {
        private ThingDef choice;

        protected OutpostExtension_Choose ChooseExt => Ext as OutpostExtension_Choose;

        public override List<ResultOption> ResultOptions => Ext.ResultOptions.OrEmpty().Concat(GetExtraOptions()).Where(ro => ro.Thing == choice).ToList();

        public override IEnumerable<Gizmo> GetGizmos()
        {
            return base.GetGizmos().Append(new Command_Action
            {
                action = () => Find.WindowStack.Add(new FloatMenu(Ext.ResultOptions.OrEmpty().Concat(GetExtraOptions()).Select(ro => ro.MinSkills?.SatisfiedBy(CapablePawns) ?? true
                        ? new FloatMenuOption(ro.Explain(CapablePawns.ToList()), () => choice = ro.Thing, ro.Thing)
                        : new FloatMenuOption(ro.Explain(CapablePawns.ToList()) + " - " + "Outposts.SkillTooLow".Translate(ro.MinSkills.Max(abs => abs.Count)), null, ro.Thing))
                    .ToList())),
                defaultLabel = ChooseExt.ChooseLabel.Formatted(choice.label),
                defaultDesc = ChooseExt.ChooseDesc,
                icon = choice.uiIcon
            });
        }

        public override void RecachePawnTraits()
        {
            base.RecachePawnTraits();
            choice ??= Ext.ResultOptions.OrEmpty().Concat(GetExtraOptions()).MinBy(ro => ro.MinSkills?.Sum(abs => abs.Count) ?? 0f).Thing;
            if (ResultOptions.FirstOrDefault(ro => !(ro.MinSkills?.SatisfiedBy(CapablePawns) ?? true)) is {Thing: {label: var produced}})
            {
                Messages.Message("Outposts.SkillChange".Translate(Name, produced), this, MessageTypeDefOf.NegativeEvent);
                choice = Ext.ResultOptions.OrEmpty().Concat(GetExtraOptions()).MinBy(ro => ro.MinSkills?.Sum(abs => abs.Count) ?? 0f).Thing;
            }
        }

        public virtual IEnumerable<ResultOption> GetExtraOptions()
        {
            yield break;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref choice, "choice");
        }
    }

    public class OutpostExtension_Choose : OutpostExtension
    {
        public string ChooseDesc;
        public string ChooseLabel;
    }
}