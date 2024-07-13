using NuGet.Configuration;
using NuGet.Protocol.Core.Types;

namespace Spriggit.Engine.Services.Singletons;

public class NugetSourceProvider
{
    public ISettings Settings { get; }
    public SourceRepositoryProvider SourceRepositoryProvider { get; }
    private readonly Lazy<SourceRepository[]> _sourceRespositories;
    public SourceRepository[] SourceRepositories => _sourceRespositories.Value;
    
    public NugetSourceProvider()
    {
        Settings = NuGet.Configuration.Settings.LoadDefaultSettings(root: null);
        SourceRepositoryProvider = new SourceRepositoryProvider(
            Settings, 
            Repository.Provider.GetCoreV3());
        _sourceRespositories = new Lazy<SourceRepository[]>(() =>
        {
            return SourceRepositoryProvider.GetRepositories().ToArray();
        });
    }
}