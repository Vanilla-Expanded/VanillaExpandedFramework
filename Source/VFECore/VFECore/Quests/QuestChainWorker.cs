using System.Collections.Generic;
using Verse;
using RimWorld;
using System.Text.RegularExpressions;
using Verse.Grammar;

namespace VFECore
{
    public class QuestChainWorker
    {
        public QuestChainDef def;

        private string _cachedDescription;

        public QuestChainState State =>
            GameComponent_QuestChains.Instance.GetStateFor(def);

        public void EnsureAllUniquePawnsCreated()
        {
            if (def.uniqueCharacters != null)
            {
                foreach (var pawnKind in def.uniqueCharacters)
                {
                    var ext = pawnKind.GetModExtension<UniqueCharacterExtension>();
                    if (State.GetUniquePawn(ext.tag) == null)
                    {
                        CreateAndStoreUniquePawn(pawnKind, ext);
                    }
                }
            }
        }

        public virtual Pawn CreateAndStoreUniquePawn(PawnKindDef kind, UniqueCharacterExtension ext)
        {
            Pawn pawn = GeneratePawn(kind);
            Log.Message($"Creating unique pawn {pawn.Name} {pawn.thingIDNumber} with faction {pawn.Faction?.def} for quest chain {def.defName} with tag {ext.tag}");
            State.StoreUniquePawn(ext.tag, pawn, deepSave: true);
            InvalidateDescriptionCache();
            return pawn;
        }

        private static readonly List<CreepJoinerBaseDef> requires = new List<CreepJoinerBaseDef>();

        private static readonly List<CreepJoinerBaseDef> exclude = new List<CreepJoinerBaseDef>();

        public virtual Pawn GeneratePawn(PawnKindDef kind)
        {
            Faction faction = null;
            if (kind.defaultFactionType != null)
            {
                faction = Find.FactionManager.FirstFactionOfDef(kind.defaultFactionType);
            }
            Pawn pawn = null;
            if (kind is CreepJoinerFormKindDef form)
            {
                requires.AddRange(form.Requires);
                exclude.AddRange(form.Excludes);
                var combatPoints = form.combatPower;
                CreepJoinerBenefitDef benefit = CreepJoinerUtility.GetRandom(DefDatabase<CreepJoinerBenefitDef>.AllDefsListForReading, combatPoints, requires, exclude);
                CreepJoinerDownsideDef downside = CreepJoinerUtility.GetRandom(DefDatabase<CreepJoinerDownsideDef>.AllDefsListForReading, combatPoints, requires, exclude);
                CreepJoinerAggressiveDef aggressive = CreepJoinerUtility.GetRandom(DefDatabase<CreepJoinerAggressiveDef>.AllDefsListForReading, combatPoints, requires, exclude);
                CreepJoinerRejectionDef rejection = CreepJoinerUtility.GetRandom(DefDatabase<CreepJoinerRejectionDef>.AllDefsListForReading, combatPoints, requires, exclude);
                PawnGenerationRequest request = new PawnGenerationRequest(form, null, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: true, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: true, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, DevelopmentalStage.Adult, null, null, null, forceRecruitable: true);
                request.AllowedDevelopmentalStages = DevelopmentalStage.Adult;
                request.ForceGenerateNewPawn = true;
                request.AllowFood = true;
                request.DontGiveWeapon = true;
                request.OnlyUseForcedBackstories = form.fixedAdultBackstories.Any();
                request.MaximumAgeTraits = 1;
                request.MinimumAgeTraits = 1;
                request.IsCreepJoiner = true;
                request.ForceNoIdeoGear = true;
                request.MustBeCapableOfViolence = true;
                request.Faction = faction;
                pawn = PawnGenerator.GeneratePawn(request);
                Pawn_CreepJoinerTracker creepjoiner = pawn.creepjoiner;
                creepjoiner.form = form;
                creepjoiner.benefit = benefit;
                creepjoiner.downside = downside;
                creepjoiner.aggressive = aggressive;
                creepjoiner.rejection = rejection;
                ApplyExtraTraits(pawn, benefit.traits);
                ApplyExtraTraits(pawn, downside.traits);
                ApplyExtraHediffs(pawn, benefit.hediffs);
                ApplyExtraHediffs(pawn, downside.hediffs);
                ApplySkillOverrides(pawn, benefit.skills);
                ApplyExtraAbilities(pawn, benefit.abilities);
                ApplyExtraAbilities(pawn, downside.abilities);
                pawn.guest.Recruitable = false;
                creepjoiner.Notify_Created();
            }
            else
            {
                pawn = PawnGenerator.GeneratePawn(kind, faction);
            }
            var nameMaker = pawn.gender == Gender.Female && kind.nameMakerFemale != null
                ? kind.nameMakerFemale : kind.nameMaker;
            if (nameMaker != null)
            {
                pawn.Name = NameResolvedFrom(nameMaker);
            }
            exclude.Clear();
            requires.Clear();
            return pawn;
        }

        private static Name NameResolvedFrom(RulePackDef nameMaker, bool forceNoNick = false, List<Rule> extraRules = null)
        {
            return NameTriple.FromString(NameGenerator.GenerateName(nameMaker, (string x) => !NameTriple.FromString(x).UsedThisGame, appendNumberIfNameUsed: false, null, null, extraRules), forceNoNick);
        }

        private static void ApplySkillOverrides(Pawn pawn, List<CreepJoinerBenefitDef.SkillValue> skills)
        {
            foreach (CreepJoinerBenefitDef.SkillValue skill2 in skills)
            {
                SkillRecord skill = pawn.skills.GetSkill(skill2.skill);
                skill.Level = skill2.range.RandomInRange;
                skill.xpSinceMidnight = 0f;
                skill.xpSinceLastLevel = 0f;
            }
        }

        private static void ApplyExtraTraits(Pawn pawn, List<BackstoryTrait> traits)
        {
            foreach (BackstoryTrait trait in traits)
            {
                if (!pawn.story.traits.HasTrait(trait.def))
                {
                    pawn.story.traits.GainTrait(new Trait(trait.def, trait.degree, forced: true));
                }
            }
        }

        private static void ApplyExtraHediffs(Pawn pawn, List<HediffDef> hediffs)
        {
            foreach (HediffDef hediff in hediffs)
            {
                pawn.health.AddHediff(hediff);
            }
        }

        private static void ApplyExtraAbilities(Pawn pawn, List<AbilityDef> abilities)
        {
            foreach (AbilityDef ability in abilities)
            {
                pawn.abilities.GainAbility(ability);
            }
        }

        public virtual Pawn GetUniquePawn(string tag)
        {
            var pawn = State.GetUniquePawn(tag);
            if (pawn != null)
            {
                return pawn;
            }
            EnsureAllUniquePawnsCreated();
            return State.GetUniquePawn(tag);
        }

        public virtual string GetDescription()
        {
            if (_cachedDescription == null)
            {
                string desc = def.description;
                _cachedDescription = Regex.Replace(desc, @"\[(\w+?)_(\w+?)\]", match =>
                {
                    string tag = match.Groups[1].Value;
                    string property = match.Groups[2].Value;
                    Pawn pawn = GetUniquePawn(tag);
                    if (pawn == null) return match.Value;

                    string value = property switch
                    {
                        "FullName" => pawn.Name?.ToStringFull ?? pawn.LabelCap,
                        "ShortName" => pawn.Name?.ToStringShort ?? pawn.LabelShortCap,
                        "Label" => pawn.LabelCap,
                        _ => ""
                    };

                    if (string.IsNullOrEmpty(value)) return match.Value;
                    return value.Colorize(PawnNameColorUtility.PawnNameColorOf(pawn));
                });
            }
            return _cachedDescription;
        }

        public void InvalidateDescriptionCache()
        {
            _cachedDescription = null;
        }
    }
}