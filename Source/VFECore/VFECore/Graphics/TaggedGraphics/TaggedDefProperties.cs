using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFECore
{
    public class TaggedDefProperties : DefModExtension
    {
        public enum RNGSource
        {
            Pawn, Faction, Ideo, PawnKind, FactionDef
        }
        public List<TaggedAdvancedColor> generateAdvancedColors = [];
        public List<List<TaggedText>> generateRandomStrings = [];

        public List<TaggedColor> taggedColors = [];
        public List<TaggedText> taggedStrings = [];

        public override IEnumerable<string> ConfigErrors()
        {
            // Check so no tags or their entries are null.
            if (taggedColors.Any(tc => tc.tag.NullOrEmpty() || tc.value == null))
                yield return "TaggedColor has null or empty tag or value.";
            if (taggedStrings.Any(tp => tp.tag.NullOrEmpty() || tp.value.NullOrEmpty()))
                yield return "TaggedPath has null or empty tag or value.";
        }

        public List<T> GetTaggedItems<T>() where T : ITaggedItem
        {
            if (typeof(T) == typeof(TaggedColor))
            {
                return taggedColors as List<T>;
            }
            else if (typeof(T) == typeof(TaggedText))
            {
                return taggedStrings as List<T>;
            }
            else
                throw new NotImplementedException($"Attempted to fetch tagged item for unsuported type: {typeof(T)}");
        }

        public void GenerateTags(Faction faction)
        {
            foreach (var taggedColor in generateAdvancedColors)
            {
                var color = taggedColor.value.GetColor(faction);
                faction.SetColorTag(taggedColor.tag, color);
            }
            foreach (var taggedString in generateRandomStrings)
            {
                var text = taggedString.RandomElement().value;
                faction.SetStringTag(taggedString.First().tag, text);
            }
        }

        public void GenerateTags(Pawn pawn)
        {
            foreach (var taggedColor in generateAdvancedColors)
            {
                var color = taggedColor.value.GetColor(pawn);
                pawn.SetColorTag(taggedColor.tag, color);
            }
            foreach (var taggedString in generateRandomStrings)
            {
                var text = taggedString.RandomElement().value;
                pawn.SetStringTag(taggedString.First().tag, text);
            }
        }
    }
}
