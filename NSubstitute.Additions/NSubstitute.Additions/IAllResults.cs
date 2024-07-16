using NSubstitute.Core;

namespace NSubstitute.Additions;

public interface IAllResults
{
    IEnumerable<ICall> AllCalls();
    IEnumerable<CallSpecAndTarget> QuerySpecification();
}
