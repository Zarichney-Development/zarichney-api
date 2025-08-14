using FluentAssertions;
using Xunit;
using System.IO;
using System.Threading.Tasks;
using Zarichney.Tests.Framework.Attributes;

namespace Zarichney.Tests.Integration.Services.WorkflowIntegration;

[Collection("IntegrationInfra")]
[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Feature, "Infrastructure")]
public class ClaudeDispatchWorkflowTests
{
    private readonly string _workflowPath = Path.Combine(
        Directory.GetCurrentDirectory(),
        "..",
        "..",
        "..",
        "..",
        "..",
        ".github",
        "workflows",
        "claude-dispatch.yml"
    );

    [Fact]
    [LogTestStartEnd]
    public async Task ClaudeDispatchWorkflow_ShouldExist_AndContainRequiredConfiguration()
    {
        // Act & Assert
        var normalizedPath = Path.GetFullPath(_workflowPath);
        File.Exists(normalizedPath).Should().BeTrue(
            because: "Claude dispatch workflow file should exist at expected location");

        var content = await File.ReadAllTextAsync(normalizedPath);
        
        // Verify essential workflow configuration
        content.Should().Contain("name: Claude Code Dispatch",
            because: "workflow should have correct name");
        content.Should().Contain("repository_dispatch:",
            because: "workflow should be triggered by repository dispatch");
        content.Should().Contain("types: [claude-dispatch]",
            because: "workflow should listen for claude-dispatch event type");
        content.Should().Contain("ref: develop",
            because: "workflow should checkout develop branch by default");
    }

    [Fact]
    [LogTestStartEnd]
    public async Task ClaudeDispatchWorkflow_ShouldHaveRequiredPermissions()
    {
        // Arrange
        var normalizedPath = Path.GetFullPath(_workflowPath);
        File.Exists(normalizedPath).Should().BeTrue();

        // Act
        var content = await File.ReadAllTextAsync(normalizedPath);

        // Assert critical permissions
        content.Should().Contain("contents: write",
            because: "Claude needs to write files and create commits");
        content.Should().Contain("pull-requests: write",
            because: "Claude needs to create pull requests");
        content.Should().Contain("issues: write",
            because: "Claude needs to interact with GitHub issues");
    }

    [Fact]
    [LogTestStartEnd]
    public async Task ClaudeDispatchWorkflow_ShouldIncludeTestBaseline_Generation()
    {
        // Arrange
        var normalizedPath = Path.GetFullPath(_workflowPath);
        File.Exists(normalizedPath).Should().BeTrue();

        // Act
        var content = await File.ReadAllTextAsync(normalizedPath);

        // Assert test baseline generation
        content.Should().Contain("Generate Initial Test Results",
            because: "workflow should generate test baseline for Claude context");
        content.Should().Contain("./Scripts/run-test-suite.sh report summary",
            because: "workflow should use unified test suite for baseline");
        content.Should().Contain("test_baseline.txt",
            because: "workflow should save test results to baseline file");
    }

    [Fact]
    [LogTestStartEnd]
    public async Task ClaudeDispatchWorkflow_ShouldConfigureProperEnvironment()
    {
        // Arrange
        var normalizedPath = Path.GetFullPath(_workflowPath);
        File.Exists(normalizedPath).Should().BeTrue();

        // Act
        var content = await File.ReadAllTextAsync(normalizedPath);

        // Assert environment setup
        content.Should().Contain("dotnet-version: '8.0.x'",
            because: "workflow should specify correct .NET version");
        content.Should().Contain("node-version: '18.x'",
            because: "workflow should specify correct Node.js version for Angular frontend");
        content.Should().Contain("setup-dotnet: 'true'",
            because: "workflow should enable .NET setup");
        content.Should().Contain("cache-dependencies: 'true'",
            because: "workflow should enable dependency caching for performance");
    }

    [Fact]
    [LogTestStartEnd]
    public async Task ClaudeDispatchWorkflow_ShouldAllowRequiredBashCommands()
    {
        // Arrange
        var normalizedPath = Path.GetFullPath(_workflowPath);
        File.Exists(normalizedPath).Should().BeTrue();

        // Act
        var content = await File.ReadAllTextAsync(normalizedPath);

        // Assert allowed tools configuration
        var requiredCommands = new[]
        {
            "Bash(dotnet build:*)",
            "Bash(dotnet test:*)", 
            "Bash(git:*)",
            "Bash(gh:*)",
            "Bash(./Scripts/*)"
        };

        foreach (var command in requiredCommands)
        {
            content.Should().Contain(command,
                because: $"Claude should be allowed to run {command} for development tasks");
        }
    }

    [Fact]
    [LogTestStartEnd]
    public async Task ClaudeDispatchWorkflow_ShouldCaptureArtifacts()
    {
        // Arrange
        var normalizedPath = Path.GetFullPath(_workflowPath);
        File.Exists(normalizedPath).Should().BeTrue();

        // Act
        var content = await File.ReadAllTextAsync(normalizedPath);

        // Assert artifact configuration
        content.Should().Contain("Upload Test Artifacts",
            because: "workflow should upload test results as artifacts");
        content.Should().Contain("TestResults/",
            because: "workflow should capture test result files");
        content.Should().Contain("CoverageReport/",
            because: "workflow should capture coverage reports");
        content.Should().Contain("test_baseline.txt",
            because: "workflow should preserve test baseline for analysis");
    }
}