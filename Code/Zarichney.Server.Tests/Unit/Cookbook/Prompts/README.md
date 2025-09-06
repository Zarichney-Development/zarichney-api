# Module/Directory: /Unit/Cookbook/Prompts

**Last Updated:** 2025-04-18

> **Parent:** [`Cookbook`](../README.md)
> *(Note: A README for `/Unit/Cookbook/` may be needed)*
> **Related:**
> * **Source:** [`Cookbook/Prompts/`](../../../../Zarichney.Server/Cookbook/Prompts/) (Contains specific prompt classes like `AnalyzeRecipe`, `SynthesizeRecipe`, etc.)
> * **Base Class:** [`Services/AI/PromptBase.cs`](../../../../Zarichney.Server/Services/AI/PromptBase.cs)
> * **Dependencies:** Usually none directly, but they operate on input models (e.g., `Recipe`, `Order`).
> * **Standards:** [`TestingStandards.md`](../../../../Docs/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../Docs/Standards/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the individual **prompt classes** (inheriting from `PromptBase`) used within the Cookbook feature. These classes encapsulate the specific instructions, formatting, input data handling, and potentially output parsing logic required for distinct interactions with the `LlmService`.

* **Why Unit Tests?** To validate that each prompt class correctly constructs the final prompt text or structured input (e.g., for function calling) that will be sent to the AI model. Tests ensure that input data (like `Recipe` objects, user queries, or order details) is accurately formatted and integrated into the prompt according to the class's specific template and logic, all in isolation from the `LlmService` itself.
* **Isolation:** Achieved by directly instantiating the specific prompt class under test and providing it with sample input data. Mocking is typically not required unless a prompt class has unusual external dependencies.

## 2. Scope & Key Functionality Tested (What?)

Unit tests focus on the core responsibility of each prompt class:

* **Prompt Generation:** Verifying the output of methods or properties responsible for generating the final prompt string(s) (e.g., `SystemMessage`, `UserMessage`) or structured data for the LLM. This involves:
    * Correctly incorporating input data (e.g., recipe ingredients, user query) into the prompt template.
    * Applying any specific formatting rules defined within the prompt class.
    * Handling different input data scenarios (e.g., missing optional data).
* **Input Validation:** Testing any validation logic within the prompt class that checks the input data before generating the prompt.
* **Output Parsing:** (Less common, usually done by the calling service) If a prompt class *also* includes logic to parse the `LlmResult`, that parsing logic would be tested here against sample `LlmResult` data.

## 3. Test Environment Setup

* **Instantiation:** The specific prompt class (e.g., `AnalyzeRecipePrompt`, `RankRecipePrompt`) is instantiated directly in test methods.
* **Mocking:** Generally not required, as prompt classes typically operate on input data and internal logic/templates.
* **Test Data:** Requires creating instances of input data models (e.g., `Recipe`, `Order`, simple strings) to pass to the prompt class methods being tested. Define expected output prompt strings or structures for assertion.

## 4. Maintenance Notes & Troubleshooting

* **Prompt Wording/Structure:** Tests are tightly coupled to the exact wording, formatting, and structure defined within each prompt class. Any change to the prompt's template or logic requires updating the corresponding tests and expected output assertions.
* **Input Data:** Ensure test input data covers relevant variations that might affect the generated prompt (e.g., recipes with/without certain fields, different query types).
* **Clarity:** Tests should clearly show the input data and the expected output prompt string/structure, making it easy to verify correctness.

## 5. Test Cases & TODOs

Detailed TODOs belong within the specific test files for each prompt class (e.g., `AnalyzeRecipePromptTests.cs`). Ensure test files exist and have comprehensive coverage for at least the following prompt classes:

* **TODO:** Create/Populate `AnalyzeRecipePromptTests.cs`
* **TODO:** Create/Populate `ChooseRecipesPromptTests.cs`
* **TODO:** Create/Populate `CleanRecipePromptTests.cs`
* **TODO:** Create/Populate `GetAlternativeQueryPromptTests.cs`
* **TODO:** Create/Populate `ProcessOrderPromptTests.cs`
* **TODO:** Create/Populate `RankRecipePromptTests.cs`
* **TODO:** Create/Populate `RecipeNamerPromptTests.cs`
* **TODO:** Create/Populate `SynthesizeRecipePromptTests.cs`
* **TODO:** *(Add any other prompt classes present in the source directory)*

**For each prompt test file, ensure TODOs cover:**
* Testing prompt generation with typical valid input data -> verify exact output string/structure.
* Testing prompt generation with edge-case input data (e.g., empty lists, null properties) -> verify correct handling in output.
* Testing any input validation logic within the prompt class.
* Testing any output parsing logic within the prompt class (if applicable).

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for Prompt class unit tests. (Gemini)

