namespace Ramstack.FileProviders.Utilities;

public sealed class DisposableFileProvider(IFileProvider provider) : IFileProvider, IDisposable
{
    public IFileInfo GetFileInfo(string subpath) =>
        provider.GetFileInfo(subpath);

    public IDirectoryContents GetDirectoryContents(string subpath) =>
        provider.GetDirectoryContents(subpath);

    public IChangeToken Watch(string filter) =>
        provider.Watch(filter);

    public void Dispose() =>
        (provider as IDisposable)?.Dispose();
}
