# --- README Template Instructions ---

**Purpose:** This template provides a standardized structure for creating `README.md` files within each directory of the codebase. Its primary goal is to capture essential contextual information, design rationale, interface contracts, and operational guidance that are *not* immediately obvious from the code itself, specifically to aid AI coding assistants in understanding and safely modifying the code within this directory.

**Target Audience:** AI assistants tasked with generating or updating README files for specific code directories.

**Core Goal:** Maximize value for future AI maintainers by documenting the 'why' and the 'rules' of the code in this directory, potentially supplemented by diagrams. Avoid simple code-to-English translation; focus on implicit knowledge, assumptions, and design decisions. **Create a navigable documentation network by linking related READMEs.** This README should complement, not duplicate, information readily available in code comments or the central standards documents in `/Docs/Standards/`.

**How to Use:**
1.  Replace placeholders like `[Directory Name]`, `[YYYY-MM-DD]`, `[Path to Parent README]`, etc., with specific details.
2.  Fill out each section based on the code and components *within this specific directory*.
3.  **Embedding Diagrams:** Section 2 (Architecture & Key Concepts) is the primary location for embedding relevant Mermaid diagrams. Follow the rules in `Docs/Standards/DiagrammingStandards.md` when creating or updating diagrams.
4.  **Linking (Crucial):**
    * **Parent:** Always include a link to the parent directory's README.md (unless this is the root).
    * **Children:** If this directory contains subdirectories with their own READMEs, link to each one from Section 1.
    * **Dependencies/Dependents:** In Section 6, link to the READMEs of any internal dependencies or dependents mentioned.
    * **Diagrams (Optional):** If using separate `.mmd` files, link to them from Section 2.
    * Use relative paths for links (e.g., `../README.md`, `./Subdirectory/README.md`, `../../../docs/diagrams/MyModule/Diagram.mmd`).
5.  Be concise but thorough. Focus on information that a stateless AI assistant would need.
6.  **Pruning:** When updating, remove historical rationale (Section 7) or known issues (Section 8) that are no longer relevant. Keep the documentation focused on the *current* state.
7.  Reference the central standards documents in `/Docs/Standards/` for global rules, but detail *local* conventions or exceptions in Section 4.
8.  Update the `Last Updated` date whenever significant changes are made to the text or embedded diagrams.

# --- End of Instructions ---

**start of template**

---

# Module/Directory: [Directory Name - e.g., /Cookbook/Recipes]

**Last Updated:** [YYYY-MM-DD]

**(Optional: Link to Parent Directory's README)**
> **Parent:** [`[Parent Directory Name]`]([../path/to/parent/README.md])

## 1. Purpose & Responsibility

* **What it is:** [Concise description of this directory's role in the overall system. What core functionality or domain concept does it represent? E.g., "Handles recipe data acquisition, processing, storage, and retrieval."]
* **Key Responsibilities:** [Bulleted list of the main capabilities. E.g., "- Scraping recipe data", "- Cleaning/Standardizing data via AI", "- Indexing and Searching recipes", "- Synthesizing new recipes". Focus on *what* it does functionally.]
* **Why it exists:** [Brief rationale. E.g., "To encapsulate all recipe-related logic, separating it from order processing or authentication."]
* **(Optional: Links to Child Directories' READMEs if applicable)**
    * **Submodule:** [`[Child Directory 1 Name]`](./[ChildDir1]/README.md) - [Brief description of child's role]
    * **Submodule:** [`[Child Directory 2 Name]`](./[ChildDir2]/README.md) - [Brief description of child's role]
    * ...

## 2. Architecture & Key Concepts

* **High-Level Design:** [Brief overview of the main classes/services within this directory (e.g., `RecipeService`, `RecipeRepository`, `WebScraperService`) and how they interact. Mention key interactions with components *outside* this directory (e.g., "Receives requests from `OrderService`").]
* **Core Logic Flow:** [Describe the typical sequence for a primary responsibility, focusing on the interaction between components. E.g., "1. `RecipeService.GetRecipes` called. 2. Checks `RecipeRepository`. 3. If needed, calls `WebScraperService`. 4. Results passed back to `RecipeRepository` for ranking/storage..."]
* **Key Data Structures:** [List the primary C# classes/records representing the core domain data *defined* or *centrally managed* here (e.g., `Recipe`, `ScrapedRecipe`, `SynthesizedRecipe`). No need to list every DTO.]
* **State Management:** [Where is persistent state managed? E.g., "`RecipeRepository` manages state via `FileService` (file system). `RecipeIndexer` maintains an in-memory index."]
* **Diagram(s):** [Embed relevant Mermaid diagram(s) here using ```mermaid code blocks. Diagrams MUST follow [`Docs/Standards/DiagrammingStandards.md`](../Standards/DiagrammingStandards.md). Link to separate `.mmd` files if necessary.]
    ```mermaid
    %% Example Placeholder - Replace with actual diagram
    graph TD
        A[Component A] --> B{Decision};
        B -- Yes --> C[Result 1];
        B -- No --> D[Result 2];
    ```

## 3. Interface Contract & Assumptions

* **Key Public Interfaces (for external callers):** [List the main public methods/entry points intended for use by *other* modules/directories. Focus on methods that represent the module's main capabilities.]
    * `ServiceOrClassName.MethodName(params)`:
        * **Purpose:** [Briefly, what is the goal of calling this?]
        * **Critical Preconditions:** [What *must* be true before calling? (e.g., "`RecipeRepository` initialized", "Valid `LlmConfig` required if scraping", "Input `query` must not be null/empty").]
        * **Critical Postconditions:** [What state change or outcome is guaranteed *after* successful execution? (e.g., "Relevant recipes returned", "New recipes queued for background processing if scraped").]
        * **Non-Obvious Error Handling:** [Mention key exceptions or failure modes *not* obvious from the method signature. (e.g., "Throws `NoRecipeException` if no suitable recipes found after retries", "May throw exceptions from `LlmService` or `FileService`").]
* **Critical Assumptions:** [Bulleted list of vital assumptions this module makes.]
    * **External Systems/Config:** [e.g., "Assumes valid OpenAI API key", "Assumes Playwright dependencies installed", "Relies on `site_selectors.json` being accurate".]
    * **Data Integrity:** [e.g., "Assumes AI responses conform to expected JSON schema", "Assumes `FileService` handles write concurrency correctly".]
    * **Implicit Constraints:** [e.g., "Performance depends on external site speed and AI latency", "File system storage might not scale infinitely", "Designed assuming `IBackgroundWorker` is functional".]

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Configuration:** [Required `appsettings.json` sections (e.g., `RecipeConfig`, `WebscraperConfig`) and critical keys (e.g., API Keys). Mention external config files like `site_selectors.json`.]
* **Directory Structure:** [Specific organization *within* this directory (e.g., `Prompts/`, `Training/`). Location of persisted data (e.g., `Data/Recipes`).]
* **Technology Choices:** [Specific tech used heavily *here* if notable (e.g., Playwright for scraping, specific AI models used).]
* **Performance/Resource Notes:** [Key performance considerations specific to this module (e.g., "Heavy use of parallelism via `RecipeConfig.MaxParallelTasks`", "Scraping can be resource-intensive").]
* **Security Notes:** [Specific security points for this module (e.g., "Requires secure handling of OpenAI key").]

## 5. How to Work With This Code

* **Setup:** [Essential local setup steps *specific* to this module (e.g., "Ensure Playwright installed (`playwright.ps1 install`)", "Configure `ApiKey` in user secrets").]
* **Testing:**
    * **Location:** [Path to relevant test projects/files.]
    * **How to Run:** [Command to run tests for this module.]
    * **Testing Strategy:** [Key areas to focus testing on (e.g., "Mock `LlmService` for unit tests", "Integration tests needed for service interactions", "Test scraping error handling"). Refer to [`Docs/Standards/TestingStandards.md`](../Standards/TestingStandards.md).]
* **Common Pitfalls / Gotchas:** [Non-obvious behaviors or frequent issues (e.g., "Scraping selectors break if websites change", "AI model updates might require prompt changes", "Ensure thread-safety around index updates").]

## 6. Dependencies

* **Internal Code Dependencies:** [List other key project directories/modules this code *consumes*. **Link to their READMEs.**]
    * [`[Dependency Dir Name]`]([../path/to/dependency/README.md]) - [Brief reason for dependency]
    * ...
* **External Library Dependencies:** [List key NuGet packages or external libraries used directly and their purpose (e.g., `OpenAI`, `Playwright`, `Polly`, `AutoMapper`).]
* **Dependents (Impact of Changes):** [List other key project directories/modules that *consume this code*. **Link to their READMEs.** Helps assess impact.]
    * [`[Dependent Dir Name]`]([../path/to/dependent/README.md]) - [Brief nature of dependency]
    * ...

## 7. Rationale & Key Historical Context

* [Briefly explain non-obvious design choices *specific* to this module (e.g., "Why file-based storage was chosen over a DB", "Why extensive AI processing is used here"). Link to separate ADRs if used.]

## 8. Known Issues & TODOs

* [Bulleted list of known limitations or planned work *specific* to this module (e.g., "Scraping fragility requires ongoing selector maintenance", "Initial load performance might degrade with many recipes"). Link to issue tracker if available.]

---

**end of template**
