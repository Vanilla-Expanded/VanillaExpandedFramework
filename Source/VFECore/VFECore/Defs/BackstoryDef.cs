namespace VFECore
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.Runtime.CompilerServices;
	using HarmonyLib;
	using RimWorld;
	using Verse;

	public class TraitEntryBackstory
	{
		public TraitDef defName;
		public int degree = 0;
		public float chance = 100;

		public float commonalityMale = -1f;
		public float commonalityFemale = -1f;
	}

	[HarmonyPatch(typeof(PawnBioAndNameGenerator), "FillBackstorySlotShuffled")]
	public static class PawnBioAndNameGenerator_FillBackstorySlotShuffled
	{
		public static bool Prefix(Pawn pawn, BackstorySlot slot)
		{
			if (slot == BackstorySlot.Adulthood && pawn.story.Childhood is VEBackstoryDef absd && absd.linkedBackstory != null)
			{
				pawn.story.Adulthood = absd.linkedBackstory;
				return false;
			}
			return true;
		}

		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			MethodInfo backstoryDatabaseInfo = AccessTools.PropertyGetter(typeof(DefDatabase<BackstoryDef>), nameof(DefDatabase<BackstoryDef>.AllDefs));

			bool done = false;

			List<CodeInstruction> instructionList = instructions.ToList();

			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction codeInstruction = instructionList[i];
				yield return codeInstruction;

				if (!done && i > 1 && codeInstruction.Calls(backstoryDatabaseInfo))
				{
					done = true;
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Ldarg_1);
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PawnBioAndNameGenerator_FillBackstorySlotShuffled), nameof(Backstories)));
				}
			}
		}

		public static List<BackstoryDef> Backstories(List<BackstoryDef> backstories, Pawn pawn, BackstorySlot slot)
		{
			return backstories.Where(bs =>
									{
										VEBackstoryDef def = bs as VEBackstoryDef;
										return (def?.Approved(pawn) ?? true) && (slot != BackstorySlot.Adulthood || ((def?.linkedBackstory == null) || pawn.story.Childhood == def.linkedBackstory));
									}).ToList();
		}
	}

	[HarmonyPatch(typeof(PawnGenerator), "GenerateSkills")]
	public static class PawnGenerator_GenerateSkills
	{
		public static void Postfix(Pawn pawn)
		{
			foreach (BackstoryDef backstory in pawn.story.AllBackstories)
			{
				if (PASSIONS.TryGetValue(backstory, out var passions))
				{
					foreach (var passion in passions)
					{
						pawn.skills.GetSkill(passion.skill).passion = (Passion)passion.amount;
					}
				}
			}
		}

		public static readonly ConditionalWeakTable<BackstoryDef, List<SkillGain>> PASSIONS = new();
	}

	public class VEBackstoryDef : BackstoryDef
	{
        public List<TraitEntryBackstory>     forcedTraitsChance     = new List<TraitEntryBackstory>();
        public List<TraitEntryBackstory>     disallowedTraitsChance = new List<TraitEntryBackstory>();
        public WorkTags                      workAllows             = WorkTags.AllWork;
        public float                         maleCommonality        = 100f;
        public float                         femaleCommonality      = 100f;
        public BackstoryDef                  linkedBackstory;
		public List<string>                  forcedHediffs = new List<string>();
        public List<SkillGain>               passions      = new List<SkillGain>();
        public IntRange                      bioAgeRange;
        public IntRange                      chronoAgeRange;
        public List<ThingDefCountRangeClass> forcedItems = new List<ThingDefCountRangeClass>();

        public bool CommonalityApproved(Gender g) => Rand.Range(minInclusive: 0, maxInclusive: 100) < (g == Gender.Female ? this.femaleCommonality : this.maleCommonality);

        public bool Approved(Pawn p) => this.CommonalityApproved(p.gender)                                                                                                                              &&
										(this.bioAgeRange    == default || (this.bioAgeRange.min    < p.ageTracker.AgeBiologicalYears    && p.ageTracker.AgeBiologicalYears    < this.bioAgeRange.max)) &&
										(this.chronoAgeRange == default || (this.chronoAgeRange.min < p.ageTracker.AgeChronologicalYears && p.ageTracker.AgeChronologicalYears < this.chronoAgeRange.max));

        public override void ResolveReferences()
        {
			this.identifier = this.defName;
			base.ResolveReferences();

			this.forcedTraits = (this.forcedTraits ??= new List<BackstoryTrait>()).
								Concat(this.forcedTraitsChance.Where(predicate: trait => Rand.Range(minInclusive: 0, maxInclusive: 100) < trait.chance).ToList().ConvertAll(converter: trait => new BackstoryTrait { def = trait.defName, degree = trait.degree })).ToList();
			this.disallowedTraits = (this.disallowedTraits ??= new List<BackstoryTrait>()).
									Concat(this.disallowedTraitsChance.Where(predicate: trait => Rand.Range(minInclusive: 0, maxInclusive: 100) < trait.chance).ToList().ConvertAll(converter: trait => new BackstoryTrait { def = trait.defName, degree = trait.degree })).ToList();
			this.workDisables = (this.workAllows & WorkTags.AllWork) != 0 ? this.workDisables : ~this.workAllows;

			if (this.bodyTypeGlobal == null && this.bodyTypeFemale == null && this.bodyTypeMale == null)
				this.bodyTypeGlobal = DefDatabase<BodyTypeDef>.GetRandom();

			if (!passions.NullOrEmpty())
				PawnGenerator_GenerateSkills.PASSIONS.Add(this, this.passions);
        }
	}
}