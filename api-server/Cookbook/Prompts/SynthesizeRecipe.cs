using System.Text.Json;
using AutoMapper;
using Zarichney.Cookbook.Orders;
using Zarichney.Cookbook.Recipes;
using Zarichney.Services.AI;

namespace Zarichney.Cookbook.Prompts;

public class SynthesizeRecipePrompt(IMapper mapper) : PromptBase
{
  public override string Name => "Recipe Maker";
  public override string Description => "Synthesize a new recipe based on existing recipes and user preferences";
  public override string Model => LlmModels.Gpt4Omini;

  public override string SystemPrompt =>
    """
    # Recipe Synthesizer System Prompt
    **Role:** Customized recipe generation.
    **Steps:**
    1. **Analyze Cookbook Order:**
       - Review Cookbook Content, Details, and User Details.
       - Focus on dietary restrictions, allergies, skill level, and cooking goals.
    2. **Review Provided Recipes:**
       - You will be provided with a list of recipes web sourced from various recipe websites.
       - Use these recipes as a basis for inspiration for the new synthesized recipe, mixing and matching elements.
    3. **Create New Recipe:**
       - Blend elements from relevant recipes to form the desired recipe name.
       - Ensure it meets dietary needs, preferences, skill level, and time constraints.
       - Align with the cookbook's theme and requirements, meeting the user's expectations of customization.
    4. **Customize Recipe:**
       - Scale the ingredients to adjust for the desired serving size. If a recipe serves 2 and the user wants 4 servings, double the ingredients.
       - Include alternatives or substitutions when the provided recipes are in conflict of the user's dietary restrictions.
       - IMPORTANT: Always alter the synthesized recipe given the user's allergies.
    5. **Enhance Recipe:**
       - Add cultural context or storytelling.
       - Incorporate educational content.
       - Include meal prep tips or leftover ideas.
       - Scale the amount of detail according to how specific the user's cookbook expectations are. Keep it concise for simple orders.
       - Do not add factual information such as nutritional data or facts.
    6. **Format Output:**
       - Use provided Recipe class structure.
       - Fill out all fields.
       - Don't include the word "Step" in the enumerated directions.
       - Omit a conclusion.
       - When applicable, include customizations, cultural context, or educational content in Notes (uses the subheader ##). Ensure additional subheaders uses ### or ####.
    7. **Provide Grounding:**
       - List "Inspired by" URLs from the sourced recipes.
       - Include at least the most relevant one, but only include those that contributed towards the synthesized recipe.
    8. **Review and Refine:**
       - The synthesized recipe will be assess for quality assurance.
       - Provide a new revision when provided suggestions for improvement.

    **Goal:** Tailor recipes to user needs and preferences while maintaining sourced recipes integrity and cookbook theme.
    """;

  public string GetUserPrompt(string recipeName, List<Recipe> recipes, CookbookOrder order) =>
    $"""
     Cookbook Order:
     ```md
     {order.ToMarkdown()}
     ```

     Recipe data:
     ```json
     {JsonSerializer.Serialize(mapper.Map<List<ScrapedRecipe>>(recipes))}
     ```

     Please synthesize a personalized recipe for '{recipeName}'. Thank you.
     """;

  public override FunctionDefinition GetFunction() => new()
  {
    Name = "SynthesizeRecipe",
    Description = "Synthesize a personalized recipe using existing recipes and user's cookbook order",
    Strict = true,
    Parameters = JsonSerializer.Serialize(new
    {
      type = "object",
      properties = new
      {
        title = new { type = "string", description = "The requested recipe name." },
        description = new { type = "string", description = "A preface to the given recipe." },
        // TODO: Enhance these with descriptions
        servings = new { type = "string" },
        prepTime = new { type = "string" },
        cookTime = new { type = "string" },
        totalTime = new { type = "string" },
        ingredients = new { type = "array", items = new { type = "string" } },
        directions = new { type = "array", items = new { type = "string" } },
        inspiredBy = new { type = "array", items = new { type = "string" } },
        notes = new { type = "string" },
      },
      required = new[]
      {
        "title", "description", "servings", "prepTime", "cookTime", "totalTime",
        "ingredients", "directions", "inspiredBy", "notes"
      }
    })
  };
}
