using System.Collections.Generic;
using Verse;
using RimWorld;

namespace VFECore
{
    public class QuestChainWorker
    {
        public QuestChainDef def;

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

        public virtual Pawn CreateAndStoreUniquePawn(PawnKindDef kind, UniqueCharacterExtension ext, Faction faction = null)
        {
            Pawn pawn = PawnGenerator.GeneratePawn(kind, faction);
            State.uniquePawnsByTag[ext.tag] = pawn;
            return pawn;
        }

        public virtual Pawn GetUniquePawn(string tag)
        {
            return State.GetUniquePawn(tag);
        }

        public virtual string GetDescription()
        {
            return def.description;
        }
    }
}