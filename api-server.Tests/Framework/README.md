# Module/Directory: /Framework

**Last Updated:** 2025-05-19

> **Parent:** [`api-server.Tests`](../README.md)
> **Related:**
> * **Subdirectories:** [`Client/`](Client/README.md), [`Configuration/`](Configuration/README.md), [`Fixtures/`](Fixtures/README.md), [`Helpers/`](Helpers/README.md), [`Mocks/`](Mocks/README.md)
> * **Standards:** [`TestingStandards.md`](../../Docs/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../Docs/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory groups together various components that constitute the **testing framework** and shared infrastructure for the `api-server.Tests` project. These components are not tests themselves but provide essential utilities, setup routines, configuration management, mocks, and helper classes designed to support the writing and execution of both unit (`/Unit`) and integration (`/Integration`) tests.

* **Why group these?** Consolidating these supporting elements clarifies the distinction between actual test cases and the infrastructure that enables them. It promotes consistency and reusability across different types of tests.

## 2. Scope & Key Functionality

This directory contains subdirectories responsible for different aspects of the testing framework:

* **`Client/`:** Houses the auto-generated Refit HTTP client (`IZarichneyApi`) used by integration tests to make type-safe calls to the API under test. (See `Client/README.md`)
* **`Configuration/`:** Contains helpers or classes related to loading or providing test-specific configuration values (e.g., `appsettings.Testing.json`, environment variables for tests). (See `Configuration/README.md`)
* **`Fixtures/`:** Implements xUnit fixtures for managing shared context and resources across multiple tests. This includes the custom `WebApplicationFactory` (for bootstrapping the API in-memory) and `DatabaseFixture` (for managing test database containers). (See `Fixtures/README.md`)
* **`Helpers/`:** Provides various utility classes, extension methods, custom attributes (like `DependencyFactAttribute`), and helper functions to simplify common testing tasks (e.g., `AuthTestHelper` for simulating authenticated users, `GetRandom` for generating test data). (See `Helpers/README.md`)
* **`Mocks/`:** Contains mock implementations or, more commonly, factories (`Mocks/Factories/`) that provide pre-configured mocks (using Moq) for external services (Stripe, OpenAI, GitHub, etc.). These are typically injected into the `WebApplicationFactory` for integration tests or used directly in unit tests. (See `Mocks/README.md`)

## 3. Test Environment Setup

* **Usage:** Components within the `/Framework` directory are utilized by tests located in the `/Unit` and `/Integration` directories. They form the foundation upon which those tests are built.
* **Configuration:** Setup and configuration details for specific components (like the `WebApplicationFactory`, `DatabaseFixture`, or Mock Factories) are often handled within the respective fixture classes or potentially within base test classes (`IntegrationTestBase`, `DatabaseIntegrationTestBase`).

## 4. Maintenance Notes & Troubleshooting

* **Impact of Changes:** Modifications to components within the `/Framework` directory can potentially impact a large number of tests. Changes should be made carefully and tested thoroughly.
* **Client Regeneration:** The Refit client in `Client/` must be regenerated whenever the API contract changes using either PowerShell script `Scripts/GenerateApiClient.ps1` or Bash script `Scripts/generate-api-client.sh` (see `Client/README.md`).
* **Fixture Management:** Fixtures manage shared resources (like database containers or the web application factory instance). Ensure they handle setup and cleanup (disposal) correctly to avoid resource leaks or test interference.
* **Mock Updates:** Mocks and mock factories need to be updated if the interfaces or behaviors of the external services they represent change.
* **Helper Testing:** Complex helper classes within `Helpers/` should ideally have their own unit tests (which might reside under `/Unit/Helpers/`) to ensure their correctness.

## 5. Test Cases & TODOs

* **N/A:** This directory contains testing framework components, not tests themselves. Refer to the README files within the subdirectories (`Client/`, `Configuration/`, `Fixtures/`, `Helpers/`, `Mocks/`) for specific details about each component. Unit tests *for* specific helpers or fixtures might exist under `/Unit/Helpers/` or `/Unit/Fixtures/`.

## 6. Changelog

* **2025-04-18:** Initial creation - Grouped Client, Configuration, Fixtures, Helpers, Mocks under Framework. Defined purpose and scope. (Gemini)

