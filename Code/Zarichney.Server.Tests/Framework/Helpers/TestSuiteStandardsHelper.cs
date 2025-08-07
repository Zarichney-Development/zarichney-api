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
    string Description,
    DateTime? TargetDate,
    double? MonthlyVelocityTarget,
    List<string>? PriorityAreas);

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
  /// Progressive coverage analysis result.
  /// </summary>
  public record ProgressiveCoverageAnalysis(
    double CurrentCoverage,
    double NextTarget,
    double CoverageGap,
    int CurrentPhase,
    string PhaseDescription,
    bool IsOnTrack,
    double MonthsToTarget,
    double RequiredVelocity,
    double ActualVelocity,
    List<string> Recommendations,
    List<string> PriorityAreas);

  /// <summary>
  /// Coverage progression information for a specific phase.
  /// </summary>
  public record CoveragePhaseInfo(
    int Phase,
    double FromCoverage,
    double ToCoverage,
    string Description,
    List<string> FocusAreas,
    string Strategy);

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

  /// <summary>
  /// Gets the next coverage target based on current coverage.
  /// </summary>
  /// <param name="standards">The loaded test suite standards.</param>
  /// <param name="currentCoverage">The current coverage percentage.</param>
  /// <returns>The next target coverage percentage.</returns>
  public static double GetNextCoverageTarget(TestSuiteStandards standards, double currentCoverage)
  {
    ArgumentNullException.ThrowIfNull(standards, nameof(standards));

    var targets = standards.CoverageBaselines.ProgressiveTargets.OrderBy(t => t).ToList();
    
    // Find the first target higher than current coverage
    var nextTarget = targets.FirstOrDefault(t => t > currentCoverage);
    
    // If no target found, return the highest target
    return nextTarget > 0 ? nextTarget : targets.LastOrDefault();
  }

  /// <summary>
  /// Calculates the coverage gap to the next target.
  /// </summary>
  /// <param name="standards">The loaded test suite standards.</param>
  /// <param name="currentCoverage">The current coverage percentage.</param>
  /// <returns>The coverage gap percentage.</returns>
  public static double CalculateCoverageGap(TestSuiteStandards standards, double currentCoverage)
  {
    ArgumentNullException.ThrowIfNull(standards, nameof(standards));

    var nextTarget = GetNextCoverageTarget(standards, currentCoverage);
    return Math.Max(0, nextTarget - currentCoverage);
  }

  /// <summary>
  /// Analyzes progressive coverage status and provides recommendations.
  /// </summary>
  /// <param name="standards">The loaded test suite standards.</param>
  /// <param name="currentCoverage">The current coverage percentage.</param>
  /// <param name="historicalCoverage">Historical coverage data for velocity calculation (optional).</param>
  /// <returns>Progressive coverage analysis with recommendations.</returns>
  public static ProgressiveCoverageAnalysis AnalyzeProgressiveCoverage(
    TestSuiteStandards standards,
    double currentCoverage,
    List<(DateTime Date, double Coverage)>? historicalCoverage = null)
  {
    ArgumentNullException.ThrowIfNull(standards, nameof(standards));

    var nextTarget = GetNextCoverageTarget(standards, currentCoverage);
    var coverageGap = CalculateCoverageGap(standards, currentCoverage);
    var phase = GetCurrentPhase(standards, currentCoverage);
    var phaseInfo = GetPhaseInfo(phase.Phase);

    // Calculate velocity and timeline predictions
    var actualVelocity = CalculateActualVelocity(historicalCoverage);
    var targetDate = standards.CoverageBaselines.TargetDate ?? DateTime.Now.AddYears(2);
    var monthsToTarget = (targetDate - DateTime.Now).TotalDays / 30.44;
    var requiredVelocity = standards.CoverageBaselines.MonthlyVelocityTarget ?? 
                          (monthsToTarget > 0 ? (90.0 - currentCoverage) / monthsToTarget : 0);
    
    var isOnTrack = actualVelocity >= requiredVelocity * 0.8; // 80% tolerance

    // Generate recommendations
    var recommendations = GetCoverageRecommendations(standards, currentCoverage, phase, isOnTrack);
    var priorityAreas = standards.CoverageBaselines.PriorityAreas?.ToList() ?? new List<string>();

    return new ProgressiveCoverageAnalysis(
      currentCoverage,
      nextTarget,
      coverageGap,
      phase.Phase,
      phase.Description,
      isOnTrack,
      monthsToTarget,
      requiredVelocity,
      actualVelocity,
      recommendations,
      priorityAreas);
  }

  /// <summary>
  /// Determines if coverage progression is on track for target timeline.
  /// </summary>
  /// <param name="standards">The loaded test suite standards.</param>
  /// <param name="currentCoverage">The current coverage percentage.</param>
  /// <param name="historicalCoverage">Historical coverage data for trend analysis.</param>
  /// <returns>True if progression is on track, false otherwise.</returns>
  public static bool IsCoverageProgressionOnTrack(
    TestSuiteStandards standards,
    double currentCoverage,
    List<(DateTime Date, double Coverage)>? historicalCoverage = null)
  {
    var analysis = AnalyzeProgressiveCoverage(standards, currentCoverage, historicalCoverage);
    return analysis.IsOnTrack;
  }

  /// <summary>
  /// Gets coverage recommendations based on current phase and status.
  /// </summary>
  /// <param name="standards">The loaded test suite standards.</param>
  /// <param name="currentCoverage">The current coverage percentage.</param>
  /// <param name="phase">The current coverage phase.</param>
  /// <param name="isOnTrack">Whether progression is on track.</param>
  /// <returns>List of actionable recommendations.</returns>
  public static List<string> GetCoverageRecommendations(
    TestSuiteStandards standards,
    double currentCoverage,
    CoveragePhaseInfo phase,
    bool isOnTrack)
  {
    ArgumentNullException.ThrowIfNull(standards, nameof(standards));
    ArgumentNullException.ThrowIfNull(phase, nameof(phase));

    var recommendations = new List<string>();
    var nextTarget = GetNextCoverageTarget(standards, currentCoverage);
    var gap = nextTarget - currentCoverage;

    // General progression recommendations
    recommendations.Add($"Target: {nextTarget:F1}% coverage (Gap: {gap:F1}%)");
    recommendations.Add($"Current Phase: {phase.Description}");

    // Phase-specific recommendations
    recommendations.AddRange(phase.FocusAreas.Select(area => $"Focus Area: {area}"));
    recommendations.Add($"Strategy: {phase.Strategy}");

    // Track-specific recommendations
    if (!isOnTrack)
    {
      recommendations.Add("⚠️ Coverage progression behind schedule");
      recommendations.Add("Consider increasing test development velocity");
      recommendations.Add("Prioritize high-impact test scenarios");
    }
    else
    {
      recommendations.Add("✅ Coverage progression on track");
      recommendations.Add("Continue current testing strategy");
    }

    // Add priority areas if configured
    var priorityAreas = standards.CoverageBaselines.PriorityAreas;
    if (priorityAreas?.Count > 0)
    {
      recommendations.Add($"Priority Areas: {string.Join(", ", priorityAreas)}");
    }

    return recommendations;
  }

  /// <summary>
  /// Gets the current coverage phase based on current coverage percentage.
  /// </summary>
  /// <param name="standards">The loaded test suite standards.</param>
  /// <param name="currentCoverage">The current coverage percentage.</param>
  /// <returns>The current coverage phase information.</returns>
  private static CoveragePhaseInfo GetCurrentPhase(TestSuiteStandards standards, double currentCoverage)
  {
    var targets = standards.CoverageBaselines.ProgressiveTargets.OrderBy(t => t).ToList();
    var baseline = standards.CoverageBaselines.Current;
    
    // Phase 1: Baseline to first target
    if (currentCoverage < targets.FirstOrDefault())
    {
      return GetPhaseInfo(1);
    }
    
    // Find which phase we're in based on targets achieved
    for (int i = 0; i < targets.Count - 1; i++)
    {
      if (currentCoverage >= targets[i] && currentCoverage < targets[i + 1])
      {
        return GetPhaseInfo(i + 2); // Phase 2, 3, 4, etc.
      }
    }
    
    // Final phase or beyond
    return GetPhaseInfo(targets.Count + 1);
  }

  /// <summary>
  /// Gets phase information for a specific phase number.
  /// </summary>
  /// <param name="phaseNumber">The phase number.</param>
  /// <returns>The phase information.</returns>
  private static CoveragePhaseInfo GetPhaseInfo(int phaseNumber)
  {
    return phaseNumber switch
    {
      1 => new CoveragePhaseInfo(
        1, 14.22, 20.0, "Foundation Phase - Basic Coverage",
        new List<string> { "Service layers", "Core business logic", "API contracts" },
        "Focus on low-hanging fruit and broad coverage across key components"),
        
      2 => new CoveragePhaseInfo(
        2, 20.0, 35.0, "Growth Phase - Service Layer Depth",
        new List<string> { "Service method coverage", "Integration scenarios", "Data validation" },
        "Deepen coverage in service layers and integration points"),
        
      3 => new CoveragePhaseInfo(
        3, 35.0, 50.0, "Maturity Phase - Edge Cases & Error Handling",
        new List<string> { "Error handling", "Edge cases", "Input validation", "Boundary conditions" },
        "Focus on edge cases, error scenarios, and robust error handling"),
        
      4 => new CoveragePhaseInfo(
        4, 50.0, 75.0, "Excellence Phase - Complex Scenarios",
        new List<string> { "Complex business scenarios", "Integration depth", "Cross-cutting concerns" },
        "Cover complex business scenarios and deep integration testing"),
        
      5 => new CoveragePhaseInfo(
        5, 75.0, 90.0, "Mastery Phase - Comprehensive Coverage",
        new List<string> { "Performance scenarios", "Comprehensive edge cases", "System integration" },
        "Achieve comprehensive coverage including performance and system-wide scenarios"),
        
      _ => new CoveragePhaseInfo(
        6, 90.0, 100.0, "Optimization Phase - Maintenance & Optimization",
        new List<string> { "Performance optimization", "Test maintenance", "Documentation" },
        "Maintain high coverage and optimize test suite performance")
    };
  }

  /// <summary>
  /// Calculates actual velocity from historical coverage data.
  /// </summary>
  /// <param name="historicalCoverage">Historical coverage data points.</param>
  /// <returns>The calculated monthly velocity percentage.</returns>
  private static double CalculateActualVelocity(List<(DateTime Date, double Coverage)>? historicalCoverage)
  {
    if (historicalCoverage == null || historicalCoverage.Count < 2)
    {
      return 0.0; // No data to calculate velocity
    }
    
    var sortedData = historicalCoverage.OrderBy(h => h.Date).ToList();
    var oldest = sortedData.First();
    var newest = sortedData.Last();
    
    var coverageDelta = newest.Coverage - oldest.Coverage;
    var timeDelta = (newest.Date - oldest.Date).TotalDays / 30.44; // Convert to months
    
    return timeDelta > 0 ? coverageDelta / timeDelta : 0.0;
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
    
    // New Phase 3 fields
    var targetDateString = section.GetValue<string>("targetDate");
    var targetDate = DateTime.TryParse(targetDateString, out var parsedDate) ? parsedDate : (DateTime?)null;
    var monthlyVelocityTarget = section.GetValue<double?>("monthlyVelocityTarget");
    var priorityAreas = section.GetSection("priorityAreas").Get<List<string>>();

    return new CoverageBaselines(current, incrementSize, regressionTolerance, progressiveTargets, description, 
      targetDate, monthlyVelocityTarget, priorityAreas);
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