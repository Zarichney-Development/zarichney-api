# Module/Directory: /Cookbook/Prompts

**Last Updated:** 2025-04-13

> **Parent:** [`/Cookbook`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** This directory contains specific prompt definitions used to interact with the AI Language Models (LLMs) for tasks related to the Cookbook module. Each class encapsulates the instructions, input formatting, and expected output structure for a distinct AI operation.
* **Key Responsibilities:**
    * Defining system prompts (instructions) for various AI tasks like cleaning recipe data (`CleanRecipePrompt`), ranking recipe relevance (`RankRecipePrompt`), choosing relevant URLs (`ChooseRecipesPrompt`), generating recipe index titles/aliases (`RecipeNamerPrompt`), synthesizing new recipes (`SynthesizeRecipePrompt`), analyzing synthesized recipes (`AnalyzeRecipePrompt`), generating alternative search queries (`GetAlternativeQueryPrompt` - which returns a structured `AlternativeQueryResult` with a `NewQuery` property), and generating initial recipe lists for orders (`ProcessOrderPrompt`). [cite: Zarichney.Server/Cookbook/Prompts/CleanRecipe.cs, Zarichney.Server/Cookbook/Prompts/RankRecipe.cs, Zarichney.Server/Cookbook/Prompts/ChooseRecipes.cs, Zarichney.Server/Cookbook/Prompts/RecipeNamer.cs, Zarichney.Server/Cookbook/Prompts/SynthesizeRecipe.cs, Zarichney.Server/Cookbook/Prompts/AnalyzeRecipe.cs, Zarichney.Server/Cookbook/Prompts/GetAlternativeQuery.cs, Zarichney.Server/Cookbook/Prompts/ProcessOrder.cs]
    * Providing methods (`GetUserPrompt`) to format application data (like `Recipe` or `CookbookOrder` objects) into the text input expected by the AI model for a specific task.
    * Defining the expected JSON output structure using function definitions (`GetFunction`) for tasks requiring structured responses from the AI (leveraging OpenAI's function calling feature). [cite: Zarichney.Server/Config/PromptBase.cs]
* **Why it exists:** To centralize and manage the complex prompts required for AI interactions, separating them from the core service logic. This makes prompts easier to read, modify, test, and maintain, especially as AI interactions become more sophisticated.

## 2. Architecture & Key Concepts

* **Base Class:** All prompt definitions inherit from the abstract `PromptBase` class defined in [`/Config`](../../Config/README.md). [cite: Zarichney.Server/Config/PromptBase.cs]
* **Structure per Prompt:** Each `.cs` file typically defines a single class (e.g., `SynthesizeRecipePrompt`) responsible for one specific AI interaction type. This class includes:
    * `SystemPrompt`: A string containing detailed instructions for the AI model. Often uses XML-like tags for better structure and readability by the AI. [cite: Zarichney.Server/Cookbook/Prompts/SynthesizeRecipe.cs]
    * `GetUserPrompt(...)`: A method that takes relevant application data (e.g., a `Recipe` object, a query string) and formats it into the user message part of the prompt sent to the AI.
    * `GetFunction()`: A method that returns a `FunctionDefinition` object, containing a name, description, and a JSON schema string defining the expected structured output (used for OpenAI function calling). [cite: Zarichney.Server/Cookbook/Prompts/SynthesizeRecipe.cs, Zarichney.Server/Config/PromptBase.cs]
    * `Name`, `Description`, `Model`: Properties identifying the prompt and specifying the target AI model (e.g., `LlmModels.Gpt4Omini`). [cite: Zarichney.Server/Cookbook/Prompts/SynthesizeRecipe.cs, Zarichney.Server/Services/AI/LlmService.cs]
* **Consumption:** These prompt classes are typically registered as singletons via DI (using `AddPrompts` extension method) and injected into services (like `RecipeService`, `RecipeRepository`, `WebScraperService`) that need to interact with the AI via `ILlmService`. [cite: Zarichney.Server/Program.cs, Zarichney.Server/Config/ConfigurationExtensions.cs, Zarichney.Server/Services/AI/LlmService.cs]

## 3. Interface Contract & Assumptions

* **Interface:** The public contract is defined by the `PromptBase` abstract class and the specific implementations within this directory. Services consuming these prompts rely on the `SystemPrompt`, `GetUserPrompt`, and `GetFunction` members to interact correctly with `ILlmService`.
* **Assumptions:**
    * **AI Model Compatibility:** Assumes the specified AI model (e.g., `LlmModels.Gpt4Omini`) can understand the `SystemPrompt` instructions and generate responses compatible with the schema defined in `GetFunction` (when function calling is used).
    * **Input Data:** Assumes the data passed into `GetUserPrompt` is valid and sufficient for the AI to perform its task.
    * **`ILlmService`:** Assumes the `ILlmService` implementation correctly utilizes the properties of the prompt object (SystemPrompt, UserPrompt, FunctionDefinition) when making calls to the AI API. [cite: Zarichney.Server/Services/AI/LlmService.cs]
    * **JSON Schema Validity:** Assumes the JSON schema defined in `GetFunction` is valid and accurately represents the desired output structure.

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Naming:** Prompt classes are named descriptively based on the AI task they perform (e.g., `AnalyzeRecipePrompt`).
* **Structure:** Adherence to the `PromptBase` inheritance pattern.
* **Function Definition:** Use of `System.Text.Json` for serializing the JSON schema within `GetFunction`. [cite: Zarichney.Server/Cookbook/Prompts/SynthesizeRecipe.cs]
* **System Prompt Formatting:** Often uses multi-line raw string literals (`"""..."""`) and sometimes XML-like tags for structuring complex instructions. [cite: Zarichney.Server/Cookbook/Prompts/SynthesizeRecipe.cs, Zarichney.Server/Cookbook/Prompts/ProcessOrder.cs]

## 5. How to Work With This Code

* **Adding a New AI Interaction:**
    1. Create a new class inheriting from `PromptBase` in this directory.
    2. Define the `Name`, `Description`, and target `Model`.
    3. Write the detailed instructions in the `SystemPrompt` property.
    4. Implement the `GetUserPrompt(...)` method to format input data.
    5. Implement the `GetFunction()` method, defining the desired JSON output schema if structured output is needed.
    6. The prompt will be automatically registered via `AddPrompts` in `Program.cs`. [cite: Zarichney.Server/Program.cs, Zarichney.Server/Config/ConfigurationExtensions.cs]
    7. Inject the new prompt class into the service that needs it and use it with `ILlmService`.
* **Modifying AI Behavior:** Edit the `SystemPrompt` for instruction changes or the `GetFunction` JSON schema for output structure changes within the relevant prompt class file.
* **Testing:** Testing prompts often involves:
    * Unit testing the `GetUserPrompt` method to ensure correct formatting.
    * Integration testing by injecting the prompt into a service, mocking `ILlmService` to simulate AI responses (including expected JSON structure for function calls), and verifying the service logic handles the simulated response correctly.
    * Direct testing against the actual AI model (can be slow/expensive) during development to refine prompts.
* **Common Pitfalls / Gotchas:** AI model not consistently adhering to the function schema. System prompts becoming too complex or ambiguous. Changes in AI model versions requiring prompt adjustments ("prompt drift"). JSON schema errors in `GetFunction`.

## 6. Dependencies

* **Internal Code Dependencies:**
    * [`/Config`](../../Config/README.md) - Relies on `PromptBase` and `FunctionDefinition`.
    * [`/Cookbook/Recipes`](../Recipes/README.md) - Uses models like `Recipe`, `SynthesizedRecipe` in `GetUserPrompt` methods.
    * [`/Cookbook/Orders`](../Orders/README.md) - Uses models like `CookbookOrder` in `GetUserPrompt` methods.
* **External Library Dependencies:**
    * `System.Text.Json`: Used for defining function schemas.
* **Dependents (Impact of Changes):**
    * [`/Services/AI/LlmService.cs`](../../Services/AI/README.md) - Directly consumes these prompt objects to interact with the AI API.
    * Various Services in [`/Cookbook`](../README.md) (e.g., `RecipeService`, `RecipeRepository`, `OrderService`, `WebScraperService`) - Inject and use specific prompts via `ILlmService`. Changes to prompts (especially function schemas) require validation in consuming services.

## 7. Rationale & Key Historical Context

* **Prompt Encapsulation:** Separating prompt logic into dedicated classes improves organization and makes it easier to manage complex AI instructions compared to embedding them directly within service methods.
* **Structured Output:** Using function calling (`GetFunction` with JSON schema) ensures the AI provides data in a predictable format that can be easily deserialized and used by the application, reducing reliance on fragile text parsing.
* **Maintainability:** Centralizing prompts allows for easier updates and adjustments as AI models evolve or requirements change.

## 8. Known Issues & TODOs

* Prompts may require tuning and refinement based on the performance and consistency of the target AI models (`gpt-4o-mini`).
* Robust error handling for cases where the AI fails to adhere to the function schema (despite `Strict = true`) might be needed in `LlmService`.
* Consider adding versioning to prompts if significant changes are made over time.