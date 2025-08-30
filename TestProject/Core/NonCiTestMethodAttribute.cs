namespace TestProject.Core;

/// <summary>
///     Marks a test method to only run when not executing on a CI system.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class NonCiTestMethodAttribute : TestMethodAttribute
{
    public override TestResult[] Execute(ITestMethod testMethod)
    {
        if (IsCiSystem())
        {
            return
            [
                new()
                {
                    Outcome = UnitTestOutcome.Inconclusive,
                    TestContextMessages = "This test is not supported on CI systems"
                }
            ];
        }

        return base.Execute(testMethod);
    }

    /// <summary>
    ///     True if the executing machine is a Continuous-Integration (CI) system.
    /// </summary>
    private static bool IsCiSystem() => Environment.GetEnvironmentVariable("GITHUB_JOB") is not null;
}