# Establishing Full-Stack Security Standards for Angular 19 + .NET 8

## 1. Authentication Hardening

**JWT Storage – Cookies vs. `localStorage`:** To mitigate XSS-based token theft, **avoid storing JWTs in browser `localStorage`**. Any token accessible via JavaScript (like in `localStorage` or `sessionStorage`) can be stolen if an XSS bug is present. Instead, use **Secure, `HttpOnly` cookies** with an appropriate `SameSite` attribute (ideally `Strict` or `Lax`). An `HttpOnly` cookie **cannot be accessed by JavaScript** (protecting the token’s confidentiality), and `SameSite=Strict` cookies will not be sent on cross-site requests, **greatly reducing CSRF risk**. In practice, this means issuing the JWT (or more commonly a **refresh token**) as a secure HttpOnly cookie. This cookie is automatically attached to API requests by the browser, but script code cannot read or modify it, thwarting thieves even if an XSS occurs. (Note: An XSS attacker could still act as the user’s session *in situ*, but cannot extract the token for reuse elsewhere.)

**Short-Lived Access Tokens & Refresh Tokens:** Use **short expiration times** for JWT access tokens (e.g. \~15 minutes) and rely on refresh tokens for session longevity. A best practice is to implement **Refresh Token Rotation**: each time the client uses a refresh token to get a new access token, the server *invalidates the old refresh token and issues a new one*, updating it in the HttpOnly cookie. This limits the window of abuse if a refresh token is ever captured. For example, an ASP.NET Core API might provide `/refresh-token` and `/revoke-token` endpoints – on refresh, generate a new JWT and a new refresh token (set in the cookie) and mark the old token as revoked server-side. Maintain a server-side store (database) of active refresh tokens with flags for revocation, issuance timestamps, etc., so you can detect malicious reuse of tokens and perform bulk invalidation if needed.

**Automatic Logout for Inactivity:** Enforce an **idle session timeout** so that users are logged out after a period of inactivity. A common pattern is to combine client-side tracking with server token expiry. For instance, set the refresh token’s lifetime to something like 30 minutes *sliding* – if the user doesn’t refresh or make an API call within 30 minutes, the refresh token expires and cannot be used (requiring re-authentication). This can be implemented by storing an “last activity” timestamp with the refresh token and having the refresh endpoint reject tokens that haven’t been used in the last X minutes. ASP.NET Core doesn’t do this by default for JWTs (which are stateless), so you must enforce it in your refresh logic. Additionally, on the **front-end (Angular)**, use an inactivity timer – for example, leverage the `IdleMonitor` (from libraries like `ng-idle`) or RxJS timers, to automatically call an `/logout` API or clear credentials if the user has been idle (no mouse/keyboard or API calls) for the configured duration. The combination ensures stale sessions are invalidated both client-side and server-side.

**JWT Signing Key Management:** Treat the JWT signing secret or key pair as a highly sensitive secret. **Never hard-code signing keys** in source code or config files; instead use a secrets management service. In .NET, the production-grade approach is to load keys from a secure vault or keystore at runtime. For example, if using symmetric signing (HMAC), store the secret in **AWS Secrets Manager** (since you deploy to AWS) and retrieve it on app startup. For asymmetric signing (RSA/ECDSA), you can use a key pair managed by **AWS KMS** or Azure Key Vault. *Recommended:* **Azure Key Vault** (or AWS Key Management Service) can **hold the private key and perform the signing operation on demand**, so the key never leaves the HSM. .NET can integrate with Key Vault via the Azure.Security.KeyVault libraries – e.g., use a `CryptographyClient` to sign a JWT’s SHA256 digest with an RSA key stored in Key Vault. This means your app calls the Key Vault service to sign tokens, and the vault enforces access control and key rotation. Key Vault and KMS support **automated key rotation policies** (e.g., rotate keys every 30 or 60 days). In practice, set up your .NET JwtBearer authentication to use a signing certificate retrieved from the vault at startup, or use Key Vault’s REST API for each signing operation. This adds slight overhead but greatly improves security by isolating keys. For AWS, you can use AWS KMS’s `Sign` and `Verify` APIs in a similar way with an asymmetric CMK (Customer Managed Key) designated for JWT signing.

Finally, ensure that **token validation is strict** on the server: use the JWT middleware’s options to validate issuer, audience, signing key, and set `ClockSkew = TimeSpan.Zero` to avoid tokens being accepted after expiration. All of these authentication rules should be clearly codified (e.g. *“**MUST** use HttpOnly cookies for storing tokens; no JWTs in localStorage.”*, “**MUST** expire access tokens within 15 minutes or less.”, “**MUST** retrieve signing keys from Secrets Manager and never commit them to code.”). These rules can then be enforced in code reviews or even automated checks (for example, a lint rule to detect usage of `localStorage.setItem(‘token’)` in front-end code).

## 2. Scalable Authorization Policies

**From Roles to Policy-Based Authorization:** Instead of hard-coding role checks (`[Authorize(Roles="Admin")]`) throughout the code, adopt ASP.NET Core’s **policy-based authorization** model. This allows more complex and expressive access rules that can evolve as the application grows. In Startup (Program.cs), you define policies using `AddAuthorization` and configure requirements for each policy. For example:

```csharp
builder.Services.AddAuthorization(options =>
{
    // Simple role-based policy:
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireRole("Administrator"));

    // Policy requiring either of multiple roles/claims:
    options.AddPolicy("CanAccessReporting", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("Manager") 
            || context.User.HasClaim("Department", "Finance")));
});
```

In this example, `"AdminOnly"` is a trivial policy (only users in the Administrator role succeed), while `"CanAccessReporting"` demonstrates a custom rule allowing either a certain role or a claim. Policies are **named, reusable** units – you attach them to controllers or endpoints via the `[Authorize(Policy="PolicyName")]` attribute, rather than sprinkling role names everywhere. This indirection makes it easier to change the rules in one place (the policy definition) rather than searching through code for every role check.

**Custom Policy Requirements & Handlers:** For more advanced scenarios, define **custom requirements** by creating classes that implement `IAuthorizationRequirement`, and **handlers** that implement `AuthorizationHandler<TRequirement>` to evaluate them. This is useful for policies that involve dynamic data or multi-step checks. For instance, suppose we want a policy `"EditOrderPolicy"` that allows editing an order if the user is either in the “Admin” role *or* is the creator of the order. We could create an `EditOrderRequirement : IAuthorizationRequirement` (with perhaps a constructor param for Order ID or resource info), and then implement an `AuthorizationHandler<EditOrderRequirement>` that checks the current user’s identity against the order’s owner, or checks a claim like `UserId` matches the order’s `CreatedBy`. In the handler we call `context.Succeed(requirement)` if the requirement is met. Multiple handlers can exist for one requirement; the requirement is considered satisfied if *any one* handler succeeds (handlers represent OR logic).

All such handlers should be registered in DI (e.g., `services.AddSingleton<IAuthorizationHandler, EditOrderHandler>()` at startup). Then in `AddAuthorization`, you add a policy mapping to that requirement, for example:

```csharp
options.AddPolicy("EditOrderPolicy", policy =>
    policy.Requirements.Add(new EditOrderRequirement()));
```

Now, `[Authorize(Policy="EditOrderPolicy")]` on the relevant controller action will invoke your handler to enforce the logic.

**Policy Advantages:** This approach scales better because you can **compose complex logic** (AND/OR conditions, dependency-injected services, database lookups, etc.) in one place rather than writing ad-hoc checks in controllers. It’s also **extensible** – adding a new permission no longer means creating a new attribute or magic string; you just add a policy. For example, as your app grows, you might move to a claims-based permissions model (like each user has claims like `Permission:Orders.Edit`), which can be enforced via a policy that checks for that claim. Policies can encapsulate these checks cleanly.

**Best Practice – Principle of Least Privilege:** Define policies around specific business actions or resources (e.g. `"CanPublishBlogPost"`, `"RequireMFA"` for sensitive ops, etc.) rather than broad roles whenever possible. This allows **AI code agents** and human devs to consistently apply **the correct policy name** to each endpoint. For instance, in the security standards markdown you might have a rule: *“All new API endpoints **MUST** specify an authorization policy (not just a role). If an appropriate policy does not exist, one **MUST** be created rather than using a role string.”* Then list the standard policies (and their meaning) so everyone and every coding tool knows which to use. Over time, this policy library can grow (potentially even generated from an external policy store in the future, e.g., if integrating with AWS Verified Permissions or OPA, though that’s beyond current scope). The key is that **authorization logic is centralized and configurable**.

**Example – Custom Policy:** A concrete example is using **policy requirements to encode complex conditions**. In one implementation, a `"VIPAccess"` policy might require either the user has a `VIPNumber` claim, or an `EmployeeNumber` claim from a specific partner airline, or is in the `CEO` role. Rather than requiring *all* of these (the default is AND), we can use `RequireAssertion` with a lambda to allow an OR combination, or implement an `IVIPRequirement` and then *multiple handlers* – one handler succeeds if `VIPNumber` claim exists, another if `EmployeeNumber` from Airline X exists, another if user.IsInRole("CEO"). If any of them succeeds, the policy passes. This way the logic is clear and testable in one place. We would include code samples of such handlers in the SecurityStandard document to illustrate how to write them, and possibly even provide base classes or utilities for common patterns.

## 3. OWASP Top 10 Risk Mitigations

### Input Validation & Injection Prevention

**Use Robust Server-Side Validation:** All incoming data must be validated on the server before use or persistence. In .NET, there are two primary approaches: *Data Annotations* (attributes on models) and the **FluentValidation** library. Data Annotations (e.g. `[Required]`, `[StringLength]`, `[Range]`) are simple and convenient for basic checks on DTOs/models. FluentValidation, however, is a more **powerful and flexible** fluent API for validation logic, supporting complex rules, conditional validation, and better separation of concerns (validators are defined in separate classes, not mixed into your model classes). For a large application concerned with security, **FluentValidation is recommended** – it allows centralizing all validation in one place and can handle scenarios like cross-field dependencies or different rules per context. For example, FluentValidation can easily express “if property X is Y, then property Z must be not null and within a range” – logic that is hard to do with attributes alone. FluentValidation also integrates with ASP.NET Core’s model-binding validation pipeline (via `services.AddFluentValidation()`), so that a bad request (HTTP 400) is automatically returned if validation fails, just like with Data Annotations.

No matter the mechanism, the standard should enforce an **allow-list approach** to validation: define exactly what format/characters are acceptable for each input (e.g., a username must be alphanumeric 3-20 chars, an email must match RFC patterns, an age must be an integer 0-120, etc.). **Reject input that doesn’t conform**. This not only prevents obvious garbage, but also helps prevent injection attacks by disallowing characters or patterns that have no legitimate reason to be in the input. (For example, if you expect a numeric ID, validate it is numeric – this inherently blocks SQL injection payloads like `1 OR 1=1` because letters or spaces aren’t allowed.) However, **do not rely on validation alone for security** – use parameterized queries/ORM for SQL queries, proper encoding for HTML output, etc. Input validation is a helpful *secondary* defense to reduce attack surface, but the primary defenses (like query parameterization to prevent SQL injection) must still be in place.

For preventing **SQL Injection**, use *only parameterized SQL commands or ORM parameters*. The security standard should mandate that **no SQL queries concatenate user input into the query string** – developers or AI code tools must use `SqlParameter` or LINQ query parameters. (We can integrate a static code analyzer to flag string concatenation in DB methods). For **NoSQL injection** or other injections, similarly ensure that user input isn’t directly interpreted as code/command.

**Implement Validation at All Layers:** Validate **client-side** (for user experience) *and* **server-side** (for security). The server should treat even its own SPA’s requests as potentially malicious (especially since attackers could craft requests outside the UI). If you have multiple microservices, each service should validate its inputs too (don’t assume upstream already did). All external-facing APIs should reject malformed or unexpected data early in the request pipeline to minimize downstream risk. A good practice is using **strong data types** for action method parameters – e.g., if an ID should be an `int`, make it an int parameter (model binding will fail if a non-int is provided, automatically preventing injection in that case). Use enumerations or boolean types where applicable instead of strings, etc., to enforce valid values implicitly.

We will standardize on FluentValidation usage for most scenarios (with examples in the policy doc). For instance, a rule might be: *“All API endpoints **MUST** validate request DTOs. Use FluentValidation validators for complex object validation. Simple scalar inputs (like query parameters) **MUST** be checked for range/format in the controller or business logic.”* We’ll include code snippets such as a `UserRegistrationValidator` class demonstrating typical rules (non-empty, length limits, regex for email). This not only prevents bad data but also common attacks – e.g., preventing overly long inputs can thwart buffer overruns and certain DoS attacks, and validating against a regex can mitigate some injection strings.

### Cross-Site Scripting (XSS) Defense-in-Depth

Angular by default has **built-in XSS protection** in its templating: it auto-escapes values interpolated into the DOM, and sanitizes potentially dangerous HTML in bindings (unless you explicitly bypass it). This helps a lot, but **do not rely solely on Angular’s client-side sanitization**. Apply a **Content Security Policy (CSP)** header on your web application responses as a critical defense-in-depth measure against XSS. CSP instructs the browser to only execute or load resources from allowed sources, mitigating the impact if an attacker does inject malicious script into your pages.

A **strict CSP** for an Angular SPA (in production) would disallow any external or inline scripts. For example:

```http
Content-Security-Policy: default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' https: data:; font-src 'self'; object-src 'none'; base-uri 'self'; form-action 'self'; frame-ancestors 'none'; upgrade-insecure-requests;
```

This policy (which can be delivered via an HTTP header or a `<meta>` tag in the index.html) does the following:

* **default-src 'self'**: by default, only load content from the same origin.
* **script-src 'self'**: allow scripts only from our domain (no third-party or inline scripts). Angular’s bundles are served from our domain, so that’s fine. We do **not** include `'unsafe-inline'` or `'unsafe-eval'` in production script policy – meaning no evals or inline `<script>` tags are permitted. Angular 19’s ahead-of-time compilation ensures eval is not needed at runtime. (During development, a more relaxed CSP can be used to allow Webpack dev server and live reload scripts, but those must be stripped in prod.)
* **style-src 'self' 'unsafe-inline'**: allow styles from our domain and inline styles. (Angular typically injects some small inline styles or you might use Angular Material which does, so we allow `'unsafe-inline'` for styles. This is considered low risk if script execution is blocked.)
* **img-src 'self' https: data:**: images can come from our domain, or any HTTPS URL, or data URIs. Adjust as needed if you use a CDN for images.
* **font-src 'self'**: fonts only from our domain (adjust if using e.g. Google Fonts CDN).
* **object-src 'none'**: block plugins like Flash or any `<object>` embeds entirely (reduces attack surface).
* **base-uri 'self'**: prevent the base URI from being changed by disallowing `<base>` tag injections.
* **form-action 'self'**: forms can only submit back to our site (no form POSTs to foreign sites).
* **frame-ancestors 'none'**: disallow our pages from being iframed by any site (mitigates clickjacking; equivalent to X-Frame-Options DENY).
* **upgrade-insecure-requests**: tells browsers to automatically upgrade any `http://` resource requests to `https://` (ensures we don’t accidentally load mixed content).

This CSP will **block** execution of any inline script an attacker might inject (the browser will refuse, and report a CSP violation). It also blocks loading scripts from other domains, so even if an attacker finds a loophole to inject a `<script src="http://evil.com/x.js">`, it won’t run because not from 'self'. In our SecurityStandard doc, we will include this example and note that teams should adjust the directives based on allowed CDNs or third-party scripts (with a strong recommendation to avoid those in the first place for a high-security app). We’ll also mention that Angular 17+ introduced some CSP support (like a build option to add nonces to its inline scripts) – by the time of Angular 19, using AOT compilation means the auto-generated inline scripts are minimal, but if any exist, a nonce-based CSP could be used for those. However, simplest is to ensure no inline `<script>` tags at all in our app (we can achieve this by not using things like `document.write` or third-party widgets that require it).

**Other XSS Measures:** On the server side, if the .NET backend ever returns HTML or concatenates user input into HTML (e.g., in an email template or PDF), use proper encoding. By default, Razor views in .NET Core encode values, but if you use `Html.Raw` or other mechanisms, be cautious. Our standard should say: *“Never disable Angular’s sanitization or bypass the DomSanitizer unless absolutely necessary. If you must, thoroughly validate or encode the data before binding.”* Also, set the header **`X-Content-Type-Options: nosniff`** on all responses – this prevents browsers from trying to interpret files as something else (which can lead to XSS in certain cases, e.g. if a user uploads an HTML file but it’s served as a different content type, nosniff keeps the browser honest about MIME types). Additionally, we explicitly disable the old IE XSS filter by setting **`X-XSS-Protection: 0`** (this header is mostly obsolete now, but turning it off avoids any weird browser behavior since modern CSP is the preferred solution).

### Cross-Site Request Forgery (CSRF) Protection

Since we are using JWT auth via cookies (per earlier recommendation), our API becomes susceptible to CSRF (because the browser will automatically include the auth cookie on requests to our domain). To counter CSRF, we implement the standard **double-submit cookie technique** (also known as using an **anti-CSRF token**). ASP.NET Core has built-in anti-forgery support we can leverage. We will:

* **Enable Anti-Forgery Services:** In Startup, call `services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");`. This configures the antiforgery service to look for a header named `X-XSRF-TOKEN` on incoming requests (we use the Angular default header name). It also by default expects a cookie with the antiforgery token (name like `XSRF-TOKEN` by default).

* **Send Token to Client:** We need to get the antiforgery token to the Angular app. One approach is to have a small endpoint that the SPA calls on startup (or you configure a middleware) to set the CSRF token cookie. For example, add a middleware in .NET:

  ```csharp
  app.Use(async (context, next) =>
  {
      if (context.Request.Path == "/" /* or some condition to send token */)
      {
          var tokens = antiforgery.GetAndStoreTokens(context);
          context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!,
              new CookieOptions { HttpOnly = false, Secure = true });
      }
      await next();
  });
  ```

  This will issue a cookie named `XSRF-TOKEN` (not HttpOnly, so that JavaScript can read it) containing the random antiforgery token. Angular’s HttpClient **automatically** looks for a cookie of that name and, on each request, will copy its value into an `X-XSRF-TOKEN` header. This happens for same-origin requests by default – Angular’s built-in XSRF protection is enabled as long as we use the default cookie/header names or configure them accordingly.

* **Validate on Server:** On the server, apply the `[AutoValidateAntiforgeryToken]` attribute globally for state-changing actions (POST/PUT/DELETE). This ensures that any such request *must* have a valid antiforgery token or it will be rejected with 400. The anti-forgery service will read the `X-XSRF-TOKEN` header and compare it to the token stored in the `XSRF-TOKEN` cookie for that user – if they match, it’s a legitimate request from our SPA, if not, it’s likely a CSRF attempt and gets blocked. We will document the need to add exceptions for any endpoints that cannot have CSRF (like GETs or endpoints explicitly meant to be called cross-site, if any) using `[IgnoreAntiforgeryToken]` if needed.

* **Confirm Angular Configuration:** By default, Angular’s HttpClient will do this as long as the cookie is named `XSRF-TOKEN`. If we changed names, we’d use `HttpClientXsrfModule.withOptions({ cookieName: 'XSRF-TOKEN', headerName: 'X-XSRF-TOKEN' })` in Angular. But we’ll stick to the default names. So essentially, once the user logs in and we set the auth cookie, we *also* ensure an `XSRF-TOKEN` cookie is set (could be set at login response time along with JWT issuance – the `GetAndStoreTokens` can be called and sent alongside the auth response). This cookie is **not HttpOnly** (by design, since the client-side script needs to read it), but that’s okay because its value is only a random token (not a sensitive secret or auth credential). The standard should note: *“Any state-changing API (beyond simple GETs) **MUST** be protected against CSRF. If using cookie-based auth, an anti-CSRF token mechanism **MUST** be implemented. Our standard approach is using ASP.NET Core’s Antiforgery with Angular’s built-in XSRF support.”* Include code snippet of the Angular HttpClient automatically sending the header and the .NET attribute usage.

With these measures, even if an attacker tricks a user’s browser to visit a malicious site, any forged requests to our API (which would include the auth cookie) will **lack the correct XSRF token**, and the server will reject them. This ensures that **only our SPA can effectively use the JWT cookie** because only our SPA knows the CSRF token (it’s stored in the `XSRF-TOKEN` cookie and copied into headers by Angular).

One more thing: set `SameSite=Lax` or `Strict` on the auth cookie if possible. If our SPA and API are on the same domain, `Strict` is ideal (then CSRF is largely mitigated by the browser, as it won’t send the cookie on cross-site navigations or iframes at all). However, if the SPA is served on a different domain (say using a CDN) than the API, we might need `None` (with Secure) for the auth cookie, which means CSRF protections rely entirely on the token approach above. Our standard will clarify the cookie settings: *“Auth cookies **MUST** be set with `Secure` and `HttpOnly`. `SameSite=Strict` is recommended unless you have a cross-domain scenario in which case you **MUST** implement anti-CSRF tokens as above.”*

## 4. Secure Development Lifecycle (SDL) Integration

### Dependency Vulnerability Scanning (Frontend & Backend)

Keep **dependencies updated and free of known vulnerabilities** by integrating scanning into CI. We will use native package managers’ audit tools plus GitHub capabilities:

* **npm audit:** For Angular (and any Node-based tooling), configure the CI (GitHub Actions) to run `npm audit --audit-level=high` (or `npm audit --production --audit-level=high` if dev dependencies can be ignored) on each pull request. This will list any packages with known vulns of high (or critical) severity. If any are found, the step should fail the build. We consider a PR introducing a high-severity vulnerability as a breaking issue. In the GitHub Actions workflow YAML, we can add a job step:

  ```yaml
  - name: Audit Node.js Dependencies
    run: npm ci && npm audit --audit-level=high
  ```

  By default, `npm audit` exits with a non-zero code if vulnerabilities meeting the threshold are found, causing the workflow to fail. We will document that *no new high or critical vuln is acceptable* – the developer must address it (either update the package, apply a patch, or if false-positive, explicitly acknowledge it in the security standard exception list) before merge.

* **dotnet list package --vulnerable:** .NET has an integrated tool to check NuGet dependencies against known CVE databases. We will use `dotnet list package --vulnerable --include-transitive` in CI to catch vulnerable libraries in our .NET project. This command will output any package (direct or transitive) that has a known vulnerability (data sourced from GitHub’s advisory database). We will treat any **High or Critical severity** vulnerability as a failure. The challenge is `dotnet list package --vulnerable` may not yet support failing the process with a code, so we might need to parse the output. We can write a small script or use a GitHub Action that parses the JSON output (`--format json` if available in .NET 8) and fails if any vulnerability severity >= High is present. Our standard pipeline configuration will include:

  ```yaml
  - name: Audit .NET Dependencies
    run: dotnet list package --vulnerable --include-transitive
  ```

  and then possibly a parse step. Alternatively, we can integrate **GitHub Dependabot alerts** (which are automatic in GitHub repos) and treat those alerts as blocking (by not ignoring them). In any case, the **pull request should not be merged if it introduces a new vulnerability**. The security document will specify: *“CI pipelines **MUST** run dependency vulnerability scans. Build **MUST fail** if any new high-severity vulnerability is found. Developers should update or remove the offending package before proceeding.”* This ensures we catch issues like an outdated Angular package with an XSS bug or a .NET NuGet with a known RCE.

Additionally, we recommend enabling **Dependabot** for continuous monitoring of dependencies. Dependabot can automatically open PRs to update packages when security fixes are available. This isn’t a direct CI gate, but part of SDL to keep us up-to-date. Our standard should mention it as a practice.

### Static Analysis (SAST) for Security

In addition to dependency scanning, incorporate **Static Application Security Testing** tools directly into GitHub Actions. This will automatically analyze our code for common vulnerabilities (in both C# and TypeScript). Recommended tools:

* **GitHub CodeQL**: We can use GitHub’s CodeQL code scanning (if available in our plan) to scan both the .NET and Angular codebases. CodeQL has support for C# and for JavaScript/TypeScript queries. We should set up the GitHub Actions CodeQL workflow (which uses the `github/codeql-action`); it will run on pushes and PRs and alert on any found security issues (like SQL injection, XSS, hardcoded secrets, etc.). This provides an automated code review for security anti-patterns. In our SecurityStandard.md we’d note: *“CodeQL scanning is enabled on the repository. All contributions **SHOULD** pass CodeQL with no new alerts. Any new alert must be justified or fixed before merge.”*

* **Roslyn Analyzers (Security Code Scan)**: We can also use the open-source **SecurityCodeScan** Roslyn analyzer for .NET, which finds insecure code patterns (the OWASP rules like SQL injection, XSS, CSRF misuse, etc.). By including it as a NuGet package or as part of our .NET project, these rules run during build and can surface warnings/errors. We can treat warnings from this analyzer as build failures in CI. For example, if someone tries to disable validation or use `Html.Raw` unsafely, the analyzer can flag it. Our standard will say *“The project includes static analyzers for security (e.g., SecurityCodeScan). Do not suppress their warnings without security team approval.”*

* **ESLint Security Plugin for Angular**: For the front-end, use ESLint with security-focused rules. There are plugins like eslint-plugin-security and sonarjs that catch things like use of `eval()` or detecting potential DOM XSS sinks. We ensure our Angular project’s lint config includes such rules. For example, disallow use of the Angular DOM sanitizer to bypass security (`bypassSecurityTrustHtml` etc.) without a very good reason. This can be encoded as a lint rule (or at least documented policy).

These SAST measures can be automated in CI: e.g., run `npm run lint` (with the security rules) and `dotnet build` (with analyzers enabled) on each PR. The **goal is to catch security issues early**. This also assists AI coding agents: if they inadvertently introduce a risky pattern, the automated analysis will flag it, and we can encode in the agent prompt that “lint must pass with no security warnings”.

### Secrets Management in CI/CD

Protecting secrets in the pipeline is paramount. **Never hardcode secrets** (API keys, DB passwords, etc.) in code or in the CI scripts. Instead, use **GitHub Actions Encrypted Secrets** for any credentials. Our policy: *“All secrets (API keys, tokens, connection strings) **MUST** be stored in GitHub Actions secrets and referenced via `{{ secrets.NAME }}`.*\* No plain-text secrets in repos or pipeline definitions.”\* This means, for example, AWS credentials for deployment are stored as GH secrets (`AWS_ACCESS_KEY_ID`, etc.) and not committed.

**Principle of Least Privilege for CI:** The built-in `GITHUB_TOKEN` used by Actions should be limited to only the permissions necessary. By default (since 2023) GitHub tokens have read-only perms on the repo, but we explicitly configure it in our workflows. In every workflow YAML, we’ll set:

```yaml
permissions:
  contents: read
  # (and others as needed, but no write unless absolutely required)
```

This ensures the CI job cannot, for instance, push code or create releases unless we intend it to. If a job needs to create a GitHub release or a comment on PR, we can give specific `contents: write` or `issues: write` for that job only. All other scopes remain at default none. The standard should say: *“The CI workflow YAML **MUST** declare explicit `permissions` for the GITHUB\_TOKEN, defaulting to read-only for everything. Enable write perms only for the specific actions that require it.”* This prevents a compromised build script from abusing the repo via the token. GitHub’s docs and security researchers emphasize least privilege here.

**Avoid Exposing Secrets to Pull Requests:** By GitHub design, secrets are not passed to workflows triggered by forked PRs. We will **not override that**. Our standards: do not run deploy or critical jobs on untrusted PRs, or if you do, ensure `GITHUB_TOKEN` has no write perms and no secrets are used. This mitigates threat of PRs from external contributors trying to exfiltrate secrets.

**Secret Rotation and OIDC:** We encourage using short-lived credentials and OIDC where possible. For deployment to AWS, instead of storing a long-lived AWS key in secrets, we can use GitHub’s OIDC integration to let the action assume a role in AWS (with a limited scope) – so no static secret at all, just a federated token exchange. Our policy document will mention: *“Prefer OIDC for cloud auth in CI to avoid long-lived cloud keys. If using static keys, rotate them regularly.”*. And *“CI secrets should be scoped to environment – e.g., separate dev vs prod deployment credentials, and protected by environment rules in GitHub (require manual approval for prod deploy, etc.).”*

Additionally, ensure that **no secrets end up in logs**. GitHub masks outputs that match secret values, but one could accidentally `echo $SUPER_SECRET`. We instruct: *“Do not print secrets in CI logs. If you need to debug, use tools like gitHub’s `secret` masking rather than printing the actual value.”* (This was noted in best practices).

By following these guidelines, our CI pipeline will be locked down: minimal token perms, all secrets encrypted, no secret exposure, and ephemeral tokens where possible. This significantly reduces the risk that a compromised CI job could pivot to other systems.

In summary, our SDL automation section in SecurityStandard.md will have a checklist like:

* ✅ Use dependency scans (npm audit, dotnet audit) on each PR.
* ✅ Enable automated code scanning (CodeQL/SonarCloud) for security issues.
* ✅ All code must pass linting and analyzers with no critical warnings.
* ✅ Absolutely no secrets in code; use GH Actions secrets and limit `GITHUB_TOKEN` perms.
* ✅ Protect production deployment with environment approvals (if applicable).
* ✅ Principle of least privilege at every stage.

## 5. API & Network Security

### Security HTTP Headers

Every HTTP response from our .NET API (and the Angular app, if served by our server or CDN) should include a set of **standard security headers**. We will use either middleware or server configuration to add these. The **mandatory headers** and their configurations are:

* **Strict-Transport-Security (HSTS):** Instructs browsers to only use HTTPS. We set `Strict-Transport-Security: max-age=63072000; includeSubDomains; preload`. This means for the next 2 years, browsers should refuse HTTP for our domain (and subdomains) and go HTTPS. (We can submit our domain to the browser preload list if appropriate). *Caution:* only enable after we’re confident HTTPS is always available.

* **X-Content-Type-Options:** `nosniff`. Prevents MIME-sniffing. This stops browsers from trying to execute non-JS files as JS, for example. Always set this.

* **X-Frame-Options:** `DENY` (or at least `SAMEORIGIN` if we ever need iframes from same site). This header is a backup for older browsers to prevent clickjacking (our CSP `frame-ancestors 'none'` covers modern browsers). We include it for defense-in-depth.

* **Content-Security-Policy:** As discussed in the XSS section, we will include a CSP header. Even if we start with a somewhat lenient one (to get things working), the standard will have a baseline that **no external scripts are allowed** and no unknown domains for other resources without explicit review. Over time we tighten this CSP. A sample minimal CSP in our doc might be: `default-src 'self'; img-src 'self' https: data:; script-src 'self'; style-src 'self' 'unsafe-inline'; object-src 'none'; frame-ancestors 'none';` which can be adjusted per environment. We’ll emphasize that any deviation (like adding a new CDN) must be reviewed by security.

* **Referrer-Policy:** `strict-origin-when-cross-origin`. This ensures that the Referer header sent by the browser doesn’t leak the full URL of previous page to other domains – it will only send the origin for external links. This protects potentially sensitive URL info from leaving our site.

* **Permissions-Policy:** Formerly Feature-Policy. We set this to disable any browser features we don’t use, to reduce exploitation surface. For example: `Permissions-Policy: geolocation=(), camera=(), microphone=(), payment=()` which means **disallow** those features completely. If our app needs geolocation or others, we configure accordingly (maybe allow self/origin). But as a baseline, if we don’t use VR, camera, mic, etc., we turn them off in the browser. Another one to disable might be `clipboard-write` (if not needed) or `fullscreen` (depending on app). This header is flexible; we will include an example and note to tailor it.

* **Other headers:** If applicable, **Server** header removal (by default Kestrel adds `Server: Kestrel`). We can remove or replace it to avoid advertising our server software. Not critical, but our standard can say *“The server **SHOULD NOT** reveal software versions – e.g., disable the Server header or set a generic value.”* Also, if using any cookies besides the JWT cookie, ensure `Secure` and `HttpOnly` flags (and perhaps `SameSite`) are always set. The `.NET` CookiePolicy or specific cookie options can enforce this globally.

We can use a library or middleware to add these headers. For instance, the `NetEscapades.AspNetCore.SecurityHeaders` library allows easy configuration of these headers in a fluent style. Or we manually set them in Startup. We will likely create a code snippet in the standard like:

```csharp
app.UseHsts(); // built-in for HSTS with defaults
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Add("Permissions-Policy", "geolocation=(), camera=(), microphone=()");
    await next();
});
```

(and CSP either configured via web server or also in this middleware). This ensures every response has them. For static hosting on S3/CloudFront, we can configure equivalent headers in CloudFront behaviors or the S3 bucket metadata.

**Why these headers:** We’ll include a brief justification in the doc next to each header, similar to above – e.g., “HSTS: ensure HTTPS, prevent downgrade attacks,” “XCTO: prevent MIME sniff exploit,” “Permissions-Policy: limit use of browser APIs,” etc., with references to OWASP Secure Headers guidelines.

These headers collectively harden the app against common web attacks and ensure good browser-side security posture. We’ll likely run our deployed app through security scanners (like Mozilla Observatory) to verify an A+ score, which can be an acceptance criterion.

### API Rate Limiting and DoS Protection

To protect the API from abuse such as brute-force attacks or denial-of-service via API call floods, implement **rate limiting** at the application level (in addition to any infrastructure-level limits).

**ASP.NET Core 8 Built-in Rate Limiting:** .NET 8 includes a new Rate Limiter middleware. We will use it to define policies controlling the number of requests allowed per time window. For example, we might set a **global rate limit** of, say, 100 requests per minute per IP address (or per user, if authenticated). And perhaps stricter limits on sensitive endpoints (like login). In `Program.cs`:

```csharp
builder.Services.AddRateLimiter(options =>
{
    // Global limit: 100 requests per minute per client (IP or user)
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        // Partition key: use authenticated username if present, otherwise client IP
        var id = httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress.ToString();
        return RateLimitPartition.GetFixedWindowLimiter(id, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 100,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        });
    });
    // Example of a specific named policy (e.g., more strict for login)
    options.AddFixedWindowLimiter("LoginPolicy", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2;
    });
});
app.UseRateLimiter();
```

This code globally limits each user or IP to 100 requests/minute. If the limit is exceeded, the middleware will automatically return 429 Too Many Requests. We can also tag certain endpoints with a named policy; for instance, on our login endpoint action, we use `[RequireRateLimiting("LoginPolicy")]`, which might only allow 5 attempts per minute (to slow down password brute force).

Our SecurityStandard will define some sane defaults: for example, *“Any single IP cannot hit the API more than X times per minute.”* And *“Auth endpoints (login, token refresh) should be additionally throttled to Y per minute per IP.”* We’ll enforce these via the .NET middleware as above. Also mention **User Enumeration**: for example, if login is by username, brute force across many usernames could bypass per-user limits, so IP-based limiting is also important.

**AWS Integration:** Because we deploy on AWS, we might also leverage AWS services for rate limiting: e.g., API Gateway or an Application Load Balancer can have rate limiting, or AWS WAF has **Rate-based rules** that we can configure (like block IP that makes >N requests in 5 minutes). The standard can mention: *“Enable AWS WAF rate-limit rules for an additional layer of DDoS defense. For instance, block IPs that exceed 1000 requests in 5 minutes.”* AWS Shield Standard protects against volumetric DDoS, but layer7 we handle with WAF + app logic.

However, even with app-level limiting, we note that **rate limiting is not a complete DDoS solution** (a distributed attack from many IPs can evade simple per-IP limits). For serious DDoS, AWS Shield Advanced or Cloudflare, etc., should be considered. Our standard will say: *“Rate limiting is enabled to mitigate brute-force and abusive clients. This is not sufficient for large-scale DDoS – ensure AWS Shield/WAF is configured for that threat.”*

**Logging and Monitoring:** We should log when rate limits are exceeded (at least aggregate counts), so we can detect an ongoing attack or a misbehaving client. Possibly integrate with Amazon CloudWatch or Application Insights to alert if many 429 responses are occurring (indicating either a bot attack or maybe that our limits are too low and affecting legitimate users). The security standard might mention: *“All 429 TooManyRequests responses **SHOULD** be logged with client identifiers. Repeated violations from the same IP **SHOULD** trigger an alert/ban.”* We could tie in an automated IP block if needed (though careful to avoid blocking legit traffic).

**Brute-force password guessing:** In addition to rate limiting login attempts, our Identity system should employ **account lockout** – e.g., lock an account after, say, 5 failed logins within 5 minutes. ASP.NET Core Identity has LockoutEnabled with default 5 attempts. We will ensure that’s turned on. Our standard: *“User accounts will be locked for 15 minutes after 5 failed login attempts. Password reset or admin intervention required if lockout persists.”* This complements the IP-based throttling.

In summary, **every API endpoint is governed by some rate limit**. The SecurityStandard will list the default global rate (e.g., “no more than 10 req/sec from one IP”) and any special cases (login, etc.). And it will instruct developers/AI: *“When adding new endpoints that could be expensive or sensitive, consider applying a custom rate limit policy.”* The standard will include a reference to Microsoft’s rate-limiting middleware docs and perhaps show a snippet as above for how to add a new named limiter.

---

Finally, after detailing all these measures, we will compile them into an official **`SecurityStandard.md`** structure that is actionable. This document will serve as both a policy and a practical how-to for our developers (and AI assistants). It will be organized into sections mirroring the above research points:

## Proposed Structure for `SecurityStandard.md`

1. **Introduction & Scope** – Explain that this standard covers security best practices for our Angular + .NET stack, to be enforced in development and CI/CD. Mention OWASP Top 10 alignment and automation goals.

2. **Authentication & Session Management**

   * **Token Storage & Handling** – Rules for JWT storage (HttpOnly cookies only, no localStorage), sample code config for cookies, SameSite usage.
   * **Token Expiry & Rotation** – Required token lifetimes (access token TTL, refresh token TTL), refresh rotation policy (one-time use refresh tokens), idle timeout (inactivity logout after X minutes).
   * **Secret Keys** – How to manage JWT signing keys (use AWS Secrets Manager/Azure Key Vault, never in source). Include key rotation procedures.
   * **Multi-Factor & External IdPs** – (If future extensibility, note that design should allow plugging in Google/OIDC providers securely.)

3. **Authorization**

   * **Role vs Policy** – Mandate use of policy-based \[Authorize] attributes. List standard policies (AdminOnly, etc.) and their definitions.
   * **Defining New Policies** – How to create `IAuthorizationRequirement` and handlers. Provide an example (like “Must be Order Owner or Admin” policy code).
   * **Least Privilege** – Encourage fine-grained policies (e.g., feature-specific) instead of broad roles. Every endpoint must specify auth (no \[AllowAnonymous] unless documented exception).

4. **Input Validation & Output Encoding**

   * **Validation** – All incoming data must be validated. Use FluentValidation for request DTOs. Provide examples of rules (with code).
   * **Injection Prevention** – State that prepared statements/ORM must be used for database calls (no string building SQL). No eval or dynamic code execution on user input.
   * **Output Encoding** – (If server returned HTML or we render data in Angular differently) ensure proper encoding. Angular handles most XSS via auto-escaping; caution against bypasses.

5. **Web Security Headers & Response Hardening**

   * List all required HTTP headers and their exact values. (This acts as a checklist for devs and for ops to configure CDN if needed.)
   * Example:

     * `Strict-Transport-Security: max-age=63072000; includeSubDomains; preload`
     * `Content-Security-Policy: ...` (with our approved policy string)
     * `X-Frame-Options: DENY`
     * `X-Content-Type-Options: nosniff`
     * `Referrer-Policy: strict-origin-when-cross-origin`
     * `Permissions-Policy: camera=(), microphone=(), geolocation=()`, etc.
   * Also mention cookie flags (Secure, HttpOnly, SameSite).
   * This section would provide the reasoning (maybe in footnotes or brief text) for each header, to ensure dev understanding.

6. **Cross-Site Scripting (XSS)**

   * Acknowledge Angular’s built-in XSS protections.
   * Rules: never disable sanitization without security review; prefer innerText binding to innerHTML.
   * CSP – state the CSP policy and that it must be kept enabled. If devs need to add a script or use inline styles, they must go through security team to adjust CSP rather than disabling it.
   * Possibly mention using DOMPurify (if ever needed to sanitize HTML strings) or Angular sanitizer usage guidelines.

7. **Cross-Site Request Forgery (CSRF)**

   * Explain double-submit cookie approach in use.
   * All state-changing APIs must verify the XSRF token. Provide sample code of how backend and frontend implement this (as done above).
   * Developers shouldn’t have to change this, but if new front-end frameworks are used, they must implement similar CSRF protection.

8. **Secure Development Lifecycle**

   * **Dependency Management:** Must run `npm audit` and `dotnet list package --vulnerable` in CI. Define what severity causes a build failure (e.g., “High and Critical must be 0”). If a vulnerability is discovered, developer must upgrade or document a temporary mitigation.
   * **Static Analysis:** List the SAST tools (CodeQL, ESLint rules, Roslyn analyzers). State that code must not introduce new SAST findings. Provide instructions if they run the analyzers locally (e.g., `npm run lint`, `dotnet build` with analyzers).
   * **Secrets:** Reiterate no plaintext secrets in code or config. Use environment variables and GitHub Secrets. Possibly list which secrets are currently in use (so new ones follow suit).
   * **CI Pipeline Security:** Outline that `GITHUB_TOKEN` is read-only by default; any job needing write perms must be reviewed. Mention use of environments for prod deploy with manual approval.
   * **Code Reviews:** mandate security-focused code review (maybe a checklist of questions reviewers or AI should ask, like “Are all inputs validated? Are errors not revealing sensitive info? etc.”).

9. **API Rate Limiting & Abuse Prevention**

   * Specify global rate limits (requests per minute per IP/user).
   * Specify stricter limits for auth or expensive endpoints.
   * Mention auto-ban or alert thresholds (if any).
   * This section also covers brute force login protection (account lockout policy).
   * Possibly mention logging and monitoring practices (failed logins, 429s, etc., should be monitored).

10. **Logging & Incident Response** (if desired):

    * (Not heavily covered above, but we could include) – “All authentication events and administrative actions must be logged to CloudWatch/ELK with user IDs and timestamps. Monitor logs for anomalies.”
    * “In case of a suspected breach, have procedure ...” (This might be beyond scope, but a brief note could be here since a security standard often covers logging.)

11. **Conclusion** – Emphasize that **automation** is in place: many of the above rules are enforced by tooling (lint, tests, CI checks). Developers are expected to fix issues identified by these tools. Also encourage continuous education (maybe reference OWASP Top 10 and cheat sheets as further reading).

12. **References** – (If this were a formal doc, we’d list OWASP cheatsheets, MS docs, etc., some of which we cited here).

Each section in the markdown will contain **specific prescriptive requirements**, often highlighted as “MUST”, “SHOULD”, etc., so that they can be directly used in prompts for AI coding agents. For example: in the Authentication section, a bullet might read: *“**MUST** store JWTs only in HttpOnly cookies; the application **MUST NOT** use localStorage for tokens.”* Another: *“**MUST** call `services.AddAntiforgery` and include XSRF-TOKEN cookie on login responses.”* This style ensures that whether a human developer or Copilot-type agent is writing code, these rules can be given as non-negotiable constraints to follow.

By organizing the SecurityStandard in this way, we cover the full stack (Angular front-end, .NET back-end, CI/CD, and infrastructure considerations) in one comprehensive policy. This will greatly help us not only secure the application but also enforce consistency – everyone (or every AI assistant) has the same playbook to follow for security.

