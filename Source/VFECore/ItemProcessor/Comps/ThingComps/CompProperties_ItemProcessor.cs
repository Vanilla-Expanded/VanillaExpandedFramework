
using Verse;
using System.Collections.Generic;

namespace ItemProcessor
{
    public class CompProperties_ItemProcessor : CompProperties
    {
        //Buildings can be set to a number between 1 and 4. The code ignores values outside this.
        //The process also needs a CombinationDef in XML
        //In both cases, there is a ItemAcceptedDef to define what ingredients are accepted by what building
        public int numberOfInputs = 1;

        //This makes sense for 2 to 4 inputs, so some of them can be empty in CombinationDefs
        public bool acceptsNoneAsInput = false;

        //Does the resulting product use quality levels?
        public bool qualitySelector = false;

        //Can this building automatically grab ingredients from nearby hoppers (aka auto mode)?
        public bool isAutoMachine = false;

        //Is this building 100% automated, and pawns can't even bring things to it?
        public bool isCompletelyAutoMachine = false;

        //Semi automatic machines don't auto grab ingredients, but ingredient only needs to be set once, and pawns will instantly bring them in without
        //a "bring ingredients" order. The machine also won't "forget" the set ingredient.
        public bool isSemiAutomaticMachine = false;

        //Can this building only be loaded/unloaded from its interaction spot?
        //Separate fields for loading/unloading offers flexibility
        //(e.g. a giant funnel-like building that can be loaded from any nearby cell, but only unloaded from a specific cell)
        public bool mustLoadFromInteractionSpot = false;
        public bool mustUnloadFromInteractionSpot = false;

        //Can this building automatically drop products on its interaction spot?
        public bool isAutoDropper = false;

        //Does this building ignore its ingredients? (this has no effect if the resulting product doesn't have ingredients anyway)
        public bool ignoresIngredientLists = false;

        //Does this building accept category recipes?
        public bool isCategoryBuilding = false;

        //Does this building have the player specify output instead of input?
        public bool isMachineSpecifiesOutput = false;

        //Can this processor be "paused"?
        public bool isMachinePausable = false;

        //Transfer ingredients: instead of adding the inputs as ingredients, it adds the inputs' ingredients as ingredients. Yeah, confusing...
        //Example, if false, and input = Milk and output = Cheese, Cheese will have Milk as an ingredient
        //If true, and input = Uncooked Soup and output = Cooked Soup, Cooked Soup will have the same ingredients list as Uncooked Soup (for example human meat and corn)
        public bool transfersIngredientLists = false;

        //Custom descriptions for ingredient insertion buttons
        public string InsertFirstItemDesc = "IP_InsertFirstItemDesc";
        public string InsertSecondItemDesc = "IP_InsertSecondItemDesc";
        public string InsertThirdItemDesc = "IP_InsertThirdItemDesc";
        public string InsertFourthItemDesc = "IP_InsertFourthItemDesc";

        //Custom image for choosing a new ingredient
        public string chooseIngredientsIcon = "UI/IP_NoIngredient";

        //Custom descriptions and button image for bringing ingredients to the building
        public string bringIngredientsText = "IP_BringIngredients";
        public string bringIngredientsDesc = "IP_BringIngredientsDesc";
        public string bringIngredientsIcon = "UI/IP_InsertIngredients";

        //Custom descriptions and button image for cancelling bringing ingredients to the building
        public string cancelIngredientsText = "IP_CancelIngredients";
        public string cancelIngredientsDesc = "IP_CancelIngredientsDesc";
        public string cancelIngredientsIcon = "UI/IP_CancelIngredients";

        //Custom descriptions and button image for resetting auto in semiautomatic machines
        public string resetSemiautomaticText = "IP_ResetSemiautomatic";
        public string resetSemiautomaticDesc = "IP_ResetSemiautomaticDesc";
        public string resetSemiautomaticIcon = "UI/IP_SemiautomaticReset";

        //Does lack of power or fuel destroy the production progress?
        public bool noPowerDestroysProgress = false;
        //If yes, on how many rare ticks (1 rare tick = aprox 4 seconds)?
        public int rareTicksToDestroy = 10;
        //Custom warning to pop up once if power/fuel is interrupted
        public string noPowerDestroysInitialWarning = "IP_NoPowerDestroysInitialWarning";
        //Custom message to pop up if production is interrupted
        public string noPowerDestroysMessage = "IP_NoPowerDestroysMessage";

        //Custom descriptions and button image for colonist removing the product from the building
        public string removeProductText = "IP_RemoveProduct";
        public string removeProductDesc = "IP_RemoveProductDesc";
        public string removeProductIcon = "UI/IP_ExtractProduct";

        //Does the building destroy its internally stored ingredients at the very start of the process,
        //or only when the product has reached awful quality?
        //This mainly affects what happens when the building is destroyed, since it will pop up its
        //ingredients if they haven't been destroyed
        public bool destroyIngredientsAtStartOfProcess = false;
        public bool destroyIngredientsAtAwfulQuality = false;

        //Graphic of the building when it is working.
        public string buildingOnGraphic ="";
        //Graphic of the building when it is finished.
        public string buildingFinishedGraphic ="";
        //Shader to be used for working image
        public ShaderTypeDef shaderForBuildingOnGraphic = null;
        //Shader to be used for finished image
        public ShaderTypeDef shaderForBuildingFinishedGraphic = null;

        //This defines whether the building is affected by bright light or lack of light
        public bool isLightDependingMachine = false;
        public float maxLight = 1f;
        public float minLight = 0f;
        public string messageIfOutsideLightRangesWarning = "IP_OutsideLightRange";
        public string messageIfOutsideLightRanges = "IP_SpoiledDueToLight";
        public int rareTicksToDestroyDueToLight = 30;

        //This defines whether the building is affected by rain
        public bool isRainDependingMachine = false;
        public string messageIfRainWarning = "IP_ItRains";
        public string messageIfRain = "IP_SpoiledDueToRain";
        public int rareTicksToDestroyDueToRain = 30;

        //This defines whether the building is affected by temperature
        public bool isTemperatureDependingMachine = false;
        public float maxTemp = 1f;
        public float minTemp = 0f;
        public string messageIfWrongTempWarning = "IP_WrongTemp";
        public string messageIfWrongTemp = "IP_SpoiledDueToWrongTemp";
        public int rareTicksToDestroyDueToWrongTemp = 30;

        //This defines whether the building goes faster if temperature is in a given range
        public bool isTemperatureAcceleratingMachine = false;
        public float maxAccelerationTemp = 1f;
        public float minAccelerationTemp = 0f;
        public float accelerationFactor = 1f;

        //This defines whether the building shows a fermenting progress bar like base game's
        public bool showProgressBar = false;
        //This defines whether the building shows a simple progress bar that goes from red to green
        public bool showFactoryProgressBar = false;

        //Full auto machines specify input slots for their hoppers
        public List<IntVec3> inputSlots = null;


        public CompProperties_ItemProcessor()
        {
            this.compClass = typeof(CompItemProcessor);
        }


    }
}
