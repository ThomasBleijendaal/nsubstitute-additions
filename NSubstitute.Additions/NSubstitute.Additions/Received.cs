using System.Reflection;
using NSubstitute.Core;
using NSubstitute.Core.SequenceChecking;

namespace NSubstitute.Additions;

public static class Received
{
    public static ReceivedForSubstitutes For(params object[] substitutes)
    {
        return new ReceivedForSubstitutes(substitutes);
    }

    public static ReceivedForSubstitutes ForMentioned()
    {
        return new ReceivedForSubstitutes([]);
    }

    public sealed class ReceivedForSubstitutes(object[] substitutes)
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="calls"></param>
        public void NoOtherThan(Action calls)
        {
            var query = new GetAllCallsQuery(SubstitutionContext.Current.CallSpecificationFactory);

            foreach (var substitute in substitutes)
            {
                query.RegisterSubstitute(substitute);
            }

            SubstitutionContext.Current.ThreadContext.RunInQueryContext(calls, query);
            ReceivedNoOtherAssertion.Assert(query.Result());
        }
    }

    public sealed class GetAllCallsQuery(ICallSpecificationFactory callSpecificationFactory) : IQuery, IAllResults
    {
        private readonly List<CallSpecAndTarget> _querySpec = [];
        private readonly HashSet<ICall> _allCalls = [];

        public void RegisterSubstitute(object substitute)
        {
            _allCalls.UnionWith(substitute.ReceivedCalls());
        }

        public void RegisterCall(ICall call)
        {
            var target = call.Target();
            var callSpecification = callSpecificationFactory.CreateFrom(call, MatchArgs.AsSpecifiedInCall);

            _querySpec.Add(new CallSpecAndTarget(callSpecification, target));

            var allMatchingCallsOnTarget = target.ReceivedCalls();
            _allCalls.UnionWith(allMatchingCallsOnTarget);
        }

        public IAllResults Result() => this;

        IEnumerable<ICall> IAllResults.AllCalls() => _allCalls;

        IEnumerable<CallSpecAndTarget> IAllResults.QuerySpecification() => _querySpec.Select(x => x);
    }

    public static class ReceivedNoOtherAssertion
    {
        public static void Assert(IAllResults queryResult)
        {
            var matchingCallsInOrder = queryResult
                .AllCalls()
                .Where(x => IsNotPropertyGetterCall(x.GetMethodInfo()))
                .ToArray();
            var querySpec = queryResult
                .QuerySpecification()
                .Where(x => IsNotPropertyGetterCall(x.CallSpecification.GetMethodInfo()))
                .ToArray();

            if (matchingCallsInOrder.Length != querySpec.Length)
            {
                throw new OtherCallFoundException(GetExceptionMessage(querySpec, matchingCallsInOrder));
            }

            var callsAndSpecs = matchingCallsInOrder
                .Select(call => new
                {
                    Call = call,
                    Specs = querySpec.Where(x => Matches(call, x)).ToArray()
                })
                .ToArray();

            if (Array.Exists(callsAndSpecs, x => x.Specs.Length == 0))
            {
                throw new OtherCallFoundException(GetExceptionMessage(querySpec, matchingCallsInOrder));
            }
        }

        private static bool Matches(ICall call, CallSpecAndTarget specAndTarget)
            => ReferenceEquals(call.Target(), specAndTarget.Target)
                   && specAndTarget.CallSpecification.IsSatisfiedBy(call);

        private static bool IsNotPropertyGetterCall(MethodInfo methodInfo)
            => methodInfo.GetPropertyFromGetterCallOrNull() == null;

        private static string GetExceptionMessage(CallSpecAndTarget[] querySpec, ICall[] matchingCallsInOrder)
        {
            const string callDelimiter = "\n    ";
            var formatter = new SequenceFormatter(callDelimiter, querySpec, matchingCallsInOrder);
            return string.Format("\nExpected to receive only these calls:\n{0}{1}\n" +
                                 "\nActually received matching calls:\n{0}{2}\n\n{3}",
                                 callDelimiter,
                                 formatter.FormatQuery(),
                                 formatter.FormatActualCalls(),
                                 "*** Note: calls to property getters are not considered part of the query. ***");
        }
    }
}
