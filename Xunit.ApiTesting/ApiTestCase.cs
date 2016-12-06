using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.ApiTesting
{
    public class RatioException : Exception
    {
        public RatioException(string errorString):base(errorString)
        {
            
        }
    }

    [Serializable]
    public class ApiTestCase : XunitTestCase
    {
        private int _noOfRuns;
        private int _requiredSuccessfulRuns;
        private int _runTimeout;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer", true)]
        public ApiTestCase() { }

        public ApiTestCase(IMessageSink diagnosticMessageSink, TestMethodDisplay testMethodDisplay, ITestMethod testMethod, int noOfRuns, int requiredSuccessfulRuns)
            : base(diagnosticMessageSink, testMethodDisplay, testMethod, testMethodArguments: null)
        {
            _noOfRuns = noOfRuns;
            _requiredSuccessfulRuns = requiredSuccessfulRuns;
        }

        // This method is called by the xUnit test framework classes to run the test case. We will do the
        // loop here, forwarding on to the implementation in XunitTestCase to do the heavy lifting. We will
        // continue to re-run the test until the aggregator has an error (meaning that some internal error
        // condition happened), or the test runs without failure, or we've hit the maximum number of tries.
        public override async Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink,
                                                        IMessageBus messageBus,
                                                        object[] constructorArguments,
                                                        ExceptionAggregator aggregator,
                                                        CancellationTokenSource cancellationTokenSource)
        {
            var runs = 0;
            var successfulRuns = 0;
            RunSummary firstSuccessfulSummary = null;
            DelayedMessageBus firstSuccessfulDelayedMessageBus = null;
            DelayedMessageBus errorDelayedMessageBus = new DelayedMessageBus(messageBus);
            decimal totalExecutiontime = 0;

            while (true)
            {
                // This is really the only tricky bit: we need to capture and delay messages (since those will
                // contain run status) until we know we've decided to accept the final result;
                var delayedMessageBus = new DelayedMessageBus(messageBus);

                var summary = await base.RunAsync(diagnosticMessageSink, delayedMessageBus, constructorArguments, aggregator, cancellationTokenSource);

                if (summary.Failed == 0)
                {
                    successfulRuns++;
                    if (successfulRuns == 1)
                    {
                        firstSuccessfulSummary = summary;
                        firstSuccessfulDelayedMessageBus = delayedMessageBus;
                    }
                }
                runs++;
                totalExecutiontime += summary.Time;

                var e = aggregator.ToException();
                var anyErrorsOtherThanTimeouts = AnyErrorsOtherThanTimeouts(delayedMessageBus);
                if (aggregator.HasExceptions ||  runs >= _noOfRuns || anyErrorsOtherThanTimeouts)
                {
                    if (firstSuccessfulSummary != null && successfulRuns >= _requiredSuccessfulRuns)
                    {
                        firstSuccessfulDelayedMessageBus.Dispose(); // Sends all the delayed messages
                        return firstSuccessfulSummary;
                    }
                    else
                    {
                        var errorString = $"Execution of '{DisplayName}' failed. Required successful runs:{_requiredSuccessfulRuns} of total {_noOfRuns} but was {successfulRuns} of {runs} tries.";
                        diagnosticMessageSink.OnMessage(new DiagnosticMessage(errorString));
                        var testFailed = (TestFailed)delayedMessageBus.messages.First(x => x is TestFailed);

                        errorDelayedMessageBus.QueueMessage(new TestFailed(new XunitTest(this, DisplayName), totalExecutiontime, errorString, new RatioException(errorString + Environment.NewLine + string.Join(Environment.NewLine,  testFailed.Messages))));
                        
                        errorDelayedMessageBus.Dispose();
                        delayedMessageBus.Dispose(); // Sends all the delayed messages
                        return summary;
                    }
                }
            }
        }

        private static bool AnyErrorsOtherThanTimeouts(DelayedMessageBus delayedMessageBus)
        {
            return (from testFailed in delayedMessageBus.messages.OfType<TestFailed>()
                    from exceptionType in testFailed.ExceptionTypes
                    select exceptionType).Any(x => x != "System.TimeoutException");
        }

        public override void Serialize(IXunitSerializationInfo data)
        {
            base.Serialize(data);

            data.AddValue("NoOfRuns", _noOfRuns);
            data.AddValue("RequiredSuccessfulRuns", _requiredSuccessfulRuns);
        }

        public override void Deserialize(IXunitSerializationInfo data)
        {
            base.Deserialize(data);

            _noOfRuns = data.GetValue<int>("NoOfRuns");
            _requiredSuccessfulRuns = data.GetValue<int>("RequiredSuccessfulRuns");
        }
}
}
