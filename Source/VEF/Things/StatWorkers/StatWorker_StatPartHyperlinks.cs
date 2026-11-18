using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VEF.Things;

// Only use for stats that have stat parts.
// Also use for stats that are used on non-pawns. Pawns already show those stats, nothing else does.
public class StatWorker_StatPartHyperlinks : StatWorker
{
    public override IEnumerable<Dialog_InfoCard.Hyperlink> GetInfoCardHyperlinks(StatRequest statRequest)
    {
        // Pawn already displays hyperlinks properly
        if (statRequest.Thing is Pawn)
        {
            foreach (var hyperlink in base.GetInfoCardHyperlinks(statRequest))
                yield return hyperlink;
            yield break;
        }

        if (stat.parts != null)
        {
            foreach (var part in stat.parts)
            {
                foreach (var hyperlink in part.GetInfoCardHyperlinks(statRequest))
                    yield return hyperlink;
            }
        }
    }
}