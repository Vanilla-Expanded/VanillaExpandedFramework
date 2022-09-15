using System;
using RimWorld;

namespace ItemProcessor
{

    //A simple enum storing all the stages an item processor can be in. Much better than a bunch of booleans

    public enum ProcessorStage
    {
        Inactive,
        IngredientsChosen,
        AutoIngredients,
        ExpectingIngredients,
        AllIngredientReceived,
        Working,
        Finished,
        ProductRemoved,
        Invalid

    }
}