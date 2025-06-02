using System.Collections.Generic;
using Verse;
using RimWorld;
using System.Text.RegularExpressions;

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
            Faction faction = null;
            if (kind.defaultFactionType != null)
            {
                faction = Find.FactionManager.FirstFactionOfDef(kind.defaultFactionType);
            }
            Pawn pawn = PawnGenerator.GeneratePawn(kind, faction);
            Log.Message($"Creating unique pawn {pawn.Name} with faction {pawn.Faction?.def} for quest chain {def.defName} with tag {ext.tag}");
            State.StoreUniquePawn(ext.tag, pawn, deepSave: true);
            InvalidateDescriptionCache();
            return pawn;
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