using System.Collections.Generic;
using System.Linq;
using System.Xml;
using RimWorld;
using UnityEngine;
using Verse;

namespace Outposts
{
    public class OutpostExtension : DefModExtension
    {
        public List<BiomeDef> AllowedBiomes;
        public List<ThingDefCountClass> CostToMake;
        public List<BiomeDef> DisallowedBiomes;
        public List<SkillDef> DisplaySkills;
        public HistoryEventDef Event;

        [PostToSetings("Outposts.Setting.MinimumPawns", PostToSetingsAttribute.DrawMode.IntSlider, min: 1f, max: 10f, dontShowAt: 0)]
        public int MinPawns;

        public ThingDef ProvidedFood;

        [PostToSetings("Outposts.Setting.Range", PostToSetingsAttribute.DrawMode.IntSlider, min: 1f, max: 30f, dontShowAt: -1)]
        public int Range = -1;

        public List<AmountBySkill> RequiredSkills;
        public bool RequiresGrowing;
        public List<ResultOption> ResultOptions;

        [PostToSetings("Outposts.Setting.ProductionTime", PostToSetingsAttribute.DrawMode.Time, dontShowAt: -1)]
        public int TicksPerProduction = 15 * 60000;

        [PostToSetings("Outposts.Setting.PackTime", PostToSetingsAttribute.DrawMode.Time)]
        public int TicksToPack = 7 * 60000;

        public int TicksToSetUp = -1;

        public List<SkillDef> RelevantSkills =>
            new HashSet<SkillDef>(RequiredSkills.SelectOrEmpty(rq => rq.Skill)
                    .Concat(ResultOptions.SelectManyOrEmpty(ro => ro.AmountsPerSkills.SelectOrEmpty(aps => aps.Skill).Concat(ro.MinSkills.SelectOrEmpty(ms => ms.Skill))))
                    .Concat(DisplaySkills.OrEmpty()))
                .ToList();
    }

    public class ResultOption
    {
        public int AmountPerPawn;
        public List<AmountBySkill> AmountsPerSkills;
        public int BaseAmount;
        public List<AmountBySkill> MinSkills;
        public ThingDef Thing;

        public int Amount(List<Pawn> pawns) =>
            Mathf.RoundToInt((BaseAmount + AmountPerPawn * pawns.Count + (AmountsPerSkills?.Sum(x => x.Amount(pawns)) ?? 0)) * OutpostsMod.Settings.ProductionMultiplier);

        public IEnumerable<Thing> Make(List<Pawn> pawns) => Thing.Make(Amount(pawns));
        public string Explain(List<Pawn> pawns) => $"{Amount(pawns)}x {Thing.LabelCap}";
    }

    public class AmountBySkill
    {
        public int Count;
        public SkillDef Skill;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            if (xmlRoot.ChildNodes.Count != 1)
            {
                Log.Error("Misconfigured AmountBySkill: " + xmlRoot.OuterXml);
                return;
            }

            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "Skill", xmlRoot.Name);
            Count = ParseHelper.FromString<int>(xmlRoot.FirstChild.Value);
        }

        public int Amount(List<Pawn> pawns) => Count * pawns.Sum(p => p.skills.GetSkill(Skill).Level);
    }
}