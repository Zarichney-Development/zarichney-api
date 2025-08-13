# Test Framework Configuration Externalization

Version: 0.1 (Initial Draft for Issue #102)
Related Issue: #102 (Test Framework Configuration Externalization)

## Objective
Externalize all test framework phase, threshold, and quality gate definitions from hardcoded logic in `TestSuiteStandardsHelper` to configuration-driven, validated, and easily adjustable JSON/Options sources.

## Scope
- Coverage phase roadmap (phase ranges, descriptions, focus areas, strategies)
- Progressive targets & baseline values
- Skip thresholds by environment classification
- Dynamic quality gate parameters (pass rate, regression tolerance, volatility, velocity targets)
- Dependency attribution consistency (enum-based vs legacy trait-based)
- Diagnostic tooling for visibility into effective configuration

## Target Outcomes
| Outcome | Description |
|---------|-------------|
| Externalized Phases | Remove `GetPhaseInfo` hardcoded switch; load from configuration provider |
| Unified Threshold Source | Single authoritative config section for skips, coverage, velocity |
| Fast-Fail Validation | Detect invalid phase ordering, overlaps, or regression in config before tests run |
| Attribute Consistency | Prefer `DependencyFact` over legacy traits for dependency-driven skips |
| Backward Compatibility | Feature flag to rollback to static definitions if needed |
| Diagnostics | Command / helper to dump merged effective configuration |

## Configuration Structure (Proposed)
```jsonc
{
  "TestFramework": {
    "UseConfigPhases": true,
    "Phases": [
      {
        "phase": 1,
        "from": 14.22,
        "to": 20.0,
        "name": "Foundation",
        "description": "Basic Coverage",
        "focusAreas": ["Service layers", "Core business logic", "API contracts"],
        "strategy": "Focus on low-hanging fruit and broad coverage across key components"
      },
      {
        "phase": 2,
        "from": 20.0,
        "to": 35.0,
        "name": "Growth",
        "description": "Service Layer Depth",
        "focusAreas": ["Service method coverage", "Integration scenarios", "Data validation"],
        "strategy": "Deepen coverage in service layers and integration points"
      }
      // ... additional phases ...
    ],
    "Coverage": {
      "baseline": 14.22,
      "progressiveTargets": [20.0,35.0,50.0,75.0,90.0],
      "regressionTolerance": 1.0,
      "targetDate": "2026-01-31",
      "monthlyVelocityTarget": 2.8
    },
    "SkipThresholds": {
      "environments": {
        "unconfigured": {"maxSkipPercentage": 26.7, "description": "Local / partial env"},
        "configured": {"maxSkipPercentage": 1.2, "description": "Fully configured"},
        "production": {"maxSkipPercentage": 0.0, "description": "Prod validation"}
      }
    },
    "QualityGates": {
      "passRateBase": 0.99,
      "passRateIncrementPerPhase": 0.005,
      "regressionToleranceBase": 1.0,
      "regressionToleranceDecrementPerPhase": 0.1,
      "volatilityThreshold": 2.0
    }
  }
}
```

## Interfaces & Abstractions
```csharp
public interface ITestSuitePhaseProvider {
  IReadOnlyList<CoveragePhaseInfo> GetPhases();
  CoveragePhaseInfo GetCurrentPhase(double coverage);
}

public interface ITestFrameworkConfigValidator {
  void Validate(TestFrameworkConfig config); // throws DetailedValidationException on failure
}

public interface ITestFrameworkDiagnosticsService {
  string DumpEffectiveConfiguration(); // returns JSON string
}
```

## Validation Rules
1. Phases ordered ascending by `phase`
2. `from` of first phase <= baseline
3. Each `to` of phase N == `from` of phase N+1 (or explicit non-overlapping with gap explanation)
4. Final phase `to` >= highest progressive target
5. Progressive targets strictly ascending
6. Skip thresholds 0 <= value <= 100
7. Pass rate increments do not push requirement > 1.0
8. Regression tolerance never negative after adjustments

## Migration Steps
1. Introduce configuration section & POCOs (`TestFrameworkOptions`)
2. Implement provider reading config & building `CoveragePhaseInfo`
3. Add validator; integrate into test suite bootstrap (e.g., fixture constructor)
4. Replace `GetPhaseInfo` calls with provider usage; keep legacy method behind feature flag
5. Implement diagnostics dump (CLI or test helper invoked via script flag)
6. Audit test classes for legacy dependency traits; migrate to `DependencyFact`
7. Add analyzer or reflection-based check flagging legacy-only classes (warn)
8. Update documentation & link from `TestSuiteStandards.md`

## Backward Compatibility
Feature flag: `TestFramework:UseConfigPhases` (bool, default true). When false, uses legacy hardcoded switch and logs warning.

## Testing Strategy
- Unit: Phase parsing, validation failures (overlap, gap, out-of-order) – expect thrown exception
- Unit: GetCurrentPhase boundary edges (exact `to` values)
- Unit: Dump output snapshot test (approve JSON)
- Unit: DependencyFact migration detector (ensures detection logic accurate)
- Integration: Full test run with modified config adjusting a phase boundary (no rebuild)

## Risks & Mitigations
| Risk | Mitigation |
|------|------------|
| Misconfigured phases break CI | Fast-fail validator with clear diff + sample fix hints |
| Silent fallback to legacy logic | Explicit warning log + test asserting config path used |
| Inconsistent dependency annotations | Migration audit + analyzer warnings |
| Config drift across environments | Single source in base + targeted overrides only |

## Observability
- Log event `TestFrameworkConfigValidated` with phase count, targets, hash of config
- Log warning `LegacyPhaseDefinitionInUse` when feature flag disables externalization

## Next Enhancements (Post-Issue)
- Dynamic phase interpolation for partial phase transitions
- AI-assisted recommended next focus areas based on uncovered files
- Automatic velocity recalibration from historical trend persistence

---
Generated as part of Issue #102 implementation planning.
