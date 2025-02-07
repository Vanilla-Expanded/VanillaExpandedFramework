using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static HarmonyLib.Code;

namespace VFECore
{
    public class ConditionalGraphicSet
    {
        public enum CGTrigger
        {
            Humanlike,
            SlaveOrPrisoner,
            Colonist,
            NonColonist,
            Slave,
            Prisoner,
            OfColony,

            HostileToPlayer,
            NeutralToPlayer,
            AlliedToPlayer,

            HasFaction,
            HasIdeo,

            Dead,
            Rotted,
            Dessicated,
            Downed,

            RoyaltyDLC,
            Psycaster,

            IdeologyDLC,

            BiotechDLC,
            Bloodfeeder,

            AnomalyDLC,
            Mutant,
            Ghoul,
        }
        public List<ConditionalGraphicSet> alts = [];

        /// <summary>
        /// Checks if the pawn/game-state meets some complex requirement.
        /// </summary>
        public List<CGTrigger> requirements = [];

        /// <summary>
        /// Checks if the pawn's ideology has the required meme.
        /// E.g. "Raider", "Cannibal", "Tree Connection"
        /// </summary>
        public List<MemeDef> requiredMemes = [];

        /// <summary>
        /// Checks if the given tag exists. E.g. "HeraldicColorA"
        /// </summary>
        public List<string> tagRequirements = [];

        public bool twoClrMask = true;

        protected AdvancedColor colorA = null;
        protected AdvancedColor colorB = null;

        public ShaderTypeDef shader = null;
        public bool useSkinShader = false;

        public bool useFactionRNGSeed = false;

        [NoTranslate]
        public string texPath;

        [NoTranslate]
        public List<string> texPaths;

        [NoTranslate]
        public string texPathFemale;

        [NoTranslate]
        public List<string> texPathsFemale;

        [NoTranslate]
        public string maskPath = null;

        [NoTranslate]
        public List<string> maskPaths;

        public List<BodyTypeGraphicData> bodyTypeGraphicPaths;

        public ConditionalGraphicSet GetActiveGraphicsSet(Pawn pawn, PawnRenderNode node)
        {
            var targetSet = this;

            foreach (var alt in alts)
            {
                if (alt.GetState(pawn, node))
                {
                    targetSet = alt;
                    break;
                }
            }

            return targetSet;
        }

        public Color GetColorA(PawnRenderNode renderNode, Color fallback) => colorA == null ? fallback : colorA.GetColor(renderNode, fallback);
        public Color GetColorB(PawnRenderNode renderNode, Color fallback) => colorB == null ? fallback : colorB.GetColor(renderNode, fallback);

        public bool GetState(Pawn pawn, PawnRenderNode node)
        {
            if (requirements.NullOrEmpty() && tagRequirements.NullOrEmpty() && requiredMemes.NullOrEmpty())
            {
                return true;
            }

            var requirementChecks = new Dictionary<CGTrigger, Func<bool>>
            {
                
                { CGTrigger.Humanlike, () => pawn.RaceProps.Humanlike },
                { CGTrigger.SlaveOrPrisoner, () => pawn.IsSlave || pawn.IsPrisoner },
                { CGTrigger.Colonist, () => pawn.IsColonist },
                { CGTrigger.NonColonist, () => !pawn.IsColonist },
                { CGTrigger.Slave, () => pawn.IsSlave },
                { CGTrigger.Prisoner, () => pawn.IsPrisoner },
                { CGTrigger.OfColony, () => pawn.Faction == Faction.OfPlayerSilentFail },
                { CGTrigger.HasFaction, () => pawn.Faction != null },
                { CGTrigger.HasIdeo, () => pawn.Ideo != null },
                { CGTrigger.Dead, () => pawn.Dead },
                { CGTrigger.Rotted, () => pawn.GetRotStage().Equals(RotStage.Rotting) },
                { CGTrigger.Dessicated, () => pawn.GetRotStage().Equals(RotStage.Dessicated) },
                { CGTrigger.Downed, () => pawn.Downed },

                { CGTrigger.HostileToPlayer, () => pawn.HostileTo(Faction.OfPlayerSilentFail) },
                { CGTrigger.NeutralToPlayer, () => pawn.Faction?.RelationKindWith(Faction.OfPlayerSilentFail) == FactionRelationKind.Neutral },
                { CGTrigger.AlliedToPlayer, () => pawn.Faction?.RelationKindWith(Faction.OfPlayerSilentFail) == FactionRelationKind.Ally },

                { CGTrigger.RoyaltyDLC, () => ModsConfig.RoyaltyActive },
                { CGTrigger.Psycaster, () => pawn.GetPsylinkLevel() > 0 },

                { CGTrigger.IdeologyDLC, () => ModsConfig.IdeologyActive },

                { CGTrigger.BiotechDLC, () => ModsConfig.BiotechActive },
                { CGTrigger.Bloodfeeder, pawn.IsBloodfeeder},

                { CGTrigger.AnomalyDLC, () => ModsConfig.AnomalyActive },
                { CGTrigger.Mutant, () => pawn.IsMutant },
                { CGTrigger.Ghoul, () => pawn.IsGhoul },
            };

            foreach (var requirement in requirements)
            {
                if (requirementChecks.TryGetValue(requirement, out var check) && !check())
                {
                    return false;
                }
            }
            foreach (var taggedRequirement in tagRequirements)
            {
                if (!pawn.HasTagged(taggedRequirement))
                {
                    return false;
                }
            }
            if (ModsConfig.IdeologyActive && !requiredMemes.NullOrEmpty())
            {
                return false;
            }
            foreach (var memeRequirement in requiredMemes)
            {
                if (!pawn.Ideo?.memes.Contains(memeRequirement) ?? false)
                {
                    return false;
                }
            }

            return true;
        }

        public Shader ShaderFor(Pawn pawn)
        {
            if (shader?.Shader != null)
            {
                return shader.Shader;
            }

            if (useSkinShader)
            {
                Shader skinShader = ShaderUtility.GetSkinShader(pawn);
                if (skinShader != null)
                {
                    return skinShader;
                }
            }
            return ShaderTypeDefOf.CutoutComplex.Shader;
        }

        public string MaskPathFor(Pawn pawn, PawnRenderNode node)
        {
            if (!maskPaths.NullOrEmpty())
            {
                using (new RandBlock(TexSeedFor(pawn, node, useFactionRNGSeed)))
                {
                    return maskPaths.RandomElement();
                }
            }
            return maskPath;
        }

        public string TexPathFor(Pawn pawn, PawnRenderNode node)
        {
            if (bodyTypeGraphicPaths != null)
            {
                foreach (BodyTypeGraphicData bodyTypeGraphicPath in bodyTypeGraphicPaths)
                {
                    if (pawn.story.bodyType == bodyTypeGraphicPath.bodyType)
                    {
                        return bodyTypeGraphicPath.texturePath;
                    }
                }
            }
            if (pawn.gender == Gender.Female)
            {
                if (!texPathsFemale.NullOrEmpty())
                {
                    using (new RandBlock(TexSeedFor(pawn, node, useFactionRNGSeed)))
                    {
                        return texPathsFemale.RandomElement();
                    }
                }
                if (!texPathFemale.NullOrEmpty())
                {
                    return texPathFemale;
                }
            }
            if (!texPaths.NullOrEmpty())
            {
                using (new RandBlock(TexSeedFor(pawn, node, useFactionRNGSeed)))
                {
                    return texPaths.RandomElement();
                }
            }
            return texPath;
        }
        protected virtual int TexSeedFor(Pawn pawn, PawnRenderNode node, bool useFactionSeed)
        {
            if (useFactionSeed)
            {
                int factionTexSeed = 0;
                var faction = pawn.Faction;
                if (faction != null)
                {
                    factionTexSeed += faction.GetUniqueLoadID().GetHashCode();
                    if (faction.ideos?.PrimaryIdeo is Ideo ideo)
                    {
                        factionTexSeed += ideo.GetUniqueLoadID().GetHashCode();
                    }
                }
                return factionTexSeed;
            }
            int texSeed = node.Props.texSeed;
            texSeed += pawn.thingIDNumber;
            if (node.hediff != null)
            {
                texSeed += node.hediff.loadID;
            }
            if (node.apparel != null)
            {
                texSeed += node.apparel.thingIDNumber;
            }
            if (node.trait != null)
            {
                texSeed += node.trait.def.index;
            }
            if (node.gene != null)
            {
                texSeed += node.gene.loadID;
            }
            return texSeed;
        }
    }
}
