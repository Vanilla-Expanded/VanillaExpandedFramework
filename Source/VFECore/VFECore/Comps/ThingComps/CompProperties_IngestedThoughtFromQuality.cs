using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace VFECore
{
    public class CompIngestedThoughtFromQuality : ThingComp
    {
        private CompProperties_IngestedThoughtFromQuality Props => (CompProperties_IngestedThoughtFromQuality)props;

        public override void PostIngested(Pawn ingester)
        {
            base.PostIngested(ingester);
            if (ingester.needs.mood != null)
            {
                var memories = ingester.needs.mood.thoughts.memories;
                var curIngestedMemory = memories.GetFirstMemoryOfDef(Props.ingestedThought);
                int quality = (int)parent.GetComp<CompQuality>().Quality;

                // Modify the existing memory if it exists
                if (curIngestedMemory != null)
                {
                    float averageIndex = (float)(curIngestedMemory.CurStageIndex + quality) / 2;
                    curIngestedMemory.SetForcedStage((quality > curIngestedMemory.CurStageIndex) ? Mathf.RoundToInt(averageIndex) : Mathf.FloorToInt(averageIndex));
                    curIngestedMemory.Renew();
                }

                // Otherwise create a new one
                else
                {
                    var ingestedMemory = ThoughtMaker.MakeThought(Props.ingestedThought, quality);
                    ingester.needs.mood.thoughts.memories.TryGainMemory(ingestedMemory);
                }
            }
        }
    }

    public class CompProperties_IngestedThoughtFromQuality : CompProperties
    {
        public ThoughtDef ingestedThought;

        public CompProperties_IngestedThoughtFromQuality()
        {
            compClass = typeof(CompIngestedThoughtFromQuality);
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            // Parent def does not have CompQuality
            if (!parentDef.HasComp(typeof(CompQuality)))
                yield return $"{parentDef} does not have CompQuality but is using CompProperties_IngestedThoughtFromQuality.";

            // Ingested thought is not a memory
            if (!ingestedThought.IsMemory)
                yield return $"{parentDef} CompProperties_IngestedThoughtFromQuality {ingestedThought}'s thoughtClass is not a Thought_Memory.";
        }
    }
}