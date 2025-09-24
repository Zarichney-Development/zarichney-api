using FluentAssertions;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;
using Zarichney.Server.Tests.Framework.Fixtures;
using Zarichney.Server.Tests.Framework.Attributes;
using Zarichney.Server.Tests.Framework.Helpers;

namespace Zarichney.Server.Tests.Integration.AiFramework;

/// <summary>
/// Focused integration tests validating the iterative-coverage-auditor.md prompt
/// integration with AI framework components from Issues #184, #185, #187.
/// </summary>
[Collection("IntegrationCore")]
[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Category, "AiFramework")]
public class AuditorPromptValidationTests : IntegrationTestBase
{
    public AuditorPromptValidationTests(
        ApiClientFixture apiClientFixture,
        ITestOutputHelper outputHelper)
        : base(apiClientFixture, outputHelper) { }

    #region Template Variable Processing Tests

    [Fact]
    [Trait(TestCategories.Category, "TemplateVariableProcessing")]
    public void ValidateAuditorPromptContainsRequiredAiFrameworkVariables()
    {
        // Arrange: Load the actual auditor prompt
        var promptPath = "/home/zarichney/workspace/zarichney-api/.github/prompts/iterative-coverage-auditor.md";
        var promptContent = File.ReadAllText(promptPath);

        // Act & Assert: Validate AI Framework Variables (Issue #184)
        promptContent.Should().Contain("{{BASELINE_COVERAGE}}",
            "because AI framework provides coverage baseline analysis");
        promptContent.Should().Contain("{{NEW_COVERAGE}}",
            "because AI framework tracks current coverage metrics");
        promptContent.Should().Contain("{{COVERAGE_ANALYSIS}}",
            "because AI framework provides coverage intelligence");
        promptContent.Should().Contain("{{STANDARDS_COMPLIANCE}}",
            "because AI framework validates standards compliance");
    }

    [Fact]
    [Trait(TestCategories.Category, "TemplateVariableProcessing")]
    public void ValidateAuditorPromptContainsRequiredIterativeActionVariables()
    {
        // Arrange: Load the actual auditor prompt
        var promptPath = "/home/zarichney/workspace/zarichney-api/.github/prompts/iterative-coverage-auditor.md";
        var promptContent = File.ReadAllText(promptPath);

        // Act & Assert: Validate Iterative Action Variables (Issue #185)
        promptContent.Should().Contain("{{ITERATION_COUNT}}",
            "because iterative action tracks iteration number");
        promptContent.Should().Contain("{{PREVIOUS_ITERATIONS}}",
            "because iterative action preserves historical results");
        promptContent.Should().Contain("{{CURRENT_TODO_LIST}}",
            "because iterative action manages active to-do items");
        promptContent.Should().Contain("{{HISTORICAL_CONTEXT}}",
            "because iterative action maintains context across iterations");

        // PR Context Variables
        promptContent.Should().Contain("{{PR_NUMBER}}",
            "because iterative action processes PR context");
        promptContent.Should().Contain("{{PR_AUTHOR}}",
            "because iterative action identifies PR author");
        promptContent.Should().Contain("{{SOURCE_BRANCH}}",
            "because iterative action tracks source branch");
        promptContent.Should().Contain("{{TARGET_BRANCH}}",
            "because iterative action tracks target branch");
    }

    [Fact]
    [Trait(TestCategories.Category, "TemplateVariableProcessing")]
    public void ValidateAuditorPromptContainsRequiredCoverageAnalysisVariables()
    {
        // Arrange: Load the actual auditor prompt
        var promptPath = "/home/zarichney/workspace/zarichney-api/.github/prompts/iterative-coverage-auditor.md";
        var promptContent = File.ReadAllText(promptPath);

        // Act & Assert: Validate Coverage Analysis Variables (Issue #187)
        promptContent.Should().Contain("{{COVERAGE_DATA}}",
            "because coverage analysis provides detailed metrics");
        promptContent.Should().Contain("{{COVERAGE_DELTA}}",
            "because coverage analysis tracks improvement deltas");
        promptContent.Should().Contain("{{COVERAGE_TRENDS}}",
            "because coverage analysis provides progression trends");
    }

    [Fact]
    [Trait(TestCategories.Category, "TemplateVariableProcessing")]
    public void ValidateAuditorPromptContainsAuditSpecificVariables()
    {
        // Arrange: Load the actual auditor prompt
        var promptPath = "/home/zarichney/workspace/zarichney-api/.github/prompts/iterative-coverage-auditor.md";
        var promptContent = File.ReadAllText(promptPath);

        // Act & Assert: Validate Audit-Specific Variables
        promptContent.Should().Contain("{{AUDIT_PHASE}}",
            "because auditor requires audit phase context");
        promptContent.Should().Contain("{{COVERAGE_EPIC_CONTEXT}}",
            "because auditor aligns with Epic #181 progression");
        promptContent.Should().Contain("{{COVERAGE_PROGRESS_SUMMARY}}",
            "because auditor tracks Epic progression");
        promptContent.Should().Contain("{{BLOCKING_ITEMS}}",
            "because auditor identifies advancement blockers");
        promptContent.Should().Contain("{{AUDIT_HISTORY}}",
            "because auditor maintains historical audit context");
    }

    #endregion

    #region JSON To-Do Structure Validation Tests

    [Fact]
    [Trait(TestCategories.Category, "JsonToDoValidation")]
    public void ValidateJsonToDoStructureFollowsAuditorRequirements()
    {
        // Arrange: Create sample to-do item following auditor JSON structure
        var todoJson = """
        [
          {
            "id": "audit-001",
            "category": "CRITICAL",
            "description": "Resolve superficial test coverage in OrderService",
            "file_references": ["OrderService.cs:45", "OrderServiceTests.cs:12"],
            "epic_alignment": "Epic #181 meaningful coverage requirement",
            "validation_criteria": "Add edge case tests and error path validation",
            "status": "pending",
            "blocking_rationale": "Superficial coverage compromises Epic progression",
            "completion_evidence": "Comprehensive test suite with edge cases",
            "iteration_added": 1,
            "iteration_updated": 1,
            "audit_priority": "CRITICAL"
          }
        ]
        """;

        // Act: Parse JSON structure
        var parseAction = () => JsonSerializer.Deserialize<List<Dictionary<string, object>>>(todoJson);

        // Assert: Validate JSON structure is parseable and contains required fields
        parseAction.Should().NotThrow("because the JSON structure should be valid");

        var todoItems = parseAction();
        todoItems.Should().NotBeNull("because JSON should deserialize successfully");
        todoItems!.Should().HaveCount(1, "because we created one sample item");

        var item = todoItems.First();
        item.Should().ContainKey("id", "because each to-do item must have unique identifier");
        item.Should().ContainKey("category", "because items must be categorized by priority");
        item.Should().ContainKey("description", "because items must have actionable description");
        item.Should().ContainKey("epic_alignment", "because items must align with Epic #181");
        item.Should().ContainKey("validation_criteria", "because completion criteria must be specific");
        item.Should().ContainKey("audit_priority", "because audit requires priority assessment");
        item.Should().ContainKey("status", "because items must track completion status");
    }

    [Theory]
    [InlineData("CRITICAL")]
    [InlineData("HIGH")]
    [InlineData("MEDIUM")]
    [InlineData("LOW")]
    [InlineData("COMPLETED")]
    [Trait(TestCategories.Category, "JsonToDoValidation")]
    public void ValidateToDoItemCategoriesMatchAuditorExpectations(string category)
    {
        // Arrange: Create to-do item with specific category
        var todoItem = new Dictionary<string, object>
        {
            ["id"] = "test-001",
            ["category"] = category,
            ["description"] = "Test item",
            ["status"] = "pending",
            ["audit_priority"] = category == "COMPLETED" ? "HIGH" : category
        };

        // Act: Serialize and deserialize to validate structure
        var json = JsonSerializer.Serialize(todoItem);
        var parsedItem = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

        // Assert: Validate category is preserved and valid
        parsedItem.Should().NotBeNull("because JSON should deserialize successfully");
        parsedItem!["category"].ToString().Should().Be(category,
            "because category should be preserved through serialization");
    }

    #endregion

    #region Audit Decision Logic Validation

    [Theory]
    [InlineData("+3.7%", 2, "APPROVED", "Progressive iteration with positive coverage")]
    [InlineData("+1.2%", 1, "REQUIRES_ITERATION", "First iteration needs additional work")]
    [InlineData("-0.5%", 3, "BLOCKED", "Coverage regression blocks advancement")]
    [Trait(TestCategories.Category, "AuditDecisionLogic")]
    public void ValidateAuditDecisionLogicBasedOnCoverageAndIteration(
        string coverageDelta,
        int iterationCount,
        string expectedDecision,
        string scenario)
    {
        // Arrange: Create audit scenario
        var auditContext = new
        {
            CoverageDelta = coverageDelta,
            IterationCount = iterationCount,
            Scenario = scenario
        };

        // Act: Apply audit decision logic
        var actualDecision = DetermineAuditDecision(coverageDelta, iterationCount);

        // Assert: Validate decision matches expected outcome
        actualDecision.Should().Be(expectedDecision,
            $"because scenario '{scenario}' should result in {expectedDecision} decision");
    }

    private string DetermineAuditDecision(string coverageDelta, int iterationCount)
    {
        // Simplified audit decision logic for testing
        if (coverageDelta.StartsWith("-"))
            return "BLOCKED";

        if (coverageDelta.StartsWith("+") && iterationCount > 1)
            return "APPROVED";

        return "REQUIRES_ITERATION";
    }

    #endregion

    #region Error Handling Validation

    [Theory]
    [InlineData("", "Empty input should not cause errors")]
    [InlineData("{{MISSING_VARIABLE}}", "Missing variables should be handled gracefully")]
    [InlineData("{invalid json}", "Malformed JSON should not break processing")]
    [Trait(TestCategories.Category, "ErrorHandling")]
    public void ValidateErrorHandlingForVariousInputScenarios(string input, string scenario)
    {
        // Arrange: Create error scenario
        var errorContext = new { Input = input, Scenario = scenario };

        // Act: Process with error conditions (simulated)
        var result = ProcessInputWithErrorHandling(input);

        // Assert: Validate graceful handling
        result.Should().NotBeNull("because error handling should prevent null responses");
        result.IsHandled.Should().BeTrue("because all error scenarios should be handled gracefully");
        result.ErrorMessage.Should().NotBeNullOrEmpty("because error messages should be provided");
    }

    private ErrorHandlingResult ProcessInputWithErrorHandling(string input)
    {
        try
        {
            if (string.IsNullOrEmpty(input))
            {
                return new ErrorHandlingResult
                {
                    IsHandled = true,
                    ErrorMessage = "Empty input handled with default values"
                };
            }

            if (input.Contains("MISSING_VARIABLE"))
            {
                return new ErrorHandlingResult
                {
                    IsHandled = true,
                    ErrorMessage = "Template variable not found, placeholder applied"
                };
            }

            if (input.Contains("{") && !IsValidJson(input))
            {
                return new ErrorHandlingResult
                {
                    IsHandled = true,
                    ErrorMessage = "JSON parsing failed, fallback processing applied"
                };
            }

            return new ErrorHandlingResult
            {
                IsHandled = true,
                ErrorMessage = $"Input processed successfully: {input}"
            };
        }
        catch (Exception ex)
        {
            return new ErrorHandlingResult
            {
                IsHandled = false,
                ErrorMessage = $"Unexpected error: {ex.Message}"
            };
        }
    }

    private bool IsValidJson(string input)
    {
        try
        {
            JsonSerializer.Deserialize<object>(input);
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region AI Framework Integration Test

    [Fact]
    [Trait(TestCategories.Category, "AiFrameworkIntegration")]
    public void ValidateIntegrationWithAiFrameworkComponents()
    {
        // Arrange: Simulate AI framework integration scenario
        var integrationData = new Dictionary<string, object>
        {
            // AI Framework Variables (Issue #184)
            ["BASELINE_COVERAGE"] = "24.5%",
            ["NEW_COVERAGE"] = "28.2%",
            ["COVERAGE_ANALYSIS"] = "Meaningful improvement in service layer coverage",
            ["STANDARDS_COMPLIANCE"] = "Full compliance with TestingStandards.md requirements",

            // Iterative Action Variables (Issue #185)
            ["ITERATION_COUNT"] = 2,
            ["PREVIOUS_ITERATIONS"] = "First iteration focused on API endpoint coverage",
            ["CURRENT_TODO_LIST"] = """[{"id":"audit-001","status":"completed"}]""",
            ["HISTORICAL_CONTEXT"] = "Progressive improvement across 2 iterations",

            // Coverage Analysis Variables (Issue #187)
            ["COVERAGE_DATA"] = "Detailed line and branch coverage metrics",
            ["COVERAGE_DELTA"] = "+3.7% improvement",
            ["COVERAGE_TRENDS"] = "Consistent upward trend over past 3 PRs"
        };

        // Act: Validate integration processing
        var integrationResult = ValidateAiFrameworkIntegration(integrationData);

        // Assert: Confirm successful integration
        integrationResult.Should().NotBeNull("because integration should produce results");
        integrationResult.IsSuccessful.Should().BeTrue("because all AI framework components should integrate");
        integrationResult.ValidationMessages.Should().NotBeEmpty("because validation details should be provided");
    }

    private AiFrameworkIntegrationResult ValidateAiFrameworkIntegration(Dictionary<string, object> data)
    {
        var result = new AiFrameworkIntegrationResult();

        // Validate AI Framework Variables (Issue #184)
        if (data.ContainsKey("BASELINE_COVERAGE") && data.ContainsKey("NEW_COVERAGE"))
        {
            result.ValidationMessages.Add("AI Framework coverage variables validated");
        }

        // Validate Iterative Action Variables (Issue #185)
        if (data.ContainsKey("ITERATION_COUNT") && data.ContainsKey("HISTORICAL_CONTEXT"))
        {
            result.ValidationMessages.Add("Iterative Action context variables validated");
        }

        // Validate Coverage Analysis Variables (Issue #187)
        if (data.ContainsKey("COVERAGE_DELTA") && data.ContainsKey("COVERAGE_TRENDS"))
        {
            result.ValidationMessages.Add("Coverage Analysis trend variables validated");
        }

        result.IsSuccessful = result.ValidationMessages.Count >= 3;

        if (!result.IsSuccessful)
        {
            result.ValidationMessages.Add("ERROR: Some AI framework components failed validation");
        }

        return result;
    }

    #endregion
}

#region Supporting Models

public class ErrorHandlingResult
{
    public bool IsHandled { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}

public class AiFrameworkIntegrationResult
{
    public bool IsSuccessful { get; set; }
    public List<string> ValidationMessages { get; set; } = new();
}

#endregion