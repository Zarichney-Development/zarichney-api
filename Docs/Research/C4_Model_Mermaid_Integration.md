# Integrating C4 Model with Mermaid.js for Full-Stack Angular Architecture

## Introduction

The **C4 model** (Context, Containers, Components, Code) is a hierarchical approach to visualizing software architecture. It provides multiple “zoom levels” of diagrams – from a big-picture system Context down to code details – allowing different stakeholders to get the appropriate level of detail. By integrating C4 with **Mermaid.js**, we can maintain **“diagrams as code”** in our repository, ensuring architecture diagrams are version-controlled, up-to-date, and easy to edit. This report covers how to apply C4 in Mermaid for our full-stack Angular (frontend) and .NET (backend) application, with practical Mermaid syntax examples and guidelines for embedding these diagrams in our documentation.

## 1. C4 Model Overview & Mermaid Support

**What is the C4 Model?** The C4 model is a diagramming framework that defines four layers of architecture diagrams: **Level 1: System Context**, **Level 2: Container**, **Level 3: Component**, and **Level 4: Code**. Each level zooms in on the next, akin to Google Maps zooming from a city to a street map. This hierarchy helps present complex architectures in a clear, structured way for different audiences (e.g. executives might only need the Context, developers need Component detail). Key benefits of C4 include a **consistent abstraction** per diagram and avoidance of the “big ball of mud” architecture diagram by splitting views by scope and detail. The model also encourages including descriptive text on diagrams (titles, subtitles) for clarity.

**Mermaid.js and C4:** Mermaid has *built-in support* for C4 diagrams (currently experimental) allowing us to define Persons, Systems, Containers, Components, etc., directly in Mermaid syntax. Mermaid supports five C4 diagram types: **`C4Context`**, **`C4Container`**, **`C4Component`**, **`C4Dynamic`**, and **`C4Deployment`**. Under the hood, it aligns with the popular C4-PlantUML syntax, meaning we use element declarations like `Person(alias, "Label", "Description")`, `Container(alias, "Label", "Technology", "Description")`, `Component(alias, "Label", "Technology", "Description")`, and relationship lines via `Rel(from, to, "Label", "Technology")`. This syntax yields standardized visuals (e.g. people stick-figures, container icons, etc., with consistent styling).

**All Four C4 Levels in Mermaid:** Here’s how to implement each C4 level with Mermaid, along with which diagram types or Mermaid features best suit them:

* **Level 1: System Context Diagram** – Use the Mermaid `C4Context` diagram. This shows the **big picture**: the primary software system (the one we’re building), the users/actors, and any external systems it interacts with. Include all key external dependencies here (third-party services, integrations) as **external systems**. Mermaid provides element types like `Person` (for human users) and `System`/`System_Ext` (for software systems, with \*\_Ext for externals). **Example:** A context diagram for our application might have a *User* actor using our *Web Application* system, which in turn connects to external systems like *Stripe* or *SendGrid*. In Mermaid syntax:

  ```mermaid
  C4Context
     title System Context - Full-Stack Application
     Person(user, "End User", "Customer using the web app")
     System(ourApp, "Zarichney Web App", "Angular + .NET System")
     System_Ext(emailSvc, "Email Service", "External email API (SendGrid)")
     System_Ext(paymentSvc, "Payment Provider (Stripe)", "External payment system")
     Rel(user, ourApp, "Uses", "Web browser")
     Rel(ourApp, emailSvc, "Sends notification emails via")
     Rel(ourApp, paymentSvc, "Processes payments via")
  ```

  This context diagram shows the user and highlights that our system depends on an external email service and Stripe. (All internal detail of our app is abstracted as a single box here.) The **primary benefit** is clarity on scope and integrations: a new engineer or stakeholder can immediately see *who* uses the system and *what external services* are involved.

* **Level 2: Container Diagram** – Use the `C4Container` diagram to zoom into our system and show *major components* of the system as separate deployable containers. Each **Container** in C4 is something like an application, service, database, or browser app – basically a distinct runtime/process or data store. In our case, the main containers are: the **Angular SPA** (runs in the browser), the **.NET Web API** (server), the **PostgreSQL database**, plus perhaps other services like a Redis cache or message queue if we had them. External dependencies can be shown here as well (often as `Container_Ext` or `System_Ext`). Mermaid’s `System_Boundary` syntax lets us group our containers within the system’s boundary. We’ll provide a concrete example in the next section (Section 2) illustrating our full-stack architecture. Container diagrams help developers understand the high-level *distribution of responsibilities*: e.g. UI vs API vs DB, and how they communicate (arrows labeled with protocols like HTTPS, etc.).

* **Level 3: Component Diagram** – Use the `C4Component` diagram to break down an individual container and show its internal components. In C4, a *component* is a grouping of related functionality encapsulated behind an interface (not necessarily a single class – often a set of classes or a subsystem). For example, within our Angular SPA (which is one container), we might consider major front-end subsystems or modules as “components.” In a backend API container, a component diagram might show controllers, services, repositories, etc. In an Angular frontend, components could include UI components, services, state management pieces, etc. Mermaid’s syntax allows defining components inside a container boundary (`Container_Boundary(containerAlias) { ... Component(...) ... }`). We’ll give a detailed Angular example in Section 3. Because Angular uses a module-less **standalone component** architecture (no NgModules), we typically map each important **feature area** or **service** to a C4 *Component*. This level is critical for developers: it shows how the code is organized inside the app, e.g. which parts of the Angular app talk to which services or stores.

* **Level 4: Code** – The C4 model’s last level is code, often actual source code or very fine-grained diagrams of how code works. In practice, C4 encourages simply linking to real code (since code itself is the ultimate detail), rather than drawing lots of low-level UML. However, we can use Mermaid **class diagrams** or **sequence diagrams** at this level if needed to illustrate detailed design or execution flow:

  * *Class Diagrams (`classDiagram`)* – to show class relationships, data models, or internal class structure of a module. For instance, a class diagram could depict an Angular component class, its inherited base classes or interfaces, and perhaps related data models or service classes.
  * *Sequence Diagrams (`sequenceDiagram`)* – to show an example flow or algorithm in detail (this is analogous to a C4 Dynamic diagram). For example, a sequence diagram of the **Login process** could complement the static component diagram: it would show the timeline of a login request from the UI component to the API and back. Sequence diagrams focus on *interaction over time*, which can be useful to understand dynamic behavior that static C4 diagrams (which are structural) don’t capture.
  * *Note:* In Mermaid, C4Dynamic (for dynamic diagrams) is also supported, but it’s essentially a way to index sequence flows between elements. We find standard `sequenceDiagram` often clearer for step-by-step flows. Use sequence charts to augment C4 diagrams when process clarity is needed (for example, how an Angular component dispatches an NgRx action, how the effect calls an API, etc.).

**Mermaid Syntax Quick Reference:** At all C4 levels, Mermaid’s C4 syntax uses similar function-like declarations. Basic elements include `Person`, `Person_Ext` (external user), `System`, `System_Ext` (external system), `Container` (and variants like `ContainerDb`, `ContainerQueue`, plus \*\_Ext for externals), and `Component` (and variants). Relationships are declared with `Rel(source, target, "Label", "Optional technology")` which draws an arrow with an optional label. Mermaid automatically styles these differently (persons as stick figures, containers as boxes, etc.) with a consistent legend. It’s worth noting the C4 support in Mermaid is marked *experimental*, so some layout fine-tuning might be needed (e.g., the order of definitions can affect layout). However, it greatly simplifies diagram creation by providing semantic elements instead of raw shapes. If a particular C4 diagram becomes too unwieldy or the Mermaid syntax feels limiting, it’s acceptable to simplify – e.g. use a standard flowchart or break one diagram into two – focusing on **clarity over strict adherence** to the model (remember our “practicality over purity” guideline). The goal is communicative diagrams, not just checking a box for all four levels.

## 2. Full-Stack Architecture: C4 Container Diagram Example

To illustrate the **Container level**, below is a definitive example of a C4 Container diagram for our architecture. This diagram focuses on the major deployable pieces of the system: the Angular front-end, the .NET back-end, the database, and external services like email and payments. We use Mermaid’s `C4Container` syntax to define each container and how they connect:

```mermaid
C4Container
    title Container Diagram - Web Application System
    Person(user, "User", "Customer using the web app")
    System_Boundary(webAppSystem, "Zarichney Web Application") {
        Container(spa, "Angular SPA", "TypeScript, Angular 16", "Single-page application UI [Standalone Components]")
        Container(api, ".NET Web API", "C# .NET 7 Web API", "Backend REST API application")
        ContainerDb(db, "PostgreSQL Database", "PostgreSQL 14", "Operational database for persistent data")
    }
    Container_Ext(emailSvc, "Email Service (SendGrid)", "Cloud API", "External email delivery service")
    Container_Ext(paymentSvc, "Payment Provider (Stripe)", "REST API", "External payment processing system")
    Person_Ext(admin, "Admin User", "Internal admin managing content") %% example of another actor if needed
    %% Relationships 
    Rel(user, spa, "Uses", "HTTPS/Browser")
    Rel(spa, api, "Makes API calls to", "JSON/HTTPS")
    Rel(api, db, "Reads & writes", "SQL/TCP")
    Rel(api, emailSvc, "Sends emails via", "HTTPS API")
    Rel(api, paymentSvc, "Calls for payment processing", "HTTPS API")
    %% (Optional) External interactions from admin:
    Rel(admin, api, "Uses admin interface", "HTTPS")
```

In this container diagram:

* We defined the **Angular SPA** as a Container (`spa`) running in the browser (technology: Angular/TypeScript).
* The **Web API** is another Container (`api`) running on .NET.
* The **Database** is defined with the `ContainerDb` element for a database.
* External services like the email system and Stripe are represented with `Container_Ext` (for external containers our system uses). They could also be shown as `System_Ext` since from our system’s perspective they are external systems – either is semantically acceptable. Here we treat them as external containers to emphasize they are external applications/services.
* We also included an example **Person\_Ext** (`admin`) for an administrative user role, to illustrate multiple actors (if our system has an internal admin interface, for example). This is optional based on whether it’s relevant to show different user types.

The **relationships** (Rel lines) show how the containers interact:

* The user *uses* the Angular SPA via a web browser (HTTPS).
* The SPA *calls* the .NET API (JSON over HTTPS).
* The API *reads/writes* to the PostgreSQL database.
* The API also *calls out* to the external Email service and Stripe API to perform its functions (sending emails, processing payments).

This diagram gives a one-glance understanding of our full-stack: the front-end, back-end, and data store, plus what third-party systems are in play. It also aligns with our **Nx monorepo structure** – for instance, the Angular SPA and .NET API might reside in separate projects or directories within the monorepo, but together they compose the overall system. The container diagram should reflect these high-level boundaries (frontend vs backend vs database) which likely also correspond to Nx’s designated project boundaries (e.g., an `apps/website` for the Angular app, `apps/api` for the .NET service, etc., with the database being an external resource).

**Styling and Clarity:** Even though Mermaid’s C4 auto-styles elements, ensure to include a title and concise descriptions for each container as shown. The example labels show technologies (e.g., Angular, .NET, PostgreSQL) – this is valuable context for readers. We also note that our Angular app uses **standalone components** (as indicated in the SPA’s description) – this hints at how the frontend is built without NgModules, a detail that becomes relevant at the component diagram level. If needed, we could apply consistent visual styling via Mermaid classes (e.g., coloring all external services differently), but since Mermaid’s C4 has a fixed style, we often rely on the built-in presentation which is standardized. Clarity is further improved by grouping the main three containers inside a `System_Boundary`, visually separating our system from externals.

**Why Container Diagram?** This level is most useful for **system/design discussions**: it shows deployment units and responsibilities. For example, if someone is evaluating adding a new feature, the container view clarifies *where* a new component should live (frontend vs backend?) and how data flows. It also sets context for the deeper component diagrams: e.g. knowing that the SPA calls the API, a developer will expect to see in the component diagram how the Angular code invokes an HTTP service.

## 3. Frontend Component Level: Mapping Angular Concepts to C4 Components

The **Component diagram (C4 level 3)** is where we detail the internal architecture of a container. This section addresses the critical question: *How do we map Angular’s architectural concepts to C4 components?*

**Best-Practice Mapping:** In Angular (especially with a modern standalone-component approach), you can treat each significant **feature area or service** as a “component” in the C4 sense. A C4 *Component* is “a grouping of related functionality behind a well-defined interface” – which in an Angular app might correspond to things like:

* A **UI Component or Feature** (e.g. a page component with its child components).
* A **Service** (providing a set of related functions, often singleton injectable).
* A **State Store** or Manager (e.g. NgRx store for a feature, including its actions, reducers, effects).
* Utility components like **Route Guards** (controlling access to routes).
* In some cases, an **Angular Module** might have been a component, but since our app uses *standalone components*, modules are not in play. Instead, we use the feature directory or the primary component as the grouping.

The key is to **abstract** from Angular’s low-level details to meaningful architectural building blocks. For instance, you wouldn’t model every tiny presentational component or every Angular service – focus on those that carry distinct responsibilities in the system’s architecture. Often, each distinct **feature** (e.g., “Authentication” or “Order Management”) becomes a set of components on the diagram. Within that, the major Angular artifacts (UI, service, state) appear as components and the relationships between them show how the feature works.

**Angular to C4 Mapping Guidelines:** Based on industry practices and C4 definitions:

* **UI Components**: Treat a major UI component (or a cluster of closely related components) as a component if it represents a distinct feature or screen. In our example, `LoginComponent` (the login page UI) will be one component on the diagram.
* **Services**: Angular services (e.g., `AuthService`) are natural C4 components – they encapsulate business logic and often interact with backends or stores. We model `AuthService` as a component providing authentication logic.
* **State Management**: NgRx stores or similar state managers can be modeled as a component (or a couple of components if needed). For clarity, we can represent an **Auth Store** as one component that includes the actions, reducers, selectors, and effects for that feature. The description can mention these parts, or if we want to be very explicit, we could even model “AuthStore (State)”, “AuthEffects”, etc., as separate components – but that usually adds too much complexity. Best practice is to aggregate them unless the interactions are complex enough to need distinct depiction. For our diagrams, we will use a single `AuthStore` component with a note that it encompasses NgRx actions/effects.
* **Guards and Other Infrastructure**: An `AuthGuard` (route guard) can be shown as a component since it has a clear responsibility (protecting routes by checking auth status). It interacts with the AuthService or store to do its job.
* **External interactions**: If an Angular component or service calls an external API (like our backend’s API), that external endpoint can be represented either as another component (if within the same container boundary, e.g., calling a local in-browser module) or more commonly as a connection to another container. In our case, the Angular app calls the .NET API – in the component diagram we might depict the API as an external dependency or just note it on the relationship line for the AuthService component (since the actual API container was shown in the container diagram, here we can treat it as an external element). Mermaid allows using `Container_Ext` or `System_Ext` even in a component diagram to show something outside this container.

Let’s put this into practice with a **C4 Component diagram** for the **Login feature**. We’ll zoom in on the Angular SPA container and show how the login page works internally:

```mermaid
C4Component
    title Component Diagram - "Auth/Login" Feature (Angular SPA)
    Container_Boundary(spa, "Angular SPA") {
        Component(ui_login, "LoginComponent", "Angular Component (UI)", "Renders login form and triggers authentication")
        Component(auth_service, "AuthService", "Angular Service", "Handles authentication API calls and JWT storage")
        Component(auth_guard, "AuthGuard", "Angular Route Guard", "Protects routes by checking user auth status")
        Component(auth_store, "AuthStore (NgRx)", "State Store (NgRx)", "Manages auth state: tokens, user info. \nIncludes Actions, Effects, Reducers for login/logout")
    }
    Container_Ext(api, "Auth API Endpoint", "HTTP REST (/.NET API)", "Backend endpoint for authentication") 
    %% Relationships inside SPA:
    Rel(ui_login, auth_service, "calls validateCredentials()", "click Login")
    Rel(auth_service, api, "calls /api/auth/login", "HTTP POST")
    Rel(auth_service, auth_store, "dispatches LoginSuccess action to")
    Rel(auth_store, auth_service, "effect triggers API call via AuthService") %% illustrating effect usage
    Rel(auth_guard, auth_store, "queries auth state", "checks token")
    Rel(ui_login, auth_store, "selects auth state", "subscribes to login status")
```

Breaking down this component diagram:

* We declare a `Container_Boundary(spa, "Angular SPA")` to indicate we’re zooming into the SPA container (from the previous diagram). All defined components inside this boundary are part of the Angular application.
* **Components defined** inside:

  * **LoginComponent (`ui_login`)** – labeled as an Angular Component (UI). This represents the login page UI. Its responsibility: render the form and initiate the login process (e.g., calling a method on AuthService when the user submits).
  * **AuthService (`auth_service`)** – an Angular Service responsible for authentication logic. Notably, this service communicates with the backend API (sends credentials, handles the response) and also updates the client-side state (e.g., store or local storage).
  * **AuthGuard (`auth_guard`)** – a Route Guard that intercepts navigation to protected routes. It will check if the user is authenticated (likely by consulting AuthService or the AuthStore) and allow or redirect accordingly.
  * **AuthStore (`auth_store`)** – representing the NgRx store slice for authentication. The description explicitly notes it *includes Actions, Effects, Reducers* – conveying that this component covers the entire state management for auth. We treat it as a single component for simplicity, since all those parts work together closely to manage authentication state. (If needed, one could decompose it further, but that often complicates the diagram. A comment or note can be used if more detail is necessary, but here we inline a brief mention.)
* We also have an **external container** (`api`) for the *Auth API Endpoint*. This is effectively the backend counterpart that AuthService talks to. We denote it as `Container_Ext` (or it could be `System_Ext`) to show it’s outside the SPA’s boundary (the API lives in the .NET container). Labeling it ".NET API" clarifies the technology. We could alternatively not list it as a formal element and simply label the relationship like “calls Auth API”; however, including it as a Container\_Ext gives a anchored node for clarity.

**Relationships in the component diagram:**

* **UI -> Service:** `LoginComponent --> AuthService` labeled “calls validateCredentials()” triggered by a login button click. This indicates the component invokes a method on the service to perform login (mapping to the actual code where the component uses the service).
* **Service -> API:** `AuthService --> Auth API Endpoint` labeled as an HTTP POST. This call goes out of the SPA to the backend (this arrow corresponds to the front-end making an HTTP request). In our Mermaid code, we used `Rel(auth_service, api, "calls /api/auth/login", "HTTP POST")` to make this explicit. (Note: we use a shorthand label "/api/auth/login" to identify the endpoint).
* **Service -> Store:** `AuthService --> AuthStore` with “dispatches LoginSuccess action”. This represents that after a successful login response, the AuthService might dispatch an NgRx action (e.g., to update global state). The direction here is AuthService *to* AuthStore (meaning it triggers a state change).
* **Store -> Service (Effect):** We also have `AuthStore --> AuthService` labeled “effect triggers API call via AuthService”. This arrow is a bit abstract – it’s illustrating that one of the NgRx Effects within the AuthStore uses AuthService to call the API as well (for example, an effect listening for a LoginRequested action then invokes AuthService). This creates a cyclic relationship on the diagram (AuthService <-> AuthStore), which is actually what happens in practice: the service dispatches state changes, and the state’s effects call the service. Showing this helps to convey the pattern of how NgRx Effects work with the service. If this bi-directional interaction looks visually confusing, an alternative is to not draw the effect arrow and instead describe it in a note. However, including it makes the role of effects explicit. We used a dashed relationship style (`Rel` yields a solid line by default; Mermaid C4 doesn’t have a built-in dashed variant for Rel, but one could stylistically differentiate if needed).
* **Guard -> Store:** `AuthGuard --> AuthStore` labeled “queries auth state”. This shows the guard checking if the user is logged in by looking at the store (or it could call AuthService which in turn checks the store). Either way, the guard’s decision to allow navigation is based on auth state, so we depict that dependency.
* **UI -> Store:** `LoginComponent --> AuthStore` labeled “selects auth state”. This indicates the component might subscribe to the store (e.g., using a selector to get loading/error state or to react to login success/failure). In Angular, the component could directly select from NgRx store for say error messages or login status to react in the UI. This arrow shows that read dependency.

The result is a detailed *structure* of the login feature. An engineer can see that the **login flow** starts at the UI, goes through the service to the backend, updates the store, and that the guard and UI both pay attention to the store for authentication status. This is exactly the kind of insight a new team member or an AI assistant needs to navigate the code. It maps code concepts (component, service, guard, store) to architecture roles clearly.

**Industry Best Practices Reflected:** Notice how each Angular concept is treated as a first-class citizen in the architecture:

* Angular’s distinctive building blocks (components, services, etc.) are represented as components with their roles explained. This aligns with guidance that *technology-specific concepts* like “Angular Service” or “Spring Controller” can be considered components in C4, as long as they have a distinct responsibility.
* We avoid overwhelming detail: We didn’t show, for example, every Angular component or every NgRx action name – that would clutter the diagram. Instead we summarize (“AuthStore includes Actions, Effects…”). Aim for **one node per major role**, not per class file. In practice, our AuthService might consist of multiple TS files (maybe an `auth.api.service.ts` and an `auth.facade.ts`, etc.), but architecturally we consider it one service component.
* We maintain accuracy: The diagram should be kept consistent with actual code. If, say, we introduce a new `RegistrationService` or a new NgRx feature for refresh tokens, we must update this diagram accordingly. This echoes our **“Accuracy First”** principle: the depicted components and connections must match reality.

**Standalone Components & NgModules:** Since our app exclusively uses standalone components (`standalone: true` on all components, directives, pipes), there are no Angular NgModule boundaries to represent. If we had modules, one might depict a module as a grouping or a component of its own. Instead, our grouping unit in documentation is the **feature directory** (as per Nx conventions, e.g. a library or `features/auth/` folder). We should ensure each such directory has its own README and diagram (see next section). The standalone component approach makes the diagrams simpler – we focus on components and services themselves rather than module initialization or injection intricacies (which would be more relevant in an older Angular architecture with modules).

## 4. Embedding Diagrams in our Documentation Structure

With the Mermaid C4 diagrams defined, we need to integrate them into our documentation per our standards. Our `DocumentationStandards.md` (and current `DiagrammingStandards.md`) already mandate that diagrams reside alongside the code they describe – essentially treating diagrams as living documentation that evolves with the code. Here we refine those guidelines for **placing C4 diagrams at the appropriate locations, versioning them, and using tools to manage them.**

**Per-Directory Placement:** The best practice is to place each diagram at the highest level where its context is relevant, typically in the nearest `README.md`:

* **System Context & Top-Level Container Diagrams:** For our project, a context or container diagram that shows the entire architecture should live in a top-level README. This could be the repository’s main README or a root documentation file (for example, `Code/Zarichney.Website/README.md` if that is the root of the web application). Since our monorepo contains both frontend and backend, a container diagram covering both naturally fits in a README that spans the whole system (or at least the whole product). The **Context diagram** might be placed in an even higher-level overview document (if one exists, like an architecture overview in `/Docs/Architecture.md`), but it can also be included in the root README for visibility. In summary, the root-level documentation should give any newcomer a birds-eye view (context) and the main containers (container diagram) of the system.
* **Component (and Lower-Level) Diagrams:** These should be embedded in the README of the specific feature, module, or subsystem they document. For example, the **Auth/Login component diagram** we created belongs in `src/app/features/auth/README.md` (assuming that directory contains the auth feature code). That README would include a section (Architecture or Design) where the Mermaid diagram code is inserted. This way, anyone browsing that feature’s folder sees up-to-date architecture for that feature. Likewise, if we had a component diagram for the backend’s internal structure (say, showing components inside the .NET Web API like controllers, services, etc.), it should live in the backend project’s README (e.g., `Code/Zarichney.WebApi/README.md` or similar). In an Nx monorepo, where we might have libraries for each domain (e.g., an `auth` library for front-end and an `auth` area in back-end), each library/project should have its own README with relevant diagrams.
* **Cross-linking:** It’s helpful to link between these diagrams/documents. For instance, the top-level container diagram’s nodes (like the Angular SPA box) can have references or at least text telling the reader “(See `features/auth/README.md` for internals of the Auth component)”. We can’t hyperlink directly from shapes in Mermaid, but we can surround the diagram with explanatory text that includes links. Our Documentation Standards already encourage that READMEs in subfolders be referenced from higher-level docs. Continue this practice: e.g., in the root README, after the container diagram, have a bullet list: “**Detailed Component Diagrams:** [Auth Feature](src/app/features/auth/README.md), [Payments Feature](src/app/features/payments/README.md), …”. This creates a navigable documentation system.

**Versioning and Evolution of Diagrams:** We treat all diagrams as code, so they are subject to version control. Some specific guidelines to reinforce:

* **Update diagrams in the same commit as code changes** that affect them. This is already mandated (our AI Coder and human developers must update diagrams on any architecture-impacting change). For example, if a new service is added to the Auth feature, update the Auth component diagram at the same time as the code.
* **Use the `Last Updated` field** in the diagram’s containing document (usually the README) to track when the diagram was last modified. Our current standard is to update the README’s front-matter “Last Updated: YYYY-MM-DD” whenever any part of it (including diagrams) changes. Continue this practice diligently. This gives readers a quick clue about diagram staleness.
* **When to create a new diagram vs update an existing one:** In general, prefer updating an existing diagram to reflect changes, so that the history of that diagram can be tracked via git. However, if an architecture undergoes a **major revision** such that the old diagram and new diagram would be radically different, it can be useful to preserve the old diagram for reference. In such cases, consider moving the old diagram code to an appendix in the README or to a version-tagged file (for example, create `architecture_v1.mmd` for the old version) with a note that it’s deprecated. Then update the main diagram to the new architecture. This way historical architecture is not lost but clearly separated. Always indicate in text when a diagram represents a newer version of the system, especially if some components are legacy/removed.
* **Deprecating diagrams:** If a diagram becomes completely irrelevant (e.g., feature removed), you should remove it from the README (and perhaps archive the definition in a `docs/old_diagrams/` folder if needed). Keeping obsolete diagrams in primary documentation can confuse readers and AI agents. Instead, have a changelog note or commit history to find old diagrams if necessary.
* **Diagram version numbering:** We generally rely on the README version and last-updated date rather than versioning each diagram image. If needed, the diagram’s title can include a version or date (for example, “Container Diagram (v2.0, 2025)” in the title) for explicit clarity, but this is optional since git history already provides version tracking.

**Tooling Recommendations (Creation & Verification):** We want our team (human and AI) to easily create and view Mermaid diagrams:

* **VS Code Integration:** We endorse using VS Code Mermaid preview extensions for editing diagrams. For example, the *“Markdown Preview Mermaid Support”* extension provides live previews of Mermaid diagrams within the Markdown preview pane. This is extremely useful to catch syntax errors or layout issues as you write Mermaid code. Another extension, *“Mermaid Editor”*, can offer a side-by-side view and syntax highlighting. Encourage developers to install these.
* **Mermaid Live Editor:** The online **Mermaid Live Editor** (`https://mermaid.live`) is our go-to source of truth for validating Mermaid syntax. Developers and AI agents should paste their diagram code there to ensure it renders correctly. The Live Editor often provides error messages pinpointing syntax issues (for instance, an unclosed quote or a missing brace). It’s also useful for quick experimentation with layout or new diagram types. Because Mermaid’s GitHub rendering can sometimes lag a version behind or have slight differences, the Live Editor helps isolate whether an issue is with our code or with GitHub. (Note: use the **Mermaid version selector** in the Live Editor to match the version that GitHub uses, if known – currently GitHub uses Mermaid v10.x as of mid-2025.)
* **GitHub Rendering:** Remember that **GitHub natively renders Mermaid diagrams** in Markdown as of 2022. This means our README-embedded diagrams will show up automatically on GitHub without needing to check in rendered images. However, not all Mermaid features or newer syntax are immediately supported (since GitHub’s renderer might not always be the latest Mermaid release). We must verify that our diagrams **display correctly on GitHub** – this is our primary target environment for documentation. Practically, that means after pushing changes, view the README on GitHub and ensure the diagrams appear with correct formatting. If a diagram fails to render on GitHub (e.g., due to an experimental C4 syntax quirk), consider simplifying the diagram or using a supported alternative syntax. As a fallback, our standards allow using a static image (exported via the Mermaid CLI or Live Editor) if a particularly complex diagram just won’t render correctly on GitHub. Only do this as a last resort, and document clearly that the image is an export of Mermaid code (and keep the .mmd source file in version control alongside it).
* **Mermaid CLI (`mmdc`) and Automation:** We have the Mermaid CLI available for anyone who wants to generate PNG/SVG files from Mermaid definitions. This can be used in CI pipelines to catch syntax errors (non-zero exit code if Mermaid parse fails) or to produce images for inclusion in external docs or slide decks. We recommend using `mmdc` in a pre-commit hook or CI step for complex diagrams to ensure they render (especially if using the experimental C4 syntax, which might not throw errors on a subtle layout issue until viewed). Incorporate this into our workflow as needed – e.g., a script can scan all `README.md` files for \`\`\`mermaid blocks and try to parse them.
* **Consistency in Diagram Style:** We continue to use our standard Mermaid styling defined in `DiagrammingStandards.md` Section 7 (the `classDef` styles for things like *database*, *externalApi*, *service*, *ui*, etc.). With C4 diagrams, Mermaid auto-styles elements (for example, external systems might appear with a dashed border). We should see if our custom styles need adjusting for C4 diagrams or if we let the default stand. If we want, we can still apply `UpdateElementStyle` in Mermaid C4 to tweak colors, but this might be unnecessary. The key is that across all diagrams – C4 or flowchart or sequence – we maintain a **consistent legend and color scheme** so that, for instance, a database is always depicted similarly (Mermaid’s default for `ContainerDb` is a cylinder icon which is clear). We should document any such styling decisions in the standards. Given Mermaid’s experimental state, we might not heavily customize C4 visuals yet; clarity and rendering stability take priority.

**Documentation Example – Putting it together:**

* In `Code/Zarichney.Website/README.md` (top-level), we would have a section (perhaps titled “**Architecture Overview**”) containing the context diagram and container diagram from Sections 1 and 2 above. The text would explain the overall system and mention each container briefly.
* In `src/app/features/auth/README.md`, we would include the component diagram from Section 3 in a section (e.g., “**Auth Feature Architecture**”). The README would describe the login/auth flow in words and the Mermaid diagram would visualize it. We’d also include a link back to the higher-level diagram (e.g., “See the [container diagram](../../README.md) for how this fits into the overall system”).
* We ensure each README has the metadata at the top (version number of the doc, last updated date, etc.). For example, after updating the Auth README with the diagram, update “Last Updated” to today’s date. This signals to everyone (and AI) that the diagram is current as of that date.
* The **DiagrammingStandards.md** itself will be updated to reflect these new practices: listing C4 diagrams as an approved type (which it already does), giving a brief summary of how to use them, and referencing the examples we’ve crafted. It will also incorporate the versioning/tooling points above, reinforcing that diagrams must be maintained just like code. Mention GitHub support and VSCode tools explicitly to encourage their use for previewing Mermaid diagrams.

In conclusion, by integrating the C4 model with Mermaid, we get the best of both worlds: a clear, industry-standard way to visualize architecture (C4) and the convenience and consistency of Markdown-based diagrams as code (Mermaid). Following these guidelines, our updated `DiagrammingStandards.md` will enable both human developers and AI agents to produce and understand diagrams that accurately reflect our Angular front-end and .NET backend architecture – **from high-level context down to detailed components** – in a way that is consistent, up-to-date, and easy to maintain.

**Sources:**

* Simon Brown, *"The C4 Model for Software Architecture"* – InfoQ (overview of C4 levels and benefits)
* Mermaid Official Docs – *C4 Diagram Syntax* (examples of C4Context, C4Container, C4Component usage)
* Archipeg Documentation – *C4 Model Metamodel* (definition of Containers, Components, and technology-specific concepts like Angular Service)
* Microsoft Azure Architecture Guide – *Diagramming Practices* (importance of metadata like last updated, and clarity in diagrams)
* GitHub Blog – *"Include diagrams in Markdown with Mermaid"* (confirmation of GitHub’s native Mermaid rendering support)
