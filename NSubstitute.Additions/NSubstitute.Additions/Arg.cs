using NSubstitute.Core.Arguments;

namespace NSubstitute.Additions;

public static class Arg
{
    public static ref T Is<T>(Predicate<T?> predicate, [System.Runtime.CompilerServices.CallerArgumentExpression("predicate")] string predicateExpression = "")
    {
        return ref ArgumentMatcher.Enqueue<T>(new PredicateArgumentMatcher<T>(predicate, predicateExpression))!;
    }

    private sealed class PredicateArgumentMatcher<T>(Predicate<T?> predicate, string predicateExpression) : IArgumentMatcher<T>
    {
        private readonly string _predicateDescription = predicateExpression;
        private readonly Predicate<T?> _predicate = predicate;

        public bool IsSatisfiedBy(T? argument) => _predicate((T?)argument!);

        public override string ToString() => _predicateDescription;
    }
}
