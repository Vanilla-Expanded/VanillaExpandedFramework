using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace KCSG
{
    public static class RandomUtils
    {
        /// <summary>
        /// Return random non-colony ennemy faction of *faction*. Fallback to random faction.
        /// </summary>
        /// <param name="faction"></param>
        /// <param name="allowHidden"></param>
        /// <param name="allowDefeated"></param>
        /// <param name="allowNonHumanlike"></param>
        /// <param name="minTechLevel"></param>
        /// <returns>Faction</returns>
        public static Faction RandomNonColonyEnnemy(this Faction faction, bool allowHidden = false, bool allowDefeated = false, bool allowNonHumanlike = true, TechLevel minTechLevel = TechLevel.Undefined)
        {
            var manager = Find.FactionManager;
            if (faction == null || manager == null)
                return null;

            var all = manager.GetFactions(allowHidden, allowDefeated, allowNonHumanlike, minTechLevel).ToList();
            if (all.NullOrEmpty())
                return null;

            var player = Faction.OfPlayer;
            var list = new List<Faction>();

            for (int i = 0; i < all.Count; i++)
            {
                var fac = all[i];
                if (fac != player && fac.HostileTo(faction))
                    list.Add(fac);
            }

            return list.RandomElement();
        }

        /// <summary>
        /// Return random non colony faction following parameters
        /// </summary>
        /// <param name="allowHidden"></param>
        /// <param name="allowDefeated"></param>
        /// <param name="allowNonHumanlike"></param>
        /// <param name="minTechLevel"></param>
        /// <returns>Faction</returns>
        public static Faction RandomNonColonyFaction(bool allowHidden = false, bool allowDefeated = false, bool allowNonHumanlike = true, TechLevel minTechLevel = TechLevel.Undefined)
        {
            var manager = Find.FactionManager;
            if (manager == null)
                return null;

            var all = manager.GetFactions(allowHidden, allowDefeated, allowNonHumanlike, minTechLevel).ToList();
            if (all.NullOrEmpty())
                return null;

            var player = Faction.OfPlayer;
            var list = new List<Faction>();

            for (int i = 0; i < all.Count; i++)
            {
                var fac = all[i];
                if (fac != player)
                    list.Add(fac);
            }

            return list.RandomElement();
        }

        /// <summary>
        /// Get random stuff for thingDef
        /// </summary>
        public static ThingDef RandomWallStuffWeighted(ThingDef thingDef)
        {
            if (GenOption.StuffableOptions is StuffableOptions option)
            {
                if (option.allowedWallStuff.Count > 0)
                {
                    return RandomStuffFromFor(option.allowedWallStuff, thingDef);
                }

                if (option.disallowedWallStuff.Count > 0)
                {
                    var from = StartupActions.stuffs.FindAll(t => !option.disallowedWallStuff.Contains(t));
                    return RandomStuffFromFor(from, thingDef);
                }
            }

            return RandomStuffFromFor(StartupActions.stuffs, thingDef);
        }

        /// <summary>
        /// Get random stuff for wall, from symbol
        /// </summary>
        public static ThingDef RandomWallStuffWeighted(SymbolDef symbol)
        {
            if (symbol.thingDef.MadeFromStuff && GenOption.StuffableOptions is StuffableOptions option && option.randomizeWall)
                return RandomWallStuffWeighted(symbol.thingDef);

            return symbol.stuffDef;
        }

        /// <summary>
        /// Get random stuff for furniture
        /// </summary>
        public static ThingDef RandomFurnitureStuffWeighted(SymbolDef symbol)
        {
            if (!symbol.thingDef.MadeFromStuff)
                return null;

            var option = GenOption.StuffableOptions;

            if (option != null && option.randomizeFurniture && !option.excludedFunitureDefs.Contains(symbol.thingDef))
            {
                if (option.allowedFurnitureStuff.Count > 0)
                {
                    return RandomStuffFromFor(option.allowedFurnitureStuff, symbol.thingDef);
                }

                if (option.disallowedFurnitureStuff.Count > 0)
                {
                    var from = StartupActions.stuffs.FindAll(t => !option.disallowedFurnitureStuff.Contains(t));
                    return RandomStuffFromFor(from, symbol.thingDef);
                }

                return RandomStuffFromFor(StartupActions.stuffs, symbol.thingDef);
            }

            if (symbol.randomizeStuff)
                return RandomStuffFromFor(StartupActions.stuffs, symbol.thingDef);

            return symbol.stuffDef ?? symbol.thingDef.defaultStuff ?? ThingDefOf.WoodLog;
        }

        /// <summary>
        /// Get stuff by commonality from list, matching thingDef requirement
        /// </summary>
        private static ThingDef RandomStuffFromFor(List<ThingDef> from, ThingDef thingDef)
        {
            return from.FindAll(t => thingDef.stuffCategories.Find(c => t.stuffProps.categories.Contains(c)) != null)
                .RandomElementByWeight(t => t.stuffProps.commonality);
        }

        /// <summary>
        /// Choose a random layout that match requirement(s) from a list.
        /// </summary>
        public static StructureLayoutDef RandomLayoutFrom(List<StructureLayoutDef> list)
        {
            var choices = new List<StructureLayoutDef>();
            for (int i = 0; i < list.Count; i++)
            {
                var layout = list[i];
                if (layout.RequiredModLoaded)
                {
                    choices.Add(layout);
                }
            }
            return choices.RandomElement();
        }

        /// <summary>
        /// Choose a random structure from a list.
        /// </summary>
        public static StructureLayoutDef RandomLayoutFrom(List<LayoutCommonality> list, IncidentParms parms)
        {
            var choices = new List<LayoutCommonality>();
            for (int i = 0; i < list.Count; i++)
            {
                var lComm = list[i];
                if (lComm.layout.RequiredModLoaded)
                    choices.Add(lComm);
            }
            return choices.RandomElementByWeight(l => l.commonality * parms.points).layout;
        }
    }
}