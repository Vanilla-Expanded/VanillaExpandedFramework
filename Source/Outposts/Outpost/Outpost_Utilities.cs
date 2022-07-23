using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Outposts
{
    public partial class Outpost
    {
        private readonly Dictionary<SkillDef, int> totalSkills = new();

        public int TotalSkill(SkillDef skill)
        {
            if (skillsDirty)
                foreach (var skillDef in DefDatabase<SkillDef>.AllDefs)
                    totalSkills[skillDef] = CapablePawns.Sum(p => p.skills.GetSkill(skill).Level);
            return totalSkills[skill];
        }

        protected virtual bool IsCapable(Pawn pawn)
        {
            if (!pawn.RaceProps.Humanlike) return false;
            if (pawn.skills is null) return false;
            return !Ext.RelevantSkills.Any(skill => pawn.skills.GetSkill(skill).TotallyDisabled);
        }

        public bool Has(Pawn pawn) => occupants.Contains(pawn);
        //Theres a few things that seem to leave 0 or destroyed things in contained. I fixed tend, I think the other ones is stuff decaying maybe? Not sure typically its food stuff doing it
        public void CheckNoDestroyedOrNoStack()
        {
            if (containedItems.Any(x=> x.Destroyed||x.stackCount==0))
            {
                foreach (var item in containedItems.Where(x=>x.Destroyed||x.stackCount==0).ToList()) containedItems.Remove(item);
            }
        }
        public virtual string ProductionString()
        {
            var options = ResultOptions;
            if (Ext is null || options is not {Count: >0}) return "";
            return options.Count switch
            {
                1 => "Outposts.WillProduce.1".Translate(options[0].Amount(CapablePawns.ToList()), options[0].Thing.label, TimeTillProduction).RawText,
                2 => "Outposts.WillProduce.2".Translate(options[0].Amount(CapablePawns.ToList()), options[0].Thing.label, options[1].Amount(CapablePawns.ToList()),
                    options[1].Thing.label, TimeTillProduction).RawText,
                _ => "Outposts.WillProduce.N".Translate(TimeTillProduction, options.Select(ro => ro.Explain(CapablePawns.ToList())).ToLineList("  - ")).RawText
            };
        }

        public virtual string RelevantSkillDisplay() =>
            Ext.RelevantSkills.Select(skill => "Outposts.TotalSkill".Translate(skill.skillLabel, TotalSkill(skill)).RawText).ToLineList();
    }
}