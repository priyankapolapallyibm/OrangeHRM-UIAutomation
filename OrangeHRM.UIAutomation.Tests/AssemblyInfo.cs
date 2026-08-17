// Optimization 3: NUnit parallel execution
// ParallelScope.Fixtures runs each SpecFlow feature class (fixture) in parallel.
// LevelOfParallelism(3) keeps CPU/memory usage reasonable in CI (GitHub Actions has 2 vCPUs).
// Each scenario still runs sequentially within its feature file, which is safe because
// BrowserDriver is instantiated per-scenario via ScenarioContext injection (no shared state).
using NUnit.Framework;

[assembly: Parallelizable(ParallelScope.Fixtures)]
[assembly: LevelOfParallelism(3)]
