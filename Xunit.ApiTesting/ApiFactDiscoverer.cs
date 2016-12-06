using System.Collections.Generic;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.ApiTesting
{
    public class ApiFactDiscoverer : IXunitTestCaseDiscoverer
    {
        readonly IMessageSink diagnosticMessageSink;

        public ApiFactDiscoverer(IMessageSink diagnosticMessageSink)
        {
            this.diagnosticMessageSink = diagnosticMessageSink;
        }

        public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
        {
            var noOfRuns = factAttribute.GetNamedArgument<int>("NoOfRuns");
            if (noOfRuns < 1)
                noOfRuns = 20;

            var requiredSuccessfulRuns = factAttribute.GetNamedArgument<int>("RequiredSuccessfulRuns");
            if (requiredSuccessfulRuns < 1)
                requiredSuccessfulRuns = 15;

            yield return new ApiTestCase(diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), testMethod, noOfRuns, requiredSuccessfulRuns);
        }
    }
}