using Verse;

namespace VEF.Things;

public class RecipeExtension : DefModExtension
{
    public bool individualIngredients = false;

    // If a thing is made from stone chunks, the stone chunks will be used as stuff with identical stats as their equivalent bricks
    public bool chunksAsStuff = false;
}