# **SSR\_Standard.md: Architectural Standard for Hybrid Static & Dynamic Server-Side Rendering in Angular**

## **Section 1: Foundational Principles of the Hybrid Rendering Standard**

### **1.1 Introduction to the Dual-Mode Architecture: SSG vs. SSR**

Modern web applications demand a sophisticated approach to rendering that balances performance, search engine optimization (SEO), and user experience. A monolithic rendering strategy is no longer sufficient. This standard establishes a hybrid, dual-mode architectural pattern for Angular applications, leveraging the distinct strengths of two primary server-rendering techniques: Static Site Generation (SSG) and dynamic Server-Side Rendering (SSR).  
The core of this architecture is the ability to select the optimal rendering mode on a per-route basis, ensuring that each part of the application is delivered to the user in the most efficient way possible. The two modes are formally defined as follows:

* **Static Site Generation (SSG / Pre-rendering):** This is the process of rendering application pages into complete, static HTML files during the application's *build time*. These generated files, along with their required JavaScript and CSS assets, are then deployed to a static hosting service, typically a Content Delivery Network (CDN). When a user or search crawler requests a pre-rendered route, the server responds instantly with the static HTML file without any need for on-the-fly computation. This method yields the fastest possible Time to First Byte (TTFB) and First Contentful Paint (FCP), making it ideal for content that is public and changes infrequently.  
* **Dynamic Server-Side Rendering (SSR):** This is the process of rendering an application page on a server *at request time*. When a request is received, a server environment (typically running Node.js) executes the Angular application, fetches any necessary dynamic data, and generates a fully-formed HTML page tailored to that specific request. This page is then sent to the browser. SSR is essential for pages that display personalized, real-time, or frequently changing content that cannot be known at build time.

By strategically combining these two modes, an application can achieve the unparalleled speed of SSG for its static content (like marketing pages and blog posts) while retaining the flexibility and dynamism of SSR for its interactive, user-specific sections (like dashboards and search results). This hybrid model represents the pinnacle of performance and capability for modern Angular applications.

### **1.2 Core Tenets: Performance, SEO, and AI-Coder Compatibility**

This architectural standard is founded upon three non-negotiable tenets that must guide all implementation decisions. Adherence to these principles ensures the development of robust, high-quality applications that meet both user and business objectives.

* **Performance as the Primary Mandate:** The ultimate goal of this hybrid architecture is to maximize user-perceived performance. All architectural decisions must be optimized for Google's Core Web Vitals (CWV). Specifically, this standard aims to achieve:  
  * **Fast First Contentful Paint (FCP) and Largest Contentful Paint (LCP):** By delivering pre-rendered HTML (via SSG or SSR), the browser can paint meaningful content on the screen almost immediately, drastically improving the loading experience over traditional Client-Side Rendering (CSR).  
  * **Low Time to Interactive (TTI):** Through modern techniques like non-destructive and incremental hydration, the application becomes interactive quickly, allowing users to engage with the content without frustrating delays.  
  * **Minimal Cumulative Layout Shift (CLS):** By ensuring a stable handoff from the server-rendered view to the client-hydrated application, this standard prevents the jarring layout shifts that degrade user experience.  
* **Unyielding Commitment to SEO:** Every public-facing page within the application must be fully and accurately crawlable, parsable, and indexable by all major search engine bots. Both SSG and SSR fulfill this requirement by delivering a complete HTML document in the initial server response, a stark contrast to the empty HTML shell of a pure CSR application. This ensures that all content is visible to crawlers, leading to maximum search visibility and ranking potential.  
* **Unambiguous Compatibility with AI Coders:** The rules, patterns, and decision frameworks defined in this standard are designed with a level of precision and clarity sufficient for interpretation and implementation by an advanced AI coding agent. This necessitates a move away from subjective guidelines and toward prescriptive, deterministic rules. The objective is to create a system where architectural choices are the logical outcome of applying a clear set of criteria, thereby ensuring consistency, reducing ambiguity, and enabling a high degree of automation in the development process.

### **1.3 The Rendering Spectrum and the Evolution of Angular Universal**

The hybrid model exists on a spectrum of rendering strategies. Understanding this spectrum provides context for the architectural choices mandated by this standard.

* **Client-Side Rendering (CSR):** The traditional model for Single-Page Applications (SPAs). The server delivers a minimal index.html file, and the browser must download, parse, and execute a large JavaScript bundle to render the application. While this provides a highly fluid, app-like experience after the initial load, it suffers from a slow FCP (the "blank page" problem) and presents significant challenges for SEO, as many crawlers struggle with executing complex JavaScript. CSR remains a viable choice only for applications that are not public-facing, such as internal admin dashboards or tools behind a strict authentication wall.  
* **Server-Side Rendering (SSR):** Solves the FCP and SEO problems of CSR by rendering the initial view on the server.  
* **Static Site Generation (SSG):** Takes the performance of SSR a step further by moving the rendering process to build time, eliminating per-request server computation entirely for the fastest possible response.

The evolution of Angular's tooling reflects a maturation of its rendering strategy. The framework has moved from an add-on approach to a deeply integrated, first-class feature. Early implementations relied on the @nguniversal/express-engine package, which required developers to manually wire up an Express server and manage the rendering process. This often led to complex and brittle configurations.  
With recent versions (v17 and later), this has been superseded by the @angular/ssr package. This is not merely a rebranding; it represents a fundamental architectural shift. The ng add @angular/ssr command now seamlessly integrates hybrid rendering capabilities into the Angular CLI and project structure. The most significant advancement is the introduction of a dedicated server routing configuration file, app.routes.server.ts. This file provides a declarative, type-safe API for specifying the RenderMode on a per-route basis, centralizing the rendering logic and making it far more intelligible for both human and AI developers. This standard is built exclusively upon these modern, integrated APIs. Adopting older patterns is considered a deviation from this standard, as it introduces unnecessary complexity and fails to leverage the clarity and power of the current framework design.

## **Section 2: The Definitive Decision Framework for Route Rendering**

### **2.1 Rule-Based Logic for Rendering Mode Selection**

The selection of a rendering mode for any given route within the application shall not be a matter of developer discretion or preference. It must be a deterministic outcome derived from applying a strict, rule-based framework. This approach is paramount to achieving the core tenet of AI-coder compatibility, as it transforms a complex architectural decision into a predictable, logical process. An AI agent can be programmed to analyze the characteristics of a new or existing route against this framework and select the appropriate RenderMode without human intervention.

### **2.2 The Decision Matrix**

The central tool for implementing this rule-based logic is the Route Rendering Mode Decision Matrix. This matrix maps the key characteristics of a route to the mandated rendering strategy. For any given route, its properties must be evaluated against the criteria in this table to determine its rendering mode.  
**Table 2.1: Route Rendering Mode Decision Matrix**

| Decision Criterion | Rule for Pre-render (SSG) | Rule for Dynamic Render (SSR) | Rule for Client Render (CSR) |
| :---- | :---- | :---- | :---- |
| **Data Freshness & Volatility** | Content is static or changes infrequently (e.g., blog posts, about us, legal pages, product documentation). Data is considered valid for the entire duration between application builds. | Content is highly dynamic, real-time, or changes with every request (e.g., stock prices, live sports scores, personalized user recommendations). | Content is managed entirely by client-side state after an initial page load, often in response to user interactions within a single, complex view. |
| **User-Specific Content** | Page content is **identical** for all users. No personalization is present in the initial HTML served from the build artifact. | Page content is **personalized** for the specific user making the request (e.g., a user's profile page, order history, customized dashboard). | The entire view is a user-specific workspace that is inaccessible to the public and is loaded only after successful authentication (e.g., account settings page, admin panel). |
| **SEO Criticality** | **High.** The page is a primary public-facing entry point intended to rank in search engines (e.g., home page, marketing landing pages, articles, product pages). | **High.** The page is public and must be indexed, but its content is generated from dynamic parameters that cannot all be known at build time (e.g., product search results, user-generated content pages). | **Low/None.** The page is behind an authentication wall, contains no content valuable for public search indexing, or is explicitly meant to be excluded from search engines. |
| **Data Source at Render Time** | All data required to render the page is available at **build time**. This may involve build-time scripts fetching content from a headless CMS, database, or API. | Data must be fetched from a database or API at **request time**, often using information from the user's incoming request (e.g., authentication tokens in cookies, query parameters). | Data is fetched from the browser **after** the initial page has loaded and the core Angular application has bootstrapped and hydrated. |
| **Primary Performance Goal** | **Lowest TTFB/FCP.** The primary goal is the absolute fastest delivery of static content, served directly from a CDN edge location to minimize network latency. | **Fast FCP with dynamic data.** The primary goal is to provide a fast initial paint of a fully-formed, personalized page, accepting the minor latency of server-side computation as a necessary trade-off for data freshness. | **Highest interactivity and fastest subsequent navigation.** The primary goal is a fluid, app-like experience after the initial load, prioritizing in-app responsiveness over initial paint time. |

The application of this matrix is mandatory. For every route defined in the application's routing module, a corresponding entry must exist in the server routing configuration that reflects the outcome of this decision process.

### **2.3 The "ISR Pattern": SSG with SSR Fallback for Scalability**

While the Angular framework does not have an explicit configuration for Incremental Static Regeneration (ISR) akin to other frameworks, the combination of its hybrid rendering features enables a powerful and functionally equivalent pattern. This "ISR Pattern" is the standard approach for routes that represent large, growing collections of content, such as e-commerce product catalogs or blogs. It elegantly solves the primary limitation of pure SSG (the need for a full rebuild to add new content) and the primary limitation of pure SSR (the performance cost of rendering every request on the server).  
The pattern is achieved through a specific combination of configuration and infrastructure:

1. **Pre-render with a Server Fallback:** In app.routes.server.ts, a parameterized route (e.g., /products/:id) is configured with renderMode: RenderMode.Prerender. Crucially, the fallback property is set to 'server' (which is the default behavior).  
2. **Build-Time Generation:** During the build process, getPrerenderParams is used to fetch all *known* product IDs and generate static HTML files for them.  
3. **Request-Time Logic:** When a user requests a URL:  
   * If the URL corresponds to a pre-existing static file (e.g., /products/123), the CDN or static host serves it instantly.  
   * If the URL corresponds to a newly added product for which no static file exists (e.g., /products/456), the request misses the static host and is routed to the Node.js server.  
   * Because of the fallback: 'server' configuration, the Node.js server will render this single page on-demand using SSR.  
4. **Server-Side Caching:** The SSR response for this new page is then cached at a server or CDN level with an appropriate Time-to-Live (TTL).  
5. **Subsequent Requests:** The next user who requests /products/456 will receive the fast, cached static version from the cache layer.

This sequence—SSG for known content, SSR for the first-ever request of new content, and caching for all subsequent requests—effectively regenerates static content on-demand without requiring a full application rebuild. It is the mandated strategy for scaling sites with large volumes of semi-static content.

## **Section 3: Implementation Standard: Configuration and Routing**

The implementation of the hybrid rendering strategy must be centralized and declarative to ensure clarity and maintainability. This section defines the specific file and code patterns required to configure routing according to the decision framework.

### **3.1 Standardizing app.routes.server.ts: The Single Source of Truth**

The file src/app/routes.server.ts is designated as the **single source of truth** for defining the rendering strategy of all application routes. This file provides a clean separation between client-side routing logic and server-side rendering concerns, a key improvement in modern Angular SSR.  
All per-route rendering mode decisions derived from the matrix in Section 2 must be implemented here. The use of older mechanisms for defining pre-rendering scope, such as the routes array or routesFile property within the prerender builder in angular.json, is explicitly deprecated by this standard. While the prerender target in angular.json is still required to *initiate* the pre-rendering process during the build, it must not be used to define *which* routes are pre-rendered. That logic belongs exclusively in app.routes.server.ts through the use of RenderMode.Prerender and the getPrerenderParams function. This centralization is critical for AI-coder parsing and automated validation of the rendering architecture.

### **3.2 Code Pattern: Implementing RenderMode.Prerender (SSG)**

For routes identified as static, public, and non-personalized, the RenderMode.Prerender flag must be used. This instructs the Angular build process to generate a static HTML file for the specified path.

#### **Canonical Example for Static Routes:**

`// src/app/routes.server.ts`  
`import { RenderMode, ServerRoute } from '@angular/ssr';`

`export const serverRoutes: ServerRoute =;`

This declarative pattern directly translates the decision from the framework into a clear, machine-readable configuration.

### **3.3 Code Pattern: Implementing RenderMode.Server (SSR)**

For routes identified as dynamic, personalized, or requiring real-time data, the RenderMode.Server flag must be used. This ensures that the route is rendered on the Node.js server for every incoming request.

#### **Canonical Example for Dynamic Routes:**

`// src/app/routes.server.ts`  
`import { RenderMode, ServerRoute } from '@angular/ssr';`

`export const serverRoutes: ServerRoute =;`

This configuration explicitly designates routes that require the full power and context of on-demand server computation.

### **3.4 Standard for Dynamic Pre-rendering: getPrerenderParams**

For parameterized routes that are designated for SSG (e.g., /blog/:slug, /products/:id), the getPrerenderParams function is the mandatory mechanism for discovering the specific routes to be generated at build time. This function provides a powerful, data-driven approach that decouples the build process from static configuration files.  
The getPrerenderParams function runs within an Angular injection context during the build, which allows it to use standard dependency injection to inject services. This is a profound improvement over previous methods, as it enables the build process to become self-contained and data-aware. A service can be created to fetch all necessary IDs or slugs from a headless CMS, database, or other API. This service is then injected and called within getPrerenderParams to programmatically determine the full list of pages to pre-render. This eliminates the need for brittle, external scripts that generate routes.txt files.

#### **Canonical Code Pattern for Parameterized SSG Routes:**

This example demonstrates the standard pattern for pre-rendering a collection of blog posts, including the implementation of the ISR pattern via the fallback property.  
`// src/app/services/content-api.service.ts`  
`import { Injectable, inject } from '@angular/core';`  
`import { HttpClient } from '@angular/common/http';`  
`import { Observable } from 'rxjs';`  
`import { map } from 'rxjs/operators';`

`@Injectable({ providedIn: 'root' })`  
`export class ContentApiService {`  
  `private http = inject(HttpClient);`

  `// In a real app, this would fetch from a CMS or database.`  
  `getAllBlogPostSlugs(): Observable<string> {`  
    `return this.http.get<{ posts: { slug: string } }>('https://api.my-cms.com/posts')`  
     `.pipe(map(response => response.posts.map(post => post.slug)));`  
  `}`  
`}`

`// src/app/routes.server.ts`  
`import { inject } from '@angular/core';`  
`import { RenderMode, ServerRoute } from '@angular/ssr';`  
`import { ContentApiService } from './services/content-api.service';`  
`import { firstValueFrom } from 'rxjs';`

`export const serverRoutes: ServerRoute =;`

## **Section 4: Implementation Standard: Universal Component Authoring**

Creating components that function correctly and performantly in both a server (Node.js) and browser environment requires adherence to a strict set of rules. A "universal" component must be platform-agnostic, handling the absence of browser-specific APIs on the server and ensuring a seamless transition to the client.

### **4.1 Rule \#1: Isolate Platform-Specific Code with isPlatformBrowser**

Any component code that directly references browser-only global objects—such as window, document, localStorage, sessionStorage, navigator, or IntersectionObserver—is guaranteed to throw a ReferenceError when executed on the server. Therefore, it is mandatory to guard all access to such APIs.  
The standard mechanism for this is to inject the PLATFORM\_ID token and use the isPlatformBrowser function from @angular/common. This check must wrap any code that depends on a browser environment.

#### **Canonical Code Pattern for Platform Guarding:**

`import { Component, Inject, PLATFORM_ID, OnInit } from '@angular/core';`  
`import { isPlatformBrowser } from '@angular/common';`

`@Component({`  
  `selector: 'app-local-storage-widget',`  
  `` template: `<p>Saved value: {{ savedValue }}</p>` ``  
`})`  
`export class LocalStorageWidgetComponent implements OnInit {`  
  `isBrowser: boolean;`  
  `savedValue: string = 'N/A (Server Render)';`

  `constructor(@Inject(PLATFORM_ID) private platformId: object) {`  
    `// It is best practice to determine the platform once in the constructor.`  
    `this.isBrowser = isPlatformBrowser(this.platformId);`  
  `}`

  `ngOnInit(): void {`  
    `// RULE: All access to browser-specific APIs MUST be inside a platform check.`  
    `if (this.isBrowser) {`  
      `this.savedValue = localStorage.getItem('my-app-key') |`

`| 'No value found';`  
    `}`  
  `}`  
`}`

Failure to adhere to this rule is a critical error that will break server-side rendering for any route containing the non-compliant component.

### **4.2 Rule \#2: Defer Browser-Only DOM Interaction with afterNextRender**

Simply wrapping DOM manipulation code in an isPlatformBrowser check is an insufficient and detrimental anti-pattern. While it prevents server-side crashes, executing DOM-altering logic (e.g., initializing a third-party charting library, measuring element dimensions, or directly manipulating nativeElement) within standard lifecycle hooks like ngOnInit or ngAfterViewInit interferes with Angular's non-destructive hydration process.  
Non-destructive hydration works by comparing the server-rendered DOM tree with the client-side component structure and reusing the existing DOM nodes. If a component immediately modifies its DOM upon initialization, it creates a mismatch between the server's version and the client's version. This can break the hydration process, leading to Angular destroying and re-rendering the component's DOM, which manifests as a visible "flicker" and negatively impacts the Cumulative Layout Shift (CLS) Core Web Vital.  
To solve this, the standard mandates the use of the afterNextRender hook. This function schedules a callback to run *only on the browser* and *after* the current render and hydration cycle is complete. This guarantees that any DOM manipulation occurs on a stable, hydrated application, preventing flicker and ensuring a smooth user experience.

#### **Canonical Code Pattern for Safe DOM Initialization:**

`import { Component, Inject, PLATFORM_ID, afterNextRender, ElementRef, viewChild } from '@angular/core';`  
`import { isPlatformBrowser } from '@angular/common';`  
`import * as ChartJs from 'chart.js'; // A hypothetical third-party library`

`@Component({`  
  `selector: 'app-chart-widget',`  
  `` template: `<canvas #chartCanvas></canvas>` ``  
`})`  
`export class ChartWidgetComponent {`  
  `chartCanvas = viewChild<ElementRef<HTMLCanvasElement>>('chartCanvas');`

  `constructor(`  
    `@Inject(PLATFORM_ID) private platformId: object`  
  `) {`  
    `// RULE: All browser-only DOM manipulation MUST be deferred until after hydration.`  
    `if (isPlatformBrowser(this.platformId)) {`  
      `afterNextRender(() => {`  
        `const canvas = this.chartCanvas()?.nativeElement;`  
        `if (canvas) {`  
          `// This code is guaranteed to run on the browser AFTER the component`  
          `// has been fully rendered and hydrated, preventing any flicker.`  
          `new ChartJs.Chart(canvas, {`  
            `type: 'bar',`  
            `data: { /*... chart data... */ }`  
          `});`  
        `}`  
      `});`  
    `}`  
  `}`  
`}`

### **4.3 Rule \#3: Eliminate Duplicate API Calls with TransferState**

A critical performance issue in SSR applications is the re-fetching of data. An API call made on the server to render a page will be wastefully repeated by the client when the application bootstraps, as the client has no knowledge of the data the server already fetched. This doubles the load on backend services and can cause content flicker as the client-side data replaces the server-rendered content.  
To prevent this, it is mandatory to use Angular's TransferState API. This mechanism allows the server to serialize data fetched during rendering and embed it within the initial HTML response. The client-side application then reads this state during hydration, making the data available immediately without needing to make a redundant API call.  
For standard HttpClient GET requests, this process is automated by providing withHttpTransferCacheOptions() alongside provideClientHydration(). This should be the default configuration.

#### **Configuration for Automated HTTP Transfer:**

`// src/app/app.config.ts`  
`import { ApplicationConfig } from '@angular/core';`  
`import { provideRouter } from '@angular/router';`  
`import { routes } from './app.routes';`  
`import { provideClientHydration, withHttpTransferCacheOptions } from '@angular/platform-browser';`  
`import { provideHttpClient } from '@angular/common/http';`

`export const appConfig: ApplicationConfig = {`  
  `providers:`  
`};`

For data that is not fetched via HttpClient GET requests (e.g., from a GraphQL client, or data derived from the server environment), the manual TransferState pattern must be used. This involves creating a unique StateKey, setting the data on the server, and checking for/consuming the data on the client.

#### **Canonical Code Pattern for Manual TransferState:**

`// src/app/services/product.service.ts`  
`import { Injectable, Inject, PLATFORM_ID } from '@angular/core';`  
`import { TransferState, makeStateKey, StateKey } from '@angular/platform-browser';`  
`import { isPlatformServer } from '@angular/common';`  
`import { HttpClient } from '@angular/common/http';`  
`import { Observable, of } from 'rxjs';`  
`import { tap } from 'rxjs/operators';`

`interface Product { id: string; name: string; }`  
`const PRODUCT_STATE_KEY = makeStateKey<Product>('productData');`

`@Injectable({ providedIn: 'root' })`  
`export class ProductService {`  
  `constructor(`  
    `private transferState: TransferState,`  
    `@Inject(PLATFORM_ID) private platformId: object,`  
    `private http: HttpClient`  
  `) {}`

  `getProduct(id: string): Observable<Product> {`  
    `// Check if the state was transferred from the server.`  
    `if (this.transferState.hasKey(PRODUCT_STATE_KEY)) {`  
      `const product = this.transferState.get<Product>(PRODUCT_STATE_KEY, null);`  
      `this.transferState.remove(PRODUCT_STATE_KEY); // Consume the state once to prevent reuse.`  
      `return of(product!);`  
    `} else {`  
      `// If no transferred state, fetch from the API.`  
      ``return this.http.get<Product>(`/api/products/${id}`).pipe(``  
        `tap(product => {`  
          `// If running on the server, set the state for transfer to the client.`  
          `if (isPlatformServer(this.platformId)) {`  
            `this.transferState.set(PRODUCT_STATE_KEY, product);`  
          `}`  
        `})`  
      `);`  
    `}`  
  `}`  
`}`

### **4.4 Component-Level Hydration Control: hydrate on, hydrate when, hydrate never**

Modern Angular (v19+) introduces incremental hydration, a powerful performance tuning mechanism that builds upon the @defer block. This allows for fine-grained control over which components get hydrated and when, enabling developers to prioritize interactivity for critical UI elements and improve TTI.  
The following rules apply for AI coder implementation:

* **hydrate: false (or hydrate never in older RFCs):** This attribute should be applied to components that are rendered on the server but require zero client-side interactivity. This is a powerful optimization that prevents unnecessary JavaScript from being associated with purely static content. A common use case is a site footer or a simple, non-interactive banner.  
* **@defer (on \<trigger\>) with hydration:** For components that are non-critical or appear below-the-fold, they should be wrapped in a @defer block. This not only lazy-loads the component's code but also defers its hydration until the specified trigger (e.g., on viewport) is met. This significantly reduces the initial JavaScript execution cost and improves TTI.

## **Section 5: Architectural Strategy for Performance and UX**

Beyond individual component authoring, a successful hybrid rendering strategy depends on a cohesive architectural approach to performance and user experience. This section outlines the high-level strategies that are mandatory for all applications following this standard.

### **5.1 Mandating Non-Destructive Hydration via provideClientHydration()**

The single most impactful feature for ensuring a high-quality user experience in an Angular SSR application is non-destructive hydration. Historically, Angular Universal applications suffered from a "flicker" effect where the server-rendered HTML would be displayed briefly, only to be destroyed and re-rendered by the client-side application once it bootstrapped. This caused a poor CLS score and a jarring visual experience.  
Since Angular v16, non-destructive hydration has been introduced and stabilized. When enabled, Angular attempts to reuse the server-rendered DOM, attaching event listeners and creating its internal data structures without discarding the existing HTML. This eliminates the flicker entirely and significantly improves LCP and CLS metrics.  
Therefore, it is a top-level requirement of this standard that all applications **must** enable non-destructive hydration. This is achieved by including provideClientHydration() in the providers array of the main application configuration (app.config.ts for standalone applications).

#### **Canonical Configuration:**

`// src/app/app.config.ts`  
`import { ApplicationConfig } from '@angular/core';`  
`import { provideRouter } from '@angular/router';`  
`import { routes } from './app.routes';`  
`import { provideClientHydration } from '@angular/platform-browser';`

`export const appConfig: ApplicationConfig = {`  
  `providers:`  
`};`

### **5.2 The App Shell Pattern: Ensuring a Zero-Flicker Experience**

The App Shell pattern is a complementary strategy to SSR/SSG that focuses on optimizing the initial paint of the application's static frame. The App Shell is the minimal HTML, CSS, and JavaScript required to power the user interface's skeleton—the header, navigation, and overall layout—but without any of the dynamic content.  
This pattern works in concert with hybrid rendering to create an optimal perceived loading sequence:

1. **Instant Shell Paint:** The ng build process, when configured with an App Shell, generates an index.html file that has the shell's HTML and critical CSS inlined. This file is served for the initial request, allowing the browser to paint the application's structural skeleton almost instantly.  
2. **Content Population:** The server-rendered content (from either SSR or SSG) is then used to populate the area within the shell's \<router-outlet\>.  
3. **Hydration:** The full Angular application bootstraps and hydrates, making the entire page interactive.

This combination provides the best of all worlds: the instant FCP of a static shell, followed quickly by the meaningful content from SSR/SSG, all without the "blank page" delay of CSR. The ng generate app-shell command should be used to scaffold the necessary components and build configurations. The server logic in server.ts will then automatically use the App Shell's index.html as the template into which it injects the route-specific rendered content, making the integration seamless.

### **5.3 Caching Strategies: Server-Side and CDN**

Caching is not an optional optimization; it is a fundamental layer of a high-performance hybrid architecture. Two levels of caching are required.

* **CDN Caching:** All assets generated by the pre-rendering (SSG) process, including the static HTML files for each route and the JS/CSS bundles, **must** be deployed to and served from a Content Delivery Network (CDN). The CDN's role is to cache these static assets at edge locations around the world, minimizing network latency for users. Appropriate Cache-Control headers must be configured to ensure long cache durations for versioned assets (JS/CSS bundles) and appropriate revalidation strategies for the HTML files.  
* **Server-Side Caching:** For routes that are dynamically rendered via SSR, a server-side cache is strongly recommended to reduce computational load and improve response times. For content that is dynamic but not unique to every single user or request (e.g., a list of top products that updates every 15 minutes), the rendered HTML output from the SSR process should be stored in a cache like Redis or a simple in-memory cache with a short TTL. When a subsequent request for the same page arrives within the TTL, the cached HTML can be served directly, bypassing the expensive Angular rendering process on the server entirely. This is a critical strategy for managing server costs and ensuring consistent performance under load.

## **Section 6: Deployment and Operational Standards**

A hybrid rendering architecture produces a hybrid set of build artifacts, which in turn mandates a hybrid deployment strategy. This section defines the standard CI/CD process and reference architecture for deploying and operating the application.

### **6.1 CI/CD Pipeline: Build, Prerender, and Deploy Stages**

The Continuous Integration and Continuous Deployment (CI/CD) pipeline for a hybrid Angular application must follow a specific sequence of stages to correctly build and deploy both the static and dynamic components. Older, multi-step build commands are now consolidated.  
The mandatory pipeline stages are:

1. **Install Dependencies:** The pipeline begins by installing the exact project dependencies.  
   * Command: npm ci  
2. **Build Application:** The ng build command for a production configuration now orchestrates the entire build process. It compiles the client-side application, the server-side application, and automatically triggers the pre-rendering (SSG) process as defined in app.routes.server.ts. This single command replaces older, multi-step scripts like npm run build:ssr && npm run prerender.  
   * Command: ng build (which defaults to the production configuration in most CI environments)  
3. **Deploy Static Assets:** After the build completes, the pipeline must deploy the static portion of the application. This involves uploading the entire contents of the dist/\<project-name\>/browser directory to a CDN or a cloud storage service configured for static web hosting. This directory contains all JavaScript/CSS bundles and the pre-rendered HTML files for the SSG routes.  
4. **Deploy Server Application:** Concurrently or subsequently, the pipeline must deploy the dynamic portion of the application. This involves packaging the contents of the dist/\<project-name\>/server directory and deploying it to a Node.js-compatible hosting environment. Examples include serverless platforms like Google Cloud Run or AWS Lambda, or container orchestration services like Kubernetes.

### **6.2 Reference Architecture: CDN for Static Assets, Node.js for SSR Server**

The dual nature of the build artifacts necessitates a split deployment architecture. Attempting to serve static files from the Node.js server is inefficient and negates the benefits of a CDN. Attempting to run the Node.js server on a static-only host is impossible. Therefore, the standard reference architecture separates these concerns for optimal performance, scalability, and cost-effectiveness.  
The architecture consists of three main components:

1. **Request Router / Reverse Proxy:** A service like a CDN with configurable origins (e.g., Cloudflare, AWS CloudFront) or a reverse proxy (e.g., Nginx) acts as the single entry point for all user traffic.  
2. **Static Asset Host (CDN):** This is the origin for all pre-rendered (SSG) pages and static assets (JS, CSS, images). The dist/\<project-name\>/browser directory is deployed here.  
3. **SSR Server Host (Node.js Environment):** This is the origin for all dynamically rendered (SSR) pages. The dist/\<project-name\>/server application is deployed here.

The request router is configured with rules that direct traffic based on the requested URL path.

* If a request comes in for a route that has been pre-rendered (e.g., /about-us, /blog/my-first-post), the router serves the corresponding static HTML file directly from the CDN origin.  
* If a request comes in for a route designated for dynamic rendering (e.g., /user/dashboard, /search?q=angular), the router forwards the request to the Node.js server origin for on-demand rendering.

This architecture ensures that each request is handled by the most appropriate and performant system, fully realizing the benefits of the hybrid rendering strategy.

## **Section 7: Appendix: SSR\_Standard.md for AI Coder Consumption**

This appendix provides a condensed, machine-readable summary of the core rules from this standard. It is intended for direct consumption by AI coding agents to guide the implementation of new features and the maintenance of existing code.

### **Rule: Selecting Render Mode**

Use this logic to set the renderMode property in src/app/routes.server.ts.

* **RenderMode.Prerender (SSG)**  
  * **IF** a route's content is **static**, **public**, and **identical for all users** (e.g., /about-us, /contact, product documentation).  
  * **AND IF** all data required for rendering is available at **build time**.  
  * **THEN** set renderMode: RenderMode.Prerender.  
  * **Example:**  
    `{ path: 'about-us', renderMode: RenderMode.Prerender }`

* **RenderMode.Server (SSR)**  
  * **IF** a route's content is **dynamic**, **personalized for a user**, or requires **real-time data** (e.g., /user/dashboard, /search-results).  
  * **AND IF** data must be fetched at **request time**.  
  * **THEN** set renderMode: RenderMode.Server.  
  * **Example:**  
    `{ path: 'user/dashboard', renderMode: RenderMode.Server }`

* **RenderMode.Client (CSR)**  
  * **IF** a route is **behind an authentication wall** AND is **not intended for public search indexing** (e.g., /settings/profile).  
  * **THEN** set renderMode: RenderMode.Client.  
  * **Example:**  
    `{ path: 'settings/profile', renderMode: RenderMode.Client }`

### **Rule: Handling Parameterized SSG Routes**

* **IF** a parameterized route (e.g., /blog/:slug) is set to RenderMode.Prerender.  
* **THEN** you **MUST** implement the getPrerenderParams function to fetch all possible parameter values at build time.  
* **AND** you **MUST** set fallback: 'server' to enable the ISR pattern for newly added content.  
* **Example:**  
  `{`  
    `path: 'blog/:slug',`  
    `renderMode: RenderMode.Prerender,`  
    `async getPrerenderParams() {`  
      `const api = inject(ContentApiService);`  
      `const slugs = await api.getAllSlugs();`  
      `return slugs.map(slug => ({ slug: slug }));`  
    `},`  
    `fallback: 'server'`  
  `}`

### **Rule: Universal Component Authoring**

* **Platform-Specific APIs:**  
  * **IF** your code uses window, document, localStorage, or any other browser-only global.  
  * **THEN** you **MUST** wrap that code in an if (isPlatformBrowser(platformId)) {... } block.  
  * **Example:**  
    `if (isPlatformBrowser(this.platformId)) {`  
      `localStorage.setItem('key', 'value');`  
    `}`

* **DOM Manipulation:**  
  * **IF** your code needs to manipulate the DOM or initialize a third-party library that manipulates the DOM (e.g., a chart library).  
  * **THEN** you **MUST** execute that code inside an afterNextRender(() \=\> {... }); callback to prevent hydration flicker.  
  * **Example:**  
    `if (isPlatformBrowser(this.platformId)) {`  
      `afterNextRender(() => {`  
        `new ThirdPartyLib.init(this.elementRef.nativeElement);`  
      `});`  
    `}`

* **Data Fetching and State Transfer:**  
  * **IF** data is fetched on the server to render a page.  
  * **THEN** you **MUST** use TransferState to pass that data to the client to avoid duplicate API calls.  
  * For HttpClient GET requests, enable this by providing withHttpTransferCacheOptions() in app.config.ts.  
  * For other data, use the manual makeStateKey/set/get pattern.

### **Rule: Application Configuration**

* **Hydration:**  
  * The application **MUST** enable non-destructive hydration.  
  * Ensure provideClientHydration() is present in the providers of app.config.ts.
