using System;
using System.Collections.Generic;
using VEF.Things;

namespace VEF.Cooking
{
    [Obsolete(ObsoleteError)]
    public class Recipe_Extension : RecipeExtension
    {
        private const string ObsoleteError = "VEF.Cooking.Recipe_Extension is obsolete, use VEF.Things.RecipeExtension instead";

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var configError in base.ConfigErrors())
                yield return configError;

            // Remove the comment in a while, once we're reading to remove this DefModExtension.
            // It seems like it's only used in VE anyway (nothing on GitHub seems to be using this),
            // so we should be fine with a minimal warning before removing this.
            // yield return ObsoleteError;
        }
    }
}