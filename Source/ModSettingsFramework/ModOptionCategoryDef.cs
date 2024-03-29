using System.Collections.Generic;
using Verse;

namespace ModSettingsFramework
{
    public class ModOptionCategoryDef : Def
    {
        public string modSettingsName;
        public int order;
        public List<string> mods;
    }
}
