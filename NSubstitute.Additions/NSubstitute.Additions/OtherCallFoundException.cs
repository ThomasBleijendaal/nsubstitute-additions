using NSubstitute.Exceptions;

namespace NSubstitute.Additions;

public class OtherCallFoundException(string message) : SubstituteException(message)
{
}
