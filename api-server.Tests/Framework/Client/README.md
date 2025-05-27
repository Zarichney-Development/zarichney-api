# README: /Framework/Client Directory

**Version:** 2.0
**Last Updated:** 2025-05-26
**Parent:** `../README.md`

## 1. Purpose & Responsibility

**IMPORTANT:** This directory no longer contains auto-generated Refit client interfaces. These have been **migrated to the dedicated `Zarichney.ApiClient` project** for improved modularity and reusability.

### Current Status

* **Directory Contents:** Only this README.md file remains
* **Migration Date:** 2025-05-26 (Issue #12)
* **New Location:** All Refit client interfaces and models are now in the `Zarichney.ApiClient` project

### Historical Responsibility (Pre-Migration)

Previously, this directory housed multiple auto-generated Refit client interfaces and their associated Data Transfer Objects (DTOs). These interfaces were organized by API tags (corresponding to controllers) and provided type-safe mechanisms for integration tests to interact with the `api-server`'s HTTP endpoints.

## 2. Migration Details

### What Was Moved

* **Interface Files:** `IAiApi.cs`, `IAuthApi.cs`, `ICookbookApi.cs`, `IPaymentApi.cs`, `IPublicApi.cs`
* **Models/DTOs:** `Contracts.cs` (moved to `Zarichney.ApiClient/Models/`)
* **Dependency Injection:** `DependencyInjection.cs` (moved to `Zarichney.ApiClient/Configuration/`)

### New Structure

The migrated code is now organized in the `Zarichney.ApiClient` project with the following structure:
* `Zarichney.ApiClient/Interfaces/` - All Refit interface definitions
* `Zarichney.ApiClient/Models/` - All DTOs and response models
* `Zarichney.ApiClient/Configuration/` - Dependency injection setup

### Updated Namespaces

* **Old:** `Zarichney.Client` and `Zarichney.Client.Contracts`
* **New:** `Zarichney.ApiClient.Interfaces` and `Zarichney.ApiClient.Models`

## 3. Impact on Testing Framework

### For Integration Tests

* **Current State:** Integration tests still reference the old namespace and will have build errors
* **Required Action:** Tests need to be updated to reference the new `Zarichney.ApiClient` project (planned for subsequent tasks)
* **ApiClientFixture:** Will need to be updated to use the new project structure

### Client Generation

* **Script Updates:** Generation scripts have been updated to target the new `Zarichney.ApiClient` project
* **Configuration:** The `.refitter` configuration now outputs to `../Zarichney.ApiClient` instead of this directory

## 4. Next Steps

### For Test Project Migration (Planned)

1. Add project reference to `Zarichney.ApiClient` in `api-server.Tests.csproj`
2. Update all `using` statements from `Zarichney.Client.*` to `Zarichney.ApiClient.*`
3. Update `ApiClientFixture` to use the new project structure
4. Update all integration tests that reference the client interfaces
5. Remove build errors caused by missing client references

### For Future Development

* **API Changes:** Continue using the same generation scripts (`./Scripts/generate-api-client.ps1` or `.sh`)
* **Client Usage:** Reference patterns documented in `Zarichney.ApiClient/README.md`
* **Testing Patterns:** Follow guidance in `../../../Zarichney.Standards/Testing/IntegrationTestCaseDevelopment.md`

## 5. References

* **New Client Project:** `../../../Zarichney.ApiClient/README.md`
* **Migration Task:** GitHub Issue #12
* **Testing Standards:** `../../../Zarichney.Standards/Testing/TestingStandards.md`
* **Technical Design:** `../../TechnicalDesignDocument.md`

## 6. Rationale for Migration

The migration to a dedicated `Zarichney.ApiClient` project provides:
* **Improved Modularity:** Separates client concerns from test infrastructure
* **Reusability:** Allows multiple projects to consume the API clients
* **Better Organization:** Clear separation between generated client code and test framework
* **Future-Proofing:** Prepares for potential NuGet package distribution

---

**Note:** This directory structure will be maintained for backward compatibility during the transition period. Once all tests are migrated to use the new `Zarichney.ApiClient` project, this directory may be considered for removal in future cleanup tasks.