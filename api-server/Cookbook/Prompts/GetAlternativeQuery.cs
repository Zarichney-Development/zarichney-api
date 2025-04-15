using System.Text.Json;
using Zarichney.Services.AI;

namespace Zarichney.Cookbook.Prompts;

/// <summary>
/// Prompt for generating alternative, more generic search queries when initial recipe name searches fail.
/// </summary>
public class GetAlternativeQueryPrompt : PromptBase
{
    public override string Name => "Alternative Query Generator";
    public override string Description => "Generates a more generic search query based on an original recipe name and previous failed attempts.";
    public override string Model => LlmModels.Gpt4Omini;

    public override string SystemPrompt =>
        """
        You are an assistant that helps refine recipe search queries.
        The user requested a recipe named '{originalRecipeName}' but initial searches using that name and the following previous attempts failed: {previousAttempts}.
        Your task is to generate ONE new, more generic search query that is likely to find relevant recipes for the original request.
        Focus on the core components or type of the dish. Remove specific adjectives, cooking methods, or brand names unless essential.
        Make the query broader than the previous attempts.

        Example 1:
        Original: "Grandma's Best Old-Fashioned Apple Pie"
        Previous: ["Grandma's Best Old-Fashioned Apple Pie"]
        Response: Apple Pie

        Example 2:
        Original: "Quick 30-Minute One-Pan Lemon Garlic Shrimp Pasta"
        Previous: ["Quick 30-Minute One-Pan Lemon Garlic Shrimp Pasta", "One-Pan Lemon Garlic Shrimp Pasta"]
        Response: Lemon Garlic Shrimp Pasta

        Example 3:
        Original: "Luigi's Veggie Power-Up Pizza"
        Previous: ["Luigi's Veggie Power-Up Pizza"]
        Response: Vegetable Pizza
        """;

    /// <summary>
    /// Generates a user prompt for requesting a more generic search query.
    /// </summary>
    /// <param name="originalRecipeName">The original recipe name that failed to find results</param>
    /// <param name="previousAttempts">List of previous search queries that also failed</param>
    /// <returns>A formatted user prompt for the LLM</returns>
    public string GetUserPrompt(string originalRecipeName, List<string> previousAttempts)
    {
        var previousAttemptsText = previousAttempts.Count > 0
            ? $"Previous attempts were: '{string.Join("', '", previousAttempts)}'"
            : "This is the first attempt.";
        return $"Original recipe name: '{originalRecipeName}'. {previousAttemptsText}. Please provide a more generic search query.";
    }

    public override FunctionDefinition GetFunction() => new()
    {
        Name = "GenerateAlternativeQuery",
        Description = "Generates a single, more generic search query based on a failed original query and previous attempts.",
        Strict = true,
        Parameters = JsonSerializer.Serialize(new
        {
            type = "object",
            properties = new
            {
                newQuery = new
                {
                    type = "string",
                    description = "The suggested alternative search query."
                }
            },
            required = new[] { "newQuery" }
        })
    };
}

/// <summary>
/// Structured result returned by the GenerateAlternativeQuery function
/// </summary>
public record AlternativeQueryResult
{
    public required string NewQuery { get; init; }
}