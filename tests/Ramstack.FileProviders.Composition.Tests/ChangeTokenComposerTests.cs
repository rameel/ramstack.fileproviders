namespace Ramstack.FileProviders.Composition;

[TestFixture]
public sealed class ChangeTokenComposerTests
{
    [Test]
    public void Flatten_ReturnsAsIs_WhenNoComposite()
    {
        var changeToken = new TestChangeToken();
        var result = ChangeTokenComposer.FlattenChangeToken(changeToken);

        Assert.That(result, Is.SameAs(changeToken));
    }

    [Test]
    public void Flatten_ReturnsCompositeProvider_WhenNeedComposite()
    {
        var changeToken = CreateCompositeChangeToken(new TestChangeToken(), new TestChangeToken());

        var result = ChangeTokenComposer.FlattenChangeToken(changeToken);
        Assert.That(result, Is.InstanceOf<CompositeChangeToken>());
    }

    [Test]
    public void Flatten_ReturnsAsIs_WhenAlreadyFlat()
    {
        var changeToken = CreateCompositeChangeToken(new TestChangeToken(), new TestChangeToken());

        var result = ChangeTokenComposer.FlattenChangeToken(changeToken);
        Assert.That(result, Is.SameAs(changeToken));
    }

    [Test]
    public void Flatten_ReturnsCompositeChangeToken_Flattened()
    {
        var changeToken = CreateCompositeChangeToken(
            new TestChangeToken(),
            CreateCompositeChangeToken(
                new TestChangeToken(),
                new TestChangeToken(),
                CreateCompositeChangeToken(
                    new TestChangeToken())));

        var result = ChangeTokenComposer.FlattenChangeToken(changeToken);

        Assert.That(result, Is.InstanceOf<CompositeChangeToken>());
        Assert.That(((CompositeChangeToken)result).ChangeTokens.Count, Is.EqualTo(4));
        Assert.That(((CompositeChangeToken)result).ChangeTokens, Is.All.InstanceOf<TestChangeToken>());
    }

    [Test]
    public void Flatten_RemovesNullChangeToken()
    {
        var changeToken = CreateCompositeChangeToken(
            new TestChangeToken(),
            CreateCompositeChangeToken(
                new TestChangeToken(),
                NullChangeToken.Singleton,
                new TestChangeToken()),
            NullChangeToken.Singleton);

        var result = ChangeTokenComposer.FlattenChangeToken(changeToken);

        Assert.That(result, Is.InstanceOf<CompositeChangeToken>());
        Assert.That(((CompositeChangeToken)result).ChangeTokens.Count, Is.EqualTo(3));
        Assert.That(((CompositeChangeToken)result).ChangeTokens, Is.All.InstanceOf<TestChangeToken>());
    }

    [Test]
    public void Flatten_ReturnsNullChangeToken_WhenNothingReturn()
    {
        var changeToken = CreateCompositeChangeToken(
            CreateCompositeChangeToken(
                NullChangeToken.Singleton,
                NullChangeToken.Singleton,
                CreateCompositeChangeToken(
                    CreateCompositeChangeToken(
                        NullChangeToken.Singleton,
                        NullChangeToken.Singleton),
                    NullChangeToken.Singleton),
                CreateCompositeChangeToken(
                    NullChangeToken.Singleton,
                    NullChangeToken.Singleton,
                    CreateCompositeChangeToken(
                        CreateCompositeChangeToken(
                            NullChangeToken.Singleton,
                            NullChangeToken.Singleton),
                        NullChangeToken.Singleton))),
            NullChangeToken.Singleton,
            NullChangeToken.Singleton,
            CreateCompositeChangeToken(
                NullChangeToken.Singleton,
                NullChangeToken.Singleton,
                CreateCompositeChangeToken(
                    CreateCompositeChangeToken(
                        NullChangeToken.Singleton,
                        NullChangeToken.Singleton),
                    NullChangeToken.Singleton)),
            NullChangeToken.Singleton,
            CreateCompositeChangeToken(
                CreateCompositeChangeToken(
                    NullChangeToken.Singleton,
                    NullChangeToken.Singleton),
                NullChangeToken.Singleton));

        var result = ChangeTokenComposer.FlattenChangeToken(changeToken);
        Assert.That(result, Is.InstanceOf<NullChangeToken>());
        Assert.That(result, Is.SameAs(NullChangeToken.Singleton));
    }

    [Test]
    public void Flatten_ReturnsSingleToken_WhenRemainOneToken()
    {
        var changeToken = CreateCompositeChangeToken(
            CreateCompositeChangeToken(
                NullChangeToken.Singleton,
                NullChangeToken.Singleton,
                CreateCompositeChangeToken(
                    CreateCompositeChangeToken(
                        NullChangeToken.Singleton,
                        NullChangeToken.Singleton),
                    NullChangeToken.Singleton),
                CreateCompositeChangeToken(
                    NullChangeToken.Singleton,
                    NullChangeToken.Singleton,
                    CreateCompositeChangeToken(
                        CreateCompositeChangeToken(
                            NullChangeToken.Singleton,
                            new TestChangeToken()),
                        NullChangeToken.Singleton))),
            NullChangeToken.Singleton,
            NullChangeToken.Singleton,
            CreateCompositeChangeToken(
                NullChangeToken.Singleton,
                NullChangeToken.Singleton,
                CreateCompositeChangeToken(
                    CreateCompositeChangeToken(
                        NullChangeToken.Singleton,
                        NullChangeToken.Singleton),
                    NullChangeToken.Singleton)),
            NullChangeToken.Singleton,
            CreateCompositeChangeToken(
                CreateCompositeChangeToken(
                    NullChangeToken.Singleton,
                    NullChangeToken.Singleton),
                NullChangeToken.Singleton));

        var result = ChangeTokenComposer.FlattenChangeToken(changeToken);
        Assert.That(result, Is.InstanceOf<TestChangeToken>());
    }

    [Test]
    public void Flatten_MaintainOrder_WhenComposite()
    {
        var t1 = new TestChangeToken();
        var t2 = new TestChangeToken();
        var t3 = new TestChangeToken();
        var t4 = new TestChangeToken();
        var t5 = new TestChangeToken();
        var t6 = new TestChangeToken();
        var t7 = new TestChangeToken();
        var t8 = new TestChangeToken();
        var t9 = new TestChangeToken();

        var changeToken = ChangeTokenComposer.ComposeChangeTokens(
            CreateCompositeChangeToken(
                NullChangeToken.Singleton,
                NullChangeToken.Singleton,
                CreateCompositeChangeToken(
                    t1,
                    CreateCompositeChangeToken(
                        t2,
                        NullChangeToken.Singleton,
                        t3),
                    t4,
                    NullChangeToken.Singleton),
                t5,
                CreateCompositeChangeToken(
                    NullChangeToken.Singleton,
                    NullChangeToken.Singleton,
                    CreateCompositeChangeToken(
                        CreateCompositeChangeToken(
                            NullChangeToken.Singleton,
                            t6),
                        NullChangeToken.Singleton)),
                t7),
            NullChangeToken.Singleton,
            NullChangeToken.Singleton,
            CreateCompositeChangeToken(
                NullChangeToken.Singleton,
                NullChangeToken.Singleton,
                CreateCompositeChangeToken(
                    CreateCompositeChangeToken(
                        NullChangeToken.Singleton,
                        NullChangeToken.Singleton),
                    NullChangeToken.Singleton,
                    t8)),
            NullChangeToken.Singleton,
            CreateCompositeChangeToken(
                CreateCompositeChangeToken(
                    NullChangeToken.Singleton,
                    NullChangeToken.Singleton),
                NullChangeToken.Singleton,
                t9));

        var composite = (CompositeChangeToken)changeToken;
        var changeTokens = new IChangeToken[] {t1, t2, t3, t4, t5, t6, t7, t8, t9};

        Assert.That(changeToken, Is.InstanceOf<CompositeChangeToken>());
        Assert.That(composite.ChangeTokens, Is.EquivalentTo(changeTokens));
    }

    [Test]
    public void Flatten_ReturnsSingleToken_WhenCompositeContainsOnlyOne()
    {
        var changeToken =
            CreateCompositeChangeToken(
                CreateCompositeChangeToken(
                    CreateCompositeChangeToken(
                        CreateCompositeChangeToken(
                            new TestChangeToken()))));

        Assert.That(changeToken.Flatten(), Is.InstanceOf<TestChangeToken>());
    }

    [Test]
    public void Flatten_EmptyComposite_ReturnsNullChangeToken()
    {
        var changeToken = CreateCompositeChangeToken().Flatten();
        Assert.That(changeToken, Is.InstanceOf<NullChangeToken>());
        Assert.That(changeToken, Is.SameAs(NullChangeToken.Singleton));
    }

    private static CompositeChangeToken CreateCompositeChangeToken(params IChangeToken[] changeTokens) =>
        new(changeTokens);

    private sealed class TestChangeToken : IChangeToken
    {
        public bool HasChanged => false;

        public bool ActiveChangeCallbacks => false;

        public IDisposable RegisterChangeCallback(Action<object> callback, object state) =>
            NullChangeToken.Singleton.RegisterChangeCallback(callback, state);
    }
}
