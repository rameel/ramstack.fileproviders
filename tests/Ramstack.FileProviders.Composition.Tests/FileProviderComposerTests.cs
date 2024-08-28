using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Ramstack.FileProviders.Composition;

[TestFixture]
public sealed class FileProviderComposerTests
{
    [Test]
    public void FlattenFileProvider_ReturnsAsIs_WhenNoComposite()
    {
        var provider = new TestFileProvider();
        var result = FileProviderComposer.FlattenProvider(provider);
        Assert.That(result, Is.SameAs(provider));
    }

    [Test]
    public void FlattenFileProvider_ReturnsCompositeProvider_WhenNeedComposite()
    {
        var provider = new CompositeFileProvider(new TestFileProvider(), new TestFileProvider());

        var result = FileProviderComposer.FlattenProvider(provider);
        Assert.That(result, Is.InstanceOf<CompositeFileProvider>());
    }

    [Test]
    public void FlattenFileProvider_ReturnsAsIs_WhenAlreadyFlat()
    {
        var provider = new CompositeFileProvider(new TestFileProvider(), new TestFileProvider());

        var result = FileProviderComposer.FlattenProvider(provider);
        Assert.That(result, Is.SameAs(provider));
    }

    [Test]
    public void FlattenFileProvider_ReturnsCompositeProvider_Flattened()
    {
        var provider = new CompositeFileProvider(
            new TestFileProvider(),
            new CompositeFileProvider(
                new TestFileProvider(),
                new TestFileProvider(),
                new CompositeFileProvider(
                    new TestFileProvider())));

        var result = FileProviderComposer.FlattenProvider(provider);

        Assert.That(result, Is.InstanceOf<CompositeFileProvider>());
        Assert.That(((CompositeFileProvider)result).FileProviders.Count(), Is.EqualTo(4));
        Assert.That(((CompositeFileProvider)result).FileProviders, Is.All.InstanceOf<TestFileProvider>());
    }

    [Test]
    public void FlattenFileProvider_RemovesNullFileProvider()
    {
        var provider = new CompositeFileProvider(
            new TestFileProvider(),
            new CompositeFileProvider(
                new TestFileProvider(),
                new NullFileProvider(),
                new TestFileProvider()),
            new NullFileProvider());

        var result = FileProviderComposer.FlattenProvider(provider);

        Assert.That(result, Is.InstanceOf<CompositeFileProvider>());
        Assert.That(((CompositeFileProvider)result).FileProviders.Count(), Is.EqualTo(3));
        Assert.That(((CompositeFileProvider)result).FileProviders, Is.All.InstanceOf<TestFileProvider>());
    }

    [Test]
    public void FlattenFileProvider_ReturnsNullFileProvider_WhenNothingReturn()
    {
        var provider = new CompositeFileProvider(
            new CompositeFileProvider(
                new NullFileProvider(),
                new NullFileProvider(),
                new CompositeFileProvider(
                    new CompositeFileProvider(
                        new NullFileProvider(),
                        new NullFileProvider()),
                    new NullFileProvider()),
                new CompositeFileProvider(
                    new NullFileProvider(),
                    new NullFileProvider(),
                    new CompositeFileProvider(
                        new CompositeFileProvider(
                            new NullFileProvider(),
                            new NullFileProvider()),
                        new NullFileProvider()))),
            new NullFileProvider(),
            new NullFileProvider(),
            new CompositeFileProvider(
                new NullFileProvider(),
                new NullFileProvider(),
                new CompositeFileProvider(
                    new CompositeFileProvider(
                        new NullFileProvider(),
                        new NullFileProvider()),
                    new NullFileProvider())),
            new NullFileProvider(),
            new CompositeFileProvider(
                new CompositeFileProvider(
                    new NullFileProvider(),
                    new NullFileProvider()),
                new NullFileProvider()));

        var result = FileProviderComposer.FlattenProvider(provider);
        Assert.That(result, Is.InstanceOf<NullFileProvider>());
    }

    [Test]
    public void FlattenFileProvider_ReturnsSingleProvider_WhenRemainOneProvider()
    {
        var provider = new CompositeFileProvider(
            new CompositeFileProvider(
                new NullFileProvider(),
                new NullFileProvider(),
                new CompositeFileProvider(
                    new CompositeFileProvider(
                        new NullFileProvider(),
                        new NullFileProvider()),
                    new NullFileProvider()),
                new CompositeFileProvider(
                    new NullFileProvider(),
                    new NullFileProvider(),
                    new CompositeFileProvider(
                        new CompositeFileProvider(
                            new NullFileProvider(),
                            new TestFileProvider()),
                        new NullFileProvider()))),
            new NullFileProvider(),
            new NullFileProvider(),
            new CompositeFileProvider(
                new NullFileProvider(),
                new NullFileProvider(),
                new CompositeFileProvider(
                    new CompositeFileProvider(
                        new NullFileProvider(),
                        new NullFileProvider()),
                    new NullFileProvider())),
            new NullFileProvider(),
            new CompositeFileProvider(
                new CompositeFileProvider(
                    new NullFileProvider(),
                    new NullFileProvider()),
                new NullFileProvider()));

        var result = FileProviderComposer.FlattenProvider(provider);
        Assert.That(result, Is.InstanceOf<TestFileProvider>());
    }

    [Test]
    public void FlattenFileProvider_MaintainOrder_WhenComposite()
    {
        var p1 = new TestFileProvider();
        var p2 = new TestFileProvider();
        var p3 = new TestFileProvider();
        var p4 = new TestFileProvider();
        var p5 = new TestFileProvider();
        var p6 = new TestFileProvider();
        var p7 = new TestFileProvider();
        var p8 = new TestFileProvider();
        var p9 = new TestFileProvider();

        var provider = FileProviderComposer.ComposeProviders(
            new CompositeFileProvider(
                new NullFileProvider(),
                new NullFileProvider(),
                new CompositeFileProvider(
                    p1,
                    new CompositeFileProvider(
                        p2,
                        new NullFileProvider(),
                        p3),
                    p4,
                    new NullFileProvider()),
                p5,
                new CompositeFileProvider(
                    new NullFileProvider(),
                    new NullFileProvider(),
                    new CompositeFileProvider(
                        new CompositeFileProvider(
                            new NullFileProvider(),
                            p6),
                        new NullFileProvider())),
                p7),
            new NullFileProvider(),
            new NullFileProvider(),
            new CompositeFileProvider(
                new NullFileProvider(),
                new NullFileProvider(),
                new CompositeFileProvider(
                    new CompositeFileProvider(
                        new NullFileProvider(),
                        new NullFileProvider()),
                    new NullFileProvider(),
                    p8)),
            new NullFileProvider(),
            new CompositeFileProvider(
                new CompositeFileProvider(
                    new NullFileProvider(),
                    new NullFileProvider()),
                new NullFileProvider(),
                p9));

        var composite = (CompositeFileProvider)provider;
        var providers = new IFileProvider[] {p1, p2, p3, p4, p5, p6, p7, p8, p9};

        Assert.That(provider, Is.InstanceOf<CompositeFileProvider>());
        Assert.That(composite.FileProviders, Is.EquivalentTo(providers));
    }

    private sealed class TestFileProvider : IFileProvider
    {
        public IFileInfo GetFileInfo(string subpath) =>
            new NotFoundFileInfo(subpath);

        public IDirectoryContents GetDirectoryContents(string subpath) =>
            NotFoundDirectoryContents.Singleton;

        public IChangeToken Watch(string? filter) =>
            NullChangeToken.Singleton;
    }
}
