using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using Verse;

namespace VFECore
{
    public interface ITaggedItem
    {
        string Tag { get; }
    }
    public abstract class TaggedItem<T> : IExposable, IEquatable<TaggedItem<T>>, ITaggedItem
    {
        public string tag;
        public T value;
        public string Tag => tag;

        public TaggedItem() { }

        public TaggedItem(string tag, T value)
        {
            this.tag = tag;
            this.value = value;
        }

        public bool Equals(TaggedItem<T> other)
        {
            return tag == other.tag && EqualityComparer<T>.Default.Equals(value, other.value);
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref tag, "tag");
            Scribe_Values.Look(ref value, "value");
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            tag = xmlRoot.Name;
            value = ParseHelper.FromString<T>(xmlRoot.InnerText);
        }

        public string GetUniqueLoadID()
        {
            return tag + value.GetHashCode();
        }

        public override string ToString()
        {
            return $"TaggedItem<{typeof(T)}> {tag}: {value}";
        }
    }

    public class TaggedColor : TaggedItem<Color>
    {
        public TaggedColor() { }
        public TaggedColor(string tag, Color value) : base(tag, value) { }
    }
    public class TaggedText : TaggedItem<string>
    {
        public TaggedText() { }
        public TaggedText(string tag, string value) : base(tag, value) { }
    }

    public class TaggedAdvancedColor : TaggedItem<AdvancedColor>
    {
        public TaggedAdvancedColor() { }
        public TaggedAdvancedColor(string tag, AdvancedColor value) : base(tag, value) { }
    }

}
