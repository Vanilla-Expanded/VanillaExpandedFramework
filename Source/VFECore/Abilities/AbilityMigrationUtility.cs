using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using HarmonyLib;
using Verse;

namespace VFECore.Abilities
{
    [StaticConstructorOnStartup]
    public static class AbilityMigrationUtility
    {
        private static Dictionary<string, Type> abilityClasses = new();
        static AbilityMigrationUtility()
        {
            var conversionChain = (List<BackCompatibilityConverter>) AccessTools.Field(typeof(BackCompatibility), "conversionChain").GetValue(null);
            conversionChain.Add(new BackCompatabilityConverter_Abilities());
            foreach (var def in DefDatabase<AbilityDef>.AllDefs)
            {
                foreach (var extension in def.modExtensions.OfType<AbilityExtension_ClassMigration>())
                {
                    abilityClasses.Add(extension.oldClass, def.abilityClass);
                }
            }
        }

        public class BackCompatabilityConverter_Abilities : BackCompatibilityConverter
        {
            public override bool AppliesToVersion(int majorVer, int minorVer) => true;

            public override string BackCompatibleDefName(Type defType, string defName, bool forDefInjections = false, XmlNode node = null) => null;

            public override Type GetBackCompatibleType(Type baseType, string providedClassName, XmlNode node)
            {
                if (baseType == typeof(Ability) && abilityClasses.TryGetValue(providedClassName, out var newType)) return newType;
                return null;
            }

            public override void PostExposeData(object obj)
            {
            }
        }
    }

    public class AbilityExtension_ClassMigration : DefModExtension
    {
        public string oldClass;
    }
}
