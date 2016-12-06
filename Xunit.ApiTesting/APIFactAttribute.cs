using Xunit.Sdk;

namespace Xunit.ApiTesting
{
    [XunitTestCaseDiscoverer("Xunit.ApiTesting.ApiFactDiscoverer", "Xunit.ApiTesting")]
    public class ApiFactAttribute : FactAttribute
    {
        /// <summary>
        /// Number of retries allowed for a failed test. If unset (or set less than 1), will
        /// default to 3 attempts.
        /// </summary>
        public int NoOfRuns { get; set; }
        /// <summary>
        /// Number of retries allowed for a failed test. If unset (or set less than 1), will
        /// default to 3 attempts.
        /// </summary>
        public int RequiredSuccessfulRuns{ get; set; }
    }
}