using Microsoft.Extensions.Configuration;
using Zarichney.Tests.Framework.Attributes;
using Zarichney.Tests.Framework.Fixtures;
using static Zarichney.Tests.Framework.Helpers.TestEnvironmentHelper;

namespace Zarichney.Tests.Framework.Helpers;

/// <summary>
/// Helper for loading and validating test suite standards and baselines.
/// Provides methods for environment-aware threshold validation and skip categorization.
/// </summary>
public static class TestSuiteStandardsHelper
{
  /// <summary>
  /// Configuration model for test suite standards loaded from appsettings.Testing.json.
  /// </summary>
  public record TestSuiteStandards(
    SkipThresholds SkipThresholds,
    CoverageBaselines CoverageBaselines,
    SkipCategories SkipCategories,
    QualityGates QualityGates);

  /// <summary>
  /// Configuration for skip thresholds by environment.
  /// </summary>
  public record SkipThresholds(
    Dictionary<string, EnvironmentThreshold> Environments);

  /// <summary>
  /// Threshold configuration for a specific environment.
  /// </summary>
  public record EnvironmentThreshold(
    double MaxSkipPercentage,
    string Description);

  /// <summary>
  /// Configuration for coverage baselines and progressive targets.
  /// </summary>
  public record CoverageBaselines(
    double Current,
    double IncrementSize,
    double RegressionTolerance,
    List<double> ProgressiveTargets,
    string Description);

  /// <summary>
  /// Configuration for skip categorization system.
  /// </summary>
  public record SkipCategories(
    SkipCategoryInfo ExternalServices,
    SkipCategoryInfo Infrastructure,
    SkipCategoryInfo ProductionSafety,
    SkipCategoryInfo HardcodedSkips);

  /// <summary>
  /// Information about a specific skip category.
  /// </summary>
  public record SkipCategoryInfo(
    string Type,
    bool EnvironmentDependent,
    List<string>? Services,
    List<string>? Dependencies,
    List<string>? TestCategories,
    string? Reason,
    string? Action,
    List<string>? Examples,
    string? Description);

  /// <summary>
  /// Configuration for quality gates and dynamic thresholds.
  /// </summary>
  public record QualityGates(
    DynamicThresholds DynamicThresholds,
    TrendAnalysis TrendAnalysis);

  /// <summary>
  /// Configuration for dynamic threshold calculations.
  /// </summary>
  public record DynamicThresholds(
    bool Enabled,
    int HistoricalSampleSize,
    List<string> ConfidenceLevels,
    string Description);

  /// <summary>
  /// Configuration for trend analysis features.
  /// </summary>
  public record TrendAnalysis(
    bool Enabled,
    bool LinearRegressionPrediction,
    bool AlertOnDegradation,
    double VolatilityThreshold);

  /// <summary>
  /// Result of validating test metrics against configured standards.
  /// </summary>
  public record ValidationResult(
    bool PassesThresholds,
    double ActualSkipPercentage,
    double AllowedSkipPercentage,
    TestEnvironmentClassification Environment,
    List<string> Violations,
    List<string> Recommendations,
    Dictionary<string, SkipAnalysis> SkipAnalysisByCategory);

  /// <summary>
  /// Analysis of skipped tests by category.
  /// </summary>
  public record SkipAnalysis(
    string CategoryType,
    int SkippedCount,
    int TotalCount,
    double SkipPercentage,
    bool IsExpected,
    List<string> ReasonCodes);

  /// <summary>
  /// Loads test suite standards from the configuration.
  /// </summary>
  /// <param name="configuration">The configuration instance to load from.</param>
  /// <returns>The loaded test suite standards, or null if not configured.</returns>
  public static TestSuiteStandards? LoadStandards(IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

    var section = configuration.GetSection("TestSuiteStandards");
    if (!section.Exists())
    {
      return null;
    }

    try
    {
      var skipThresholds = LoadSkipThresholds(section.GetSection("SkipThresholds"));
      var coverageBaselines = LoadCoverageBaselines(section.GetSection("CoverageBaselines"));
      var skipCategories = LoadSkipCategories(section.GetSection("SkipCategories"));
      var qualityGates = LoadQualityGates(section.GetSection("QualityGates"));

      return new TestSuiteStandards(skipThresholds, coverageBaselines, skipCategories, qualityGates);
    }
    catch (Exception ex)
    {
      throw new InvalidOperationException($"Failed to load test suite standards from configuration: {ex.Message}", ex);
    }
  }

  /// <summary>
  /// Gets the expected skip percentage for a specific environment classification.
  /// </summary>
  /// <param name="standards">The loaded test suite standards.</param>
  /// <param name="classification">The environment classification.</param>
  /// <returns>The expected skip percentage for the environment.</returns>
  public static double GetExpectedSkipPercentage(TestSuiteStandards standards, TestEnvironmentClassification classification)
  {
    ArgumentNullException.ThrowIfNull(standards, nameof(standards));

    var environmentKey = classification switch
    {
      TestEnvironmentClassification.Unconfigured => "unconfigured",
      TestEnvironmentClassification.Configured => "configured",
      TestEnvironmentClassification.Production => "production",
      _ => "unconfigured"
    };

    return standards.SkipThresholds.Environments.TryGetValue(environmentKey, out var threshold)
      ? threshold.MaxSkipPercentage
      : 100.0; // Default to allowing all skips if not configured
  }

  /// <summary>
  /// Validates test results against the configured standards.
  /// </summary>
  /// <param name="standards">The loaded test suite standards.</param>
  /// <param name="environmentInfo">Information about the current test environment.</param>
  /// <param name="totalTests">Total number of tests executed.</param>
  /// <param name="skippedTests">Number of tests skipped.</param>
  /// <param name="skippedTestsByCategory">Dictionary of skipped tests grouped by test category.</param>
  /// <returns>Validation result with pass/fail status and detailed analysis.</returns>
  public static ValidationResult ValidateTestResults(
    TestSuiteStandards standards,
    TestEnvironmentInfo environmentInfo,
    int totalTests,
    int skippedTests,
    Dictionary<string, int> skippedTestsByCategory)
  {
    ArgumentNullException.ThrowIfNull(standards, nameof(standards));
    ArgumentNullException.ThrowIfNull(environmentInfo, nameof(environmentInfo));

    var actualSkipPercentage = totalTests > 0 ? (double)skippedTests / totalTests * 100.0 : 0.0;
    var allowedSkipPercentage = environmentInfo.ExpectedSkipPercentage;
    var violations = new List<string>();
    var recommendations = new List<string>();
    var skipAnalysisByCategory = new Dictionary<string, SkipAnalysis>();

    // Analyze skips by category
    AnalyzeSkipsByCategory(standards, skippedTestsByCategory, totalTests, skipAnalysisByCategory, violations, recommendations);

    // Check overall threshold
    var passesThresholds = actualSkipPercentage <= allowedSkipPercentage;
    if (!passesThresholds)
    {
      violations.Add($"Skip rate {actualSkipPercentage:F1}% exceeds threshold of {allowedSkipPercentage:F1}% for {environmentInfo.Classification} environment");
    }

    // Add environment-specific recommendations
    if (environmentInfo.Classification == TestEnvironmentClassification.Unconfigured && environmentInfo.MissingConfigurations.Count > 0)
    {
      recommendations.Add($"Configure missing services to reduce skip rate: {string.Join(", ", environmentInfo.MissingConfigurations.Take(3))}");
    }

    return new ValidationResult(
      passesThresholds && violations.Count == 0,
      actualSkipPercentage,
      allowedSkipPercentage,
      environmentInfo.Classification,
      violations,
      recommendations,
      skipAnalysisByCategory);
  }

  /// <summary>
  /// Categorizes a test skip reason based on configured categories.
  /// </summary>
  /// <param name="standards">The loaded test suite standards.</param>
  /// <param name="testCategory">The test category (trait value).</param>
  /// <param name="skipReason">The reason the test was skipped.</param>
  /// <returns>The skip category type and whether it's expected.</returns>
  public static (string CategoryType, bool IsExpected) CategorizeSkip(
    TestSuiteStandards standards,
    string testCategory,
    string? skipReason = null)
  {
    ArgumentNullException.ThrowIfNull(standards, nameof(standards));

    // Check external services
    if (standards.SkipCategories.ExternalServices.TestCategories?.Contains(testCategory) == true)
    {
      return (standards.SkipCategories.ExternalServices.Type, true);
    }

    // Check infrastructure
    if (standards.SkipCategories.Infrastructure.TestCategories?.Contains(testCategory) == true)
    {
      return (standards.SkipCategories.Infrastructure.Type, true);
    }

    // Check for hardcoded skips based on skip reason
    if (!string.IsNullOrEmpty(skipReason) && skipReason.Contains("Skip"))
    {
      return (standards.SkipCategories.HardcodedSkips.Type, false);
    }

    // Default to production safety for unknown skips
    return (standards.SkipCategories.ProductionSafety.Type, true);
  }

  #region Private Helper Methods

  private static SkipThresholds LoadSkipThresholds(IConfigurationSection section)
  {
    var environments = new Dictionary<string, EnvironmentThreshold>();
    var environmentsSection = section.GetSection("Environments");

    foreach (var envSection in environmentsSection.GetChildren())
    {
      var maxSkipPercentage = envSection.GetValue<double>("maxSkipPercentage");
      var description = envSection.GetValue<string>("description") ?? string.Empty;
      environments[envSection.Key] = new EnvironmentThreshold(maxSkipPercentage, description);
    }

    return new SkipThresholds(environments);
  }

  private static CoverageBaselines LoadCoverageBaselines(IConfigurationSection section)
  {
    var current = section.GetValue<double>("current");
    var incrementSize = section.GetValue<double>("incrementSize");
    var regressionTolerance = section.GetValue<double>("regressionTolerance");
    var description = section.GetValue<string>("description") ?? string.Empty;
    var progressiveTargets = section.GetSection("progressiveTargets").Get<List<double>>() ?? new List<double>();

    return new CoverageBaselines(current, incrementSize, regressionTolerance, progressiveTargets, description);
  }

  private static SkipCategories LoadSkipCategories(IConfigurationSection section)
  {
    var externalServices = LoadSkipCategoryInfo(section.GetSection("externalServices"));
    var infrastructure = LoadSkipCategoryInfo(section.GetSection("infrastructure"));
    var productionSafety = LoadSkipCategoryInfo(section.GetSection("productionSafety"));
    var hardcodedSkips = LoadSkipCategoryInfo(section.GetSection("hardcodedSkips"));

    return new SkipCategories(externalServices, infrastructure, productionSafety, hardcodedSkips);
  }

  private static SkipCategoryInfo LoadSkipCategoryInfo(IConfigurationSection section)
  {
    var type = section.GetValue<string>("type") ?? string.Empty;
    var environmentDependent = section.GetValue<bool>("environmentDependent");
    var services = section.GetSection("services").Get<List<string>>();
    var dependencies = section.GetSection("dependencies").Get<List<string>>();
    var testCategories = section.GetSection("testCategories").Get<List<string>>();
    var reason = section.GetValue<string>("reason");
    var action = section.GetValue<string>("action");
    var examples = section.GetSection("examples").Get<List<string>>();
    var description = section.GetValue<string>("description");

    return new SkipCategoryInfo(type, environmentDependent, services, dependencies, testCategories, reason, action, examples, description);
  }

  private static QualityGates LoadQualityGates(IConfigurationSection section)
  {
    var dynamicThresholds = LoadDynamicThresholds(section.GetSection("dynamicThresholds"));
    var trendAnalysis = LoadTrendAnalysis(section.GetSection("trendAnalysis"));

    return new QualityGates(dynamicThresholds, trendAnalysis);
  }

  private static DynamicThresholds LoadDynamicThresholds(IConfigurationSection section)
  {
    var enabled = section.GetValue<bool>("enabled");
    var historicalSampleSize = section.GetValue<int>("historicalSampleSize");
    var confidenceLevels = section.GetSection("confidenceLevels").Get<List<string>>() ?? new List<string>();
    var description = section.GetValue<string>("description") ?? string.Empty;

    return new DynamicThresholds(enabled, historicalSampleSize, confidenceLevels, description);
  }

  private static TrendAnalysis LoadTrendAnalysis(IConfigurationSection section)
  {
    var enabled = section.GetValue<bool>("enabled");
    var linearRegressionPrediction = section.GetValue<bool>("linearRegressionPrediction");
    var alertOnDegradation = section.GetValue<bool>("alertOnDegradation");
    var volatilityThreshold = section.GetValue<double>("volatilityThreshold");

    return new TrendAnalysis(enabled, linearRegressionPrediction, alertOnDegradation, volatilityThreshold);
  }

  private static void AnalyzeSkipsByCategory(
    TestSuiteStandards standards,
    Dictionary<string, int> skippedTestsByCategory,
    int totalTests,
    Dictionary<string, SkipAnalysis> skipAnalysisByCategory,
    List<string> violations,
    List<string> recommendations)
  {
    foreach (var categorySkips in skippedTestsByCategory)
    {
      var (categoryType, isExpected) = CategorizeSkip(standards, categorySkips.Key);
      var skipPercentage = totalTests > 0 ? (double)categorySkips.Value / totalTests * 100.0 : 0.0;
      var reasonCodes = new List<string> { categoryType };

      var analysis = new SkipAnalysis(
        categoryType,
        categorySkips.Value,
        totalTests,
        skipPercentage,
        isExpected,
        reasonCodes);

      skipAnalysisByCategory[categorySkips.Key] = analysis;

      // Add category-specific violations and recommendations
      if (!isExpected)
      {
        violations.Add($"Unexpected skips in {categorySkips.Key}: {categorySkips.Value} tests ({skipPercentage:F1}%)");
      }

      if (categoryType == "problematic")
      {
        recommendations.Add($"Review and eliminate hardcoded Skip attributes in {categorySkips.Key} tests");
      }
    }
  }

  #endregion
}