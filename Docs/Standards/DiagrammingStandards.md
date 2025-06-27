# Project Diagramming Standards (Mermaid.js)

**Version:** 1.2
**Last Updated:** 2025-05-03

## 1. Purpose and Scope

* **Purpose:** To define the mandatory standards, practices, and quality expectations for creating and maintaining architectural and workflow diagrams using Mermaid.js within the `Zarichney.Server` solution. These standards ensure clarity, accuracy, consistency, and maintainability of visual documentation, supporting both human understanding and AI-assisted development.
* **Scope:** Applies to all diagrams created using Mermaid.js syntax, typically embedded within per-directory `README.md` files or potentially stored as separate `.mmd` files linked from READMEs.
* **Relationship to Other Standards:**
    * This document complements **[`./DocumentationStandards.md`](./DocumentationStandards.md)**, which governs the overall structure and maintenance of README files where these diagrams are embedded.
    * Diagram content must accurately reflect the code structure and conventions defined in **[`./CodingStandards.md`](./CodingStandards.md)**.
    * Diagram creation and maintenance are integrated into the AI Coder workflow described in **[`/Docs/Development/CodingPlannerAssistant.md`](../Development/CodingPlannerAssistant.md)**.

## 2. Core Philosophy & Principles

* **Accuracy First:** Diagrams **MUST** accurately represent the current state of the codebase's architecture, component interactions, or workflows they depict. Prioritize technical correctness over simplification if simplification would lead to inaccuracy.
* **Clarity for Dual Audience:** Diagrams should be clear and understandable for both human developers and AI assistants. Use standard conventions, semantic styling, and comments within the Mermaid code where necessary to aid interpretation.
* **Maintainability (Diagrams as Code):** Treat Mermaid diagram definitions as code artifacts. They **MUST** be stored in version control (Git) and updated alongside related code changes.
* **Consistency:** Adhere strictly to the diagram types, styling conventions, and linking strategies defined in this document.
* **Integration:** Embed diagrams directly within the relevant `README.md` files whenever feasible to keep visualizations close to the contextual documentation.

## 3. Diagram Creation & Maintenance Mandate

* **Trigger:** Any task (performed by human or AI) that introduces or modifies code affecting the documented architecture, component interactions, dependencies, or primary workflows within a module **MUST** also create or update the relevant Mermaid diagram(s) within the same commit/change.
* **Responsibility:** Both human developers and AI Coders are responsible for adhering to these standards when creating or updating diagrams. The AI Coder workflow explicitly includes checking for and performing necessary diagram updates.

## 4. Diagram Types and Usage

Select the most appropriate Mermaid diagram type for the specific architectural aspect being visualized. Prioritize detailed, accurate diagrams. Simplified overview diagrams are acceptable *only* for very high-level contexts (e.g., root README), and should ideally link to more detailed diagrams if applicable.

* **Flowcharts (`graph`):**
    * **Use For:** High-level module overviews, conceptual middleware pipelines, background job processing logic, simple decision flows within services, development workflows.
    * **Key Syntax:** Nodes (`id[Label]`), Edges (`-->`, `-.->`), Subgraphs (`subgraph...end`), Direction (`TD`, `LR`, `RL`).
* **Sequence Diagrams (`sequenceDiagram`):**
    * **Use For:** Detailing time-ordered interactions for specific API requests (e.g., login, order creation), CQRS command/query flows, communication between services, external API calls. **Highly recommended for clarifying complex interactions.**
    * **Key Syntax:** Participants (`participant Name as Alias`), Messages (`->>`, `->`, `-->`, `-x`), Activations (`activate`/`deactivate`, `+/-`), Fragments (`loop`, `alt`, `opt`).
* **Class Diagrams (`classDiagram`):**
    * **Use For:** Modeling static structure and dependencies between key classes/interfaces within a module (e.g., Controller-Service-Repository relationships, DI dependencies, inheritance). Focus on interfaces and key components, avoid excessive detail.
    * **Key Syntax:** Classes/Interfaces (`class`, `interface`, `<<Annotation>>`), Members (`+Name: type`), Relationships (`<|--`, `*--`, `o--`, `-->`, `..>`).
* **Entity Relationship Diagrams (`erDiagram`):**
    * **Use For:** Visualizing the database schema (tables, columns, keys, relationships) relevant to a module, particularly for the main `UserDbContext`.
    * **Key Syntax:** Entities (`ENTITY { type name [PK/FK] }`), Relationships (`||--o{`, etc.).
* **C4 Model Diagrams (`C4Context`, `C4Container`, `C4Component`):**
    * **Use For:** Providing standardized, hierarchical views of the system architecture (Context, Containers, Components). Useful for top-level documentation.
    * **Key Syntax:** Specific elements (`Person`, `System`, `Container`, `Component`), `Rel`/`BiRel`. Be aware support is still evolving in Mermaid.

## 5. Location and Naming

* **Primary Location:** Embed Mermaid code blocks (\`\`\`mermaid ... \`\`\`) directly within the relevant section (typically Section 2: Architecture & Key Concepts) of the corresponding directory's `README.md` file.
* **Complex Diagrams:** For exceptionally large or complex diagrams that clutter the README, store the diagram definition in a separate `.mmd` file within a `/docs/diagrams/[ModuleName]/` subdirectory (e.g., `/docs/diagrams/Cookbook/OrderProcessingSequence.mmd`). Embed a link to the `.mmd` file or a static image rendering (less preferred due to sync issues) in the README.
* **File Naming (if separate):** Use descriptive names: `[ModuleName]-[DiagramType]-[Focus].mmd` (e.g., `Auth-Sequence-Login.mmd`, `Services-Flowchart-Overview.mmd`).

## 6. Content and Detail

* **Accuracy:** Diagrams MUST reflect the actual code structure, dependencies, and interaction flows.
* **Scope:** Each diagram should have a clear focus. Use decomposition for complex systems (see Section 9).
* **Key Elements:** Include relevant classes, services, interfaces, databases, external APIs, middleware components, commands/queries, etc.
* **Labels:** Use clear, concise labels for nodes and edges. Avoid problematic formats (see Section 10).
* **Mermaid Comments (`%% ...`):** Add comments within the Mermaid code block to explain complex logic, clarify non-obvious relationships, or provide guidance for future AI interpretation. **Crucially, place comments on their own lines.** (See Section 10).

## 7. Styling Conventions (**Mandatory**)

Consistent styling enhances readability and provides semantic meaning.

* **Standard `classDef` Styles:** A standard set of `classDef` styles **MUST** be used across all diagrams. These styles should visually differentiate common component types. Apply these classes using individual `class nodeId className;` statements (See Section 10).

    ```mermaid
    %% Standard Project Styles (Define these consistently)
    classDef human fill:#cceeff,stroke:#007acc,stroke-width:1px;
    classDef aiPlanner fill:#e6f2ff,stroke:#004080,stroke-width:1px;
    classDef aiCoder fill:#e6ffe6,stroke:#006400,stroke-width:1px;
    classDef process fill:#fff,stroke:#333,stroke-width:1px;
    %% Shape applied directly or via style
    classDef document fill:#f5f5f5,stroke:#666,stroke-width:1px;
    classDef codebase fill:#f5deb3,stroke:#8b4513,stroke-width:1px;
    classDef input fill:#fff0b3,stroke:#cca300,stroke-width:1px;
    %% For DB nodes like [()]
    classDef database fill:#f5deb3,stroke:#8b4513,stroke-width:2px;
    classDef externalApi fill:#ffcc99,stroke:#ff6600,stroke-width:2px;
    classDef middleware fill:#eee,stroke:#666,stroke-width:1px;
    classDef background fill:#fff0b3,stroke:#cca300,stroke-width:1px;
    classDef queue fill:#d1e0e0,stroke:#476b6b,stroke-width:1px;
    classDef ui fill:#cceeff,stroke:#007acc,stroke-width:1px;
    classDef repository fill:#ccf,stroke:#333,stroke-width:2px,stroke-dasharray: 5 5;
    classDef service fill:#ccf,stroke:#333,stroke-width:2px;
    classDef controller fill:#f9f,stroke:#333,stroke-width:2px;
    classDef interface fill:#fff,stroke:#00f,stroke-width:2px;
    classDef command fill:#e6ffe6,stroke:#006400,stroke-width:1px;
    classDef query fill:#e6f2ff,stroke:#004080,stroke-width:1px;
    ```

* **Standard Node Shapes:** Use appropriate node shapes where available to convey meaning (e.g., `[()]` for database, `>` for parallelogram/input, `{}` for decision). Define shapes directly on the node (`NodeId>Label]`) rather than within `classDef` for better compatibility.
* **`style` Directive:** Use sparingly for highlighting specific elements (e.g., a critical component, a starting point) that deviate from the standard class styles.
* **Themes:** Stick to the `default` theme unless a project-wide decision is made to customize the `base` theme via `themeVariables`. Avoid ad-hoc theme changes per diagram.

## 8. Edge/Link Conventions (**Mandatory**)

Use specific line styles, arrowheads, and labels to convey interaction semantics accurately.

* **Synchronous Call (e.g., direct method call):**
    * Flowchart: `-->` (Solid line, filled arrow)
    * Sequence: `->>` (Filled arrow)
* **Asynchronous Message/Event (e.g., queue, MediatR notification):**
    * Flowchart: `-.->` (Dotted line, filled arrow) or `-->` labeled "Posts Event"
    * Sequence: `->` (Open arrow)
* **Dependency (e.g., DI Relationship):**
    * Class: `..>` (Dotted line, open arrow) - Use for dependency on interfaces/classes.
* **Implementation/Inheritance:**
    * Class: `..|>` (Realization), `<|--` (Generalization) - Use standard UML arrows.
* **Data Flow / Input:**
    * Flowchart: Use `-.->` (Dotted line, filled arrow) to indicate data/config/prompt inputs flowing *into* a process step.
* **Labels:** Use concise labels on edges (`-->|"Label Text"|`) where the relationship type isn't immediately obvious from the line style or context. Ensure label text formatting avoids issues (see Section 10).

## 9. Complexity Management

* **Decomposition:** Break down large, complex systems into multiple, focused diagrams (see Section 4 - Diagram Types). Avoid monolithic diagrams.
* **Layering:** Use different diagram types to show different layers of abstraction (e.g., C4 flow: Context -> Container -> Component; or Overview Flowchart -> Detailed Sequence Diagram).
* **Linking:** Leverage the documentation platform (Markdown links in READMEs) to connect related diagrams (e.g., link from a component in an overview diagram to its detailed sequence diagram). Use consistent naming for linked diagrams.
* **Subgraphs/Boundaries:** Use `subgraph` (Flowchart) or `Boundary` (C4) effectively to group related components visually. Avoid excessive nesting. Define nodes within their intended subgraphs *before* defining links separately to prevent layout issues caused by reordering.

## 10. Syntax Notes & Common Pitfalls (**Mandatory Adherence**)

Based on practical experience and renderer limitations, follow these syntax rules strictly:

* **Code Block Indentation:** The Mermaid code block fence (\`\`\`mermaid) **MUST NOT** be indented within the Markdown file. Start it at the beginning of the line to avoid rendering issues in some platforms.
    * *Correct:*
    ```markdown
    Some preceding text.
    ```mermaid
    graph TD
        A --> B
    ```
    ```
    * *Incorrect:*
    ```markdown
        * Some list item.
            ```mermaid
            graph TD
                A --> B
            ```
    ```
* **Quoting Labels:** Node and Link labels containing special characters used by Mermaid syntax (e.g., `( )`, `[ ]`, `{ }`, `#`, `;`, `/`, `+`), Markdown list characters (`- `, `* `, `1. `), or leading/trailing spaces **MUST** be enclosed in double quotes `""`.
    * *Example:* `MyNode["Label with (parentheses)"]`
    * *Example:* `A -- "Label with / Slash" --> B`
    * *Example:* `C -- "1 - Initial Step" --> D`
* **Markdown Lists in Labels:** Avoid using Markdown list syntax (`- item` or `1. item`) or newlines (`\n`) within node labels, as they may render incorrectly or inconsistently. Use single-line descriptions or break complex lists into separate nodes.
    * *Recommended:* `NodeA["Step 1: Action A | Step 2: Action B"]`
    * *Avoid:* `NodeA["- Action A\n- Action B"]`
* **Comment Placement:** Comments (`%%`) **MUST** be placed on their own dedicated lines *before* or *after* the definition (node, link, classDef, etc.) they refer to. **Do NOT place comments on the same line immediately following a definition.**
    * *Correct:*
    ```mermaid
    %% This comment describes Node A
    A[Node A]
    %% This comment describes the link
    A --> B
    ```
    * *Incorrect:* `A[Node A] %% Incorrect comment placement`
    * *Incorrect:* `A --> B %% Incorrect comment placement`
* **Class Application:** Apply `classDef` styles to nodes using individual statements for each node: `class nodeId className;`. **Avoid** applying classes to multiple nodes listed with commas on a single line (e.g., `class node1,node2 className;`) as this is unreliable across renderers.
    * *Correct:*
    ```mermaid
    class NodeA myStyle;
    class NodeB myStyle;
    ```
    * *Incorrect:* `class NodeA, NodeB myStyle;`
* **Link Label Formatting:** As mentioned under Quoting Labels, avoid starting link labels directly with list markers like `1.` or `-`. Use alternatives like `"1 - Description"`, `"Step 1: Description"`, or just `"Description"`.
    * *Recommended:* `A -- "1 - Initial Step" --> B`
    * *Avoid:* `A -- "1. Initial Step" --> B`
* **Sequence Diagram Activation (`activate`/`deactivate`):** While useful for showing execution focus, these commands can sometimes cause rendering errors (e.g., "Trying to inactivate an inactive participant") in certain renderers, particularly with complex nesting or `alt`/`opt`/`loop` blocks.
    * **Recommendation:** Use `activate`/`deactivate` sparingly if needed for clarity.
    * **Troubleshooting:** If rendering errors related to activation occur, even if the syntax seems correct according to the Mermaid documentation or Live Editor, **simplify the diagram by removing all `activate` and `deactivate` commands.** The core message flow will remain, and this typically resolves renderer-specific activation state issues.
* **Renderer Inconsistencies:** Be aware that diagrams may render slightly differently or encounter errors in various environments (e.g., GitHub, GitLab, VS Code Preview, Confluence Plugin) compared to the official Mermaid Live Editor ([https://mermaid.live/](https://mermaid.live/)).
    * **Validation:** Always test complex diagrams in the target rendering environment(s) *and* the Live Editor.
    * **Workaround:** If a diagram uses valid syntax (verified in Live Editor) but consistently fails to render correctly in a target environment due to suspected renderer bugs/limitations (like activation issues), the recommended workaround is to replace the Mermaid code block with a **static image (PNG/SVG)** generated from a reliable source (Live Editor export, `mmdc` CLI). Document this change clearly.

## 11. Tooling Recommendations

* **Development/Validation:**
    * **IDE Extensions:** Use VS Code extensions (e.g., "Markdown Preview Mermaid Support", "Mermaid Editor") for live preview and syntax highlighting while editing READMEs or `.mmd` files.
    * **Mermaid Live Editor (`mermaid.live`):** Use the official online editor for quick syntax checking, experimentation, and sharing diagram links. **Use this as the source of truth for syntax validity.**
* **Generation (Optional):**
    * **Mermaid CLI (`mmdc`):** Can be used to generate static SVG/PNG files from `.mmd` sources for build processes or embedding where live rendering isn't supported. Also useful for automated syntax validation in CI/CD.

## 12. Maintenance (AI Coder Responsibility)

* **Review Trigger:** Before implementing code changes, the AI Coder **MUST** review the relevant diagrams in the target module's README to understand the existing architecture and flow.
* **Update Mandate:** As part of the standard AI Coder workflow, if code changes impact the architecture, interactions, or flows depicted in a Mermaid diagram, the AI Coder **MUST**:
    1.  Update the Mermaid diagram definition(s) to accurately reflect the changes.
    2.  Adhere strictly to the standards defined in *this* document (`DiagrammingStandards.md`), paying close attention to Section 10.
    3.  Ensure the updated diagram renders correctly (visual check if possible, rely on syntax correctness otherwise, validate against Live Editor if errors occur, simplify if necessary - see Section 10).
    4.  Update the `Last Updated:` date in the containing `README.md` file if the diagram was modified.
* **Pruning:** Remove obsolete elements or diagrams if architectural changes render them invalid.