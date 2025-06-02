using RimWorld;
using System.Xml;
using Verse;

namespace VFECore
{
    public class ConditionSucceedQuestsCount
    {
        public QuestScriptDef questDef;
        public int count;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "questDef", xmlRoot);
            count = ParseHelper.FromString<int>(xmlRoot.FirstChild.Value);
        }
    }
}