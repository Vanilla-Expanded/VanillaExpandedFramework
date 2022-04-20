using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VFECore
{
	public class TraitEntryBackstory
	{
		public TraitDef defName;

		public int degree;

		public int chance;
	}

    [HarmonyPatch(typeof(PawnBioAndNameGenerator), "FillBackstorySlotShuffled")]
    public static class PawnBioAndNameGenerator_FillBackstorySlotShuffled
    {
        [HarmonyDebug]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) // mostly copied from AlienRaces
																										 // as it seems to be only one way to add in
																										 // age and gender restrictions for backstories
        {
			MethodInfo shuffleableInfo = AccessTools.Method(typeof(BackstoryDatabase), "ShuffleableBackstoryList");
			foreach (CodeInstruction codeInstruction in instructions)
			{
				yield return codeInstruction;
				if (codeInstruction.opcode == OpCodes.Call && codeInstruction.OperandIs(shuffleableInfo))
				{
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Call, 
						AccessTools.Method(typeof(PawnBioAndNameGenerator_FillBackstorySlotShuffled), nameof(Backstories)));
				}
			}
		}

		public static List<Backstory> Backstories(List<Backstory> backstories, Pawn pawn)
		{
			return backstories.Where(delegate (Backstory bs)
			{
				var def = DefDatabase<BackstoryDef>.GetNamedSilentFail(bs.identifier);
                if (def != null)
                {
					if (def.maleCommonality.HasValue && Rand.Chance(def.maleCommonality.Value) && pawn.gender != Gender.Male)
                    {
						return false;
                    }
                    if (def.chronologicalAgeRestriction.HasValue && !def.chronologicalAgeRestriction.Value.Includes(pawn.ageTracker.AgeChronologicalYearsFloat))
                    {
                        return false;
                    }
                    if (def.biologicalAgeRestriction.HasValue && !def.biologicalAgeRestriction.Value.Includes(pawn.ageTracker.AgeBiologicalYearsFloat))
                    {
                        return false;
                    }
                }
				return true;
			}).ToList();
		}
	}
	public class BackstoryDef : Def
	{
        public float? maleCommonality = 50f;
        public FloatRange? chronologicalAgeRestriction;
		public FloatRange? biologicalAgeRestriction;
		public override void ResolveReferences()
		{
			Backstory backstory = new Backstory();
			if (this.forcedTraits?.Any() ?? false)
            {
				backstory.forcedTraits = new List<TraitEntry>();
				foreach (var trait in this.forcedTraits.Where(x => Rand.RangeInclusive(0, 100) < x.chance))
				{
					backstory.forcedTraits.Add(new TraitEntry(trait.defName, trait.degree));
				}
			}

			if (this.disallowedTraits?.Any() ?? false)
            {
				backstory.disallowedTraits = new List<TraitEntry>();
				foreach (var trait in this.disallowedTraits.Where(x => Rand.RangeInclusive(0, 100) < x.chance))
				{
					backstory.disallowedTraits.Add(new TraitEntry(trait.defName, trait.degree));
				}
			}

			backstory.SetTitle(this.title, this.title);
			if (!GenText.NullOrEmpty(this.titleShort))
			{
				backstory.SetTitleShort(this.titleShort, this.titleShort);
			}
			else
			{
				backstory.SetTitleShort(backstory.title, backstory.title);
			}
			if (!GenText.NullOrEmpty(this.baseDescription))
			{
				backstory.baseDesc = this.baseDescription;
			}

			Traverse.Create(backstory).Field("bodyTypeGlobal").SetValue(this.bodyTypeGlobal);
			Traverse.Create(backstory).Field("bodyTypeMale").SetValue(this.bodyTypeMale);
			Traverse.Create(backstory).Field("bodyTypeFemale").SetValue(this.bodyTypeFemale);
			if (skillGains?.Any() ?? false)
            {
				var skillGainsDict = skillGains.ToDictionary(x => x.skill.defName, y => y.minLevel);
				Traverse.Create(backstory).Field("skillGains").SetValue(skillGainsDict);
            }

			backstory.slot = this.slot;
			backstory.shuffleable = this.shuffleable;
			backstory.spawnCategories = this.spawnCategories;
			if (this.workDisables.Any())
			{
				foreach (var workTag in this.workDisables)
                {
					backstory.workDisables |= workTag;
				}
			}
			else
            {
				backstory.workDisables = WorkTags.None;
            }

			backstory.PostLoad();
			backstory.ResolveReferences();
			backstory.identifier = this.defName;

			if (!backstory.ConfigErrors(true).Any())
			{
				BackstoryDatabase.AddBackstory(backstory);
			}
			else
			{
				foreach (var err in backstory.ConfigErrors(true))
                {
					Log.Error(backstory + " - " + err);
                }
			}
		}

        public string baseDescription;

		public string bodyTypeGlobal = "";

		public string bodyTypeMale = "Male";

		public string bodyTypeFemale = "Female";

		public string title;

		public string titleShort;

		public BackstorySlot slot = BackstorySlot.Childhood;

		public bool shuffleable = true;

		public List<WorkTags> workDisables = new List<WorkTags>();

		public List<string> spawnCategories = new List<string>();

		public List<SkillRequirement> skillGains;

		public List<TraitEntryBackstory> forcedTraits = new List<TraitEntryBackstory>();

		public List<TraitEntryBackstory> disallowedTraits = new List<TraitEntryBackstory>();
	}
}
