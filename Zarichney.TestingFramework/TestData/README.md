# README: /TestData Directory

**Version:** 1.1
**Last Updated:** 2025-05-22
**Parent:** `../README.md`

## 1. Purpose & Responsibility

This directory, `/TestData/`, is responsible for storing and managing various test data artifacts used across both unit and integration tests within the `api-server.Tests` project. Its primary purpose is to support the creation of consistent, reusable, and maintainable test data, facilitating clearer and more robust test scenarios.

This includes:
* **Test Data Builders (`./Builders/`):** Fluent builder classes for constructing complex domain objects or DTOs with specific states required for particular test cases.
* **Sample Static Data Files (e.g., `./Recipes/Burger.json`):** Fixed data files (typically JSON) used when tests require a predefined, static dataset that is not easily or clearly generated dynamically.

This directory complements the dynamic data generation capabilities provided by AutoFixture (often accessed via `GetRandom.cs`) and the planned advanced AutoFixture customizations in `../Framework/TestData/AutoFixtureCustomizations/`. While AutoFixture excels at providing anonymous, structurally valid data, the components in *this* `/TestData/` directory offer more explicit control or provide fixed baseline data.

Adherence to the principles outlined here and in the main testing standards (`../../Zarichney.Standards/Standards/TestingStandards.md`, `../../Zarichney.Standards/Standards/UnitTestCaseDevelopment.md`, `../../Zarichney.Standards/Standards/IntegrationTestCaseDevelopment.md`) is expected.

### Child Modules / Key Subdirectories:

* **`./Builders/README.md`**: Contains specific guidance and an overview of available Test Data Builders.
* **`./Recipes/`**: Example subdirectory for organizing static sample recipe data. Similar subdirectories may exist for other domains.

## 2. Architecture & Key Concepts

* **Test Data Builders (`./Builders/`):**
    * These classes (e.g., `RecipeBuilder.cs`, `ConfigurationItemStatusBuilder.cs`) implement the Builder pattern to provide a fluent API for constructing test objects.
    * They allow tests to define objects in a readable way, setting only the properties relevant to the test scenario while relying on sensible defaults for others.
    * Builders often use AutoFixture internally (e.g., via `GetRandom`) to populate properties not explicitly set by the test, promoting the principle of anonymous data where appropriate.
    * A `BaseBuilder.cs` might provide common functionality for all builders.
* **Sample Static Data Files:**
    * These are typically JSON files (e.g., `Burger.json`) representing specific instances of DTOs or entities.
    * Used for:
        * Testing deserialization logic.
        * Providing baseline data for specific integration test scenarios where the exact data is critical and less prone to change.
        * Input for tests that verify behavior against a known, fixed dataset.
* **Relationship to AutoFixture:**
    * **AutoFixture (via `GetRandom` or direct usage):** Preferred for generating anonymous, general-purpose test data where specific values are not critical to the test's logic.
    * **Test Data Builders:** Used when more control is needed to create objects in specific valid states, or when object creation is complex but needs to be readable and reusable. They often *leverage* AutoFixture.
    * **Static Data Files:** Used for explicit, unchanging datasets that are fundamental to a test case.
    * **Advanced AutoFixture Customizations (`../Framework/TestData/AutoFixtureCustomizations/`):** For creating reusable `ISpecimenBuilder` or `ICustomization` instances that modify AutoFixture's core behavior for generating specific types (especially complex domain/EF entities).

## 3. Interface Contract & Assumptions

* **Test Data Builders:**
    * **Interface:** Tests instantiate builder classes directly and use their fluent methods (e.g., `With[PropertyName]()`) to configure an object, finally calling a `Build()` method to get the constructed instance.
    * **Assumptions:** Builders are implemented correctly to produce valid domain objects or DTOs according to the rules set via their methods and internal defaults.
* **Static Data Files:**
    * **Interface:** Tests typically read these files (e.g., using `File.ReadAllTextAsync`) and deserialize them into C# objects (e.g., using `System.Text.Json.JsonSerializer`).
    * **Assumptions:** The structure and content of these static files are kept synchronized with the C# model classes they represent. Outdated static files can lead to deserialization errors or incorrect test logic.

## 4. Local Conventions & Constraints

* **Naming:**
    * Test Data Builders: `[EntityName]Builder.cs` (e.g., `RecipeBuilder.cs`).
    * Static data files: Should be named descriptively and placed in subdirectories organized by domain or entity type (e.g., `./Recipes/`, `./Users/`).
* **Builder Design:**
    * Builders should aim for a fluent and intuitive API.
    * Each `With[PropertyName]()` method should typically return the builder instance (`this`) to allow method chaining.
    * The `Build()` method should return the constructed object.
    * Builders should encapsulate default values for properties not explicitly set, making test setup concise.
* **Static Data Usage:** Use static data files sparingly. Prefer dynamic data generation (AutoFixture, Builders) unless the data must be exact and fixed for the test's purpose. Over-reliance on static files can make tests more brittle to model changes.
* **Organization:** Keep this directory well-organized. If the number of static files or builders for a particular domain grows large, create appropriate subdirectories.

## 5. How to Work With This Code

### Using Test Data Builders

* Instantiate the builder (often passing an `IFixture` instance if it uses AutoFixture internally via `GetRandom`).
* Use its fluent methods to set specific properties.
* Call `Build()` to get the object.

    ```csharp
    // Example using RecipeBuilder, assuming _fixture is an available IFixture instance
    var recipe = new RecipeBuilder(_fixture) // _fixture often from [AutoData] or test base
        .WithTitle("Spicy Test Curry")
        .WithCuisine("Indian")
        .WithServings(2)
        .Build();

    // Use 'recipe' in your test
    ```

### Using Static Data Files

* Read the file content.
* Deserialize to the target C# object.

    ```csharp
    // Example: Reading a JSON file (path relative to test execution)
    // Note: Ensure the JSON file is copied to the output directory (set "Copy to Output Directory" in .csproj)
    var burgerJsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "Recipes", "Burger.json"); // Adjust path as needed
    if (!File.Exists(burgerJsonPath)) throw new FileNotFoundException("Burger.json not found", burgerJsonPath);

    var burgerJson = await File.ReadAllTextAsync(burgerJsonPath);
    var burgerRecipe = JsonSerializer.Deserialize<RecipeDto>(burgerJson); // Assuming RecipeDto and System.Text.Json

    // Use 'burgerRecipe' in your test
    ```

### Adding New Test Data Artifacts

* **New Builder:**
    1.  Create a new class in `./Builders/` (e.g., `UserBuilder.cs`).
    2.  Implement the fluent builder pattern, potentially using `GetRandom` for default/random values.
    3.  Add XML documentation.
    4.  Update `./Builders/README.md`.
* **New Static Data File:**
    1.  Create the data file (e.g., a JSON file) with the required content.
    2.  Place it in an appropriate subdirectory (e.g., `./Users/SampleAdminUser.json`).
    3.  Ensure its "Copy to Output Directory" property in the `.csproj` file is set (e.g., "Copy if newer" or "Copy always") so it's available at runtime.
    4.  Update this README or a relevant subdirectory README if the organization warrants it.

## 6. Dependencies

### Internal Dependencies

* **`api-server` Project:** Test Data Builders and static files often represent or construct DTOs and domain models from the `api-server`.
* **`../Framework/Helpers/GetRandom.cs`:** Commonly used by Test Data Builders to leverage AutoFixture.
* **`../Unit/` and `../Integration/` Tests:** These are the consumers of the test data artifacts provided here.

### Key External Libraries

* **`AutoFixture`**: Used indirectly by Builders via `GetRandom`.
* **`System.Text.Json`** (or `Newtonsoft.Json`): Used for deserializing static JSON data files.

## 7. Rationale & Key Historical Context

The test data strategy for this project employs a multi-faceted approach to balance ease of use, maintainability, and control:
* **AutoFixture (Primary):** For general-purpose, anonymous data generation, reducing test brittleness from overly specific hardcoded values.
* **Test Data Builders:** Introduced for more complex objects where a fluent, readable setup is desired, or where specific combinations of properties define important test scenarios. They bridge the gap between pure anonymous data and highly specific hardcoded objects.
* **Static Data Files:** Reserved for cases where a fixed, external representation of data is necessary (e.g., testing against a specific payload, deserialization tests).

This layered approach allows tests to use the most appropriate data generation method for their needs, guided by the principles in the main testing standards. The planned `../Framework/TestData/AutoFixtureCustomizations/` directory will further enhance AutoFixture's capabilities for complex types.

## 8. Known Issues & TODOs

* **Builder Coverage:** Continue developing Test Data Builders for all key domain entities and complex DTOs to simplify test setup across the suite. (Tracked via tasks referencing the `../../Zarichney.Standards/Templates/GHTestCoverageTask.md` template where applicable for new features).
* **Static File Maintenance:** Ensure a process is in place (e.g., as part of code review or specific audit tasks) to check that static data files are kept synchronized with their corresponding C# models to prevent deserialization issues.
* **Integration with Advanced AutoFixture Customizations:** As `../Framework/TestData/AutoFixtureCustomizations/` is populated, Test Data Builders may be refactored to leverage these advanced customizations for even more powerful and consistent data generation.
* Refer to the "Framework Augmentation Roadmap (TODOs)" in `../TechnicalDesignDocument.md` for broader framework enhancements related to test data.

---
