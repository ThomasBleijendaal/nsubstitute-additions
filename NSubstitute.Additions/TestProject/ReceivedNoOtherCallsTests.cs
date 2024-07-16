using NSubstitute;
using NSubstitute.Additions;
using Received = NSubstitute.Additions.Received;

namespace TestProject;

public class ReceivedNoOtherCallsTests
{
    private IFoo _foo;
    private IBar _bar;

    [Test]
    public void Pass_when_verifying_a_single_call()
    {
        _foo.Start();

        Received.For(_foo).NoOtherThan(() => _foo.Start());
    }

    [Test]
    public void Pass_when_verifying_a_single_call_while_ignoring_other_substitute()
    {
        _foo.Start();
        _bar.Begin();

        Received.For(_foo).NoOtherThan(() => _foo.Start());
    }

    [Test]
    public void Fail_when_verifying_a_call_that_was_not_in_expected()
    {
        _foo.Start();

        Assert.Throws<OtherCallFoundException>(() =>
            Received.For(_foo).NoOtherThan(() => _foo.Finish())
            );
    }

    [Test]
    public void Fail_when_verifying_a_call_that_was_not_in_expected_on_another_substitute()
    {
        _foo.Start();
        _bar.Begin();

        Assert.Throws<OtherCallFoundException>(() =>
           Received.For(_foo, _bar).NoOtherThan(() =>
           {
               _foo.Start();
           })
           );
    }

    [Test]
    public void Pass_when_calls_match_exactly()
    {
        _foo.Start(2);
        _bar.Begin();
        _foo.Finish();
        _bar.End();

        Received.For(_foo).NoOtherThan(() =>
        {
            _foo.Start(2);
            _bar.Begin();
            _foo.Finish();
            _bar.End();
        });
    }

    [Test]
    public void Fail_when_verifying_multiple_calls_that_was_not_in_called()
    {
        _foo.Start();

        Assert.Throws<OtherCallFoundException>(() =>
            Received.ForMentioned().NoOtherThan(() =>
            {
                _foo.Start();
                _foo.Finish();
            })
            );
    }

    [Test]
    public void Pass_when_verifying_multiple_identical_calls()
    {
        _foo.Start(1);
        _foo.Start(2);
        _foo.Start(3);

        Received.ForMentioned().NoOtherThan(() =>
        {
            _foo.Start(3);
            _foo.Start(1);
            _foo.Start(2);
        });
    }

    [Test]
    public void Fail_when_verifying_multiple_identical_call_that_were_not_in_called()
    {
        _foo.Start(1);
        _foo.Start(2);
        _foo.Start(3);

        Assert.Throws<OtherCallFoundException>(() =>
            Received.ForMentioned().NoOtherThan(() =>
            {
                _foo.Start(1);
                _foo.Start(1);
                _foo.Start(1);
            })
        );
    }

    [Test]
    public void Fail_when_verifying_multiple_identical_call_that_were_not_in_expected()
    {
        _foo.Start(1);
        _foo.Start(2);

        Assert.Throws<OtherCallFoundException>(() =>
            Received.ForMentioned().NoOtherThan(() =>
            {
                _foo.Start(1);
                _foo.Start(2);
                _foo.Start(3);
            })
            );
    }

    [SetUp]
    public void SetUp()
    {
        _foo = Substitute.For<IFoo>();
        _bar = Substitute.For<IBar>();
    }

    public interface IFoo
    {
        void Start();
        void Start(int i);
        void Finish();
        void FunkyStuff(string s);
        event Action OnFoo;
    }

    public interface IBar
    {
        void Begin();
        void End();
    }
}
