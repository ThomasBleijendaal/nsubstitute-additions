using NSubstitute;
using NSubstitute.Exceptions;
using static NSubstitute.Additions.Arg;

namespace TestProject;

public class PredicateArgumentTests
{
    private ISubject _subject;

    [SetUp]
    public void Setup()
    {
        _subject = Substitute.For<ISubject>();
    }

    [Test]
    public void Pass_when_predicate_matches()
    {
        _subject.ProcessItems(["1", "2"]);

        _subject.Received().ProcessItems(Is<string[]?>(array => array?.Length > 0));
    }

    [Test]
    public void Fail_when_predicate_does_not_match()
    {
        _subject.ProcessItems(null);

        Assert.Throws<ReceivedCallsException>(() =>
            _subject.Received().ProcessItems(Is<string[]?>(array => array?.Length > 0))
            );
    }
}

public interface ISubject
{
    void ProcessItems(string[]? items);
}
