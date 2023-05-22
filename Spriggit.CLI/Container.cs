using System.IO.Abstractions;
using Mutagen.Bethesda.Serialization.Streams;
using Noggog.WorkEngine;
using Spriggit.Engine;
using StrongInject;

namespace Spriggit.CLI;

[Register(typeof(EntryPointCache))]
[Register(typeof(SpriggitEngine))]
[Register(typeof(GetMetaToUse))]
[Register(typeof(ConstructEntryPoint))]
[Register(typeof(GetDefaultEntryPoint))]
[Register(typeof(NugetDownloader))]
[Register(typeof(PluginPublisher))]
[Register(typeof(TargetFrameworkDirLocator))]
partial class Container : IContainer<SpriggitEngine>
{
    [Instance] private readonly IFileSystem _fileSystem;
    [Instance] private readonly IWorkDropoff? _workDropoff;
    [Instance] private readonly ICreateStream? _streamCreate;

    public Container(
        IFileSystem fileSystem,
        IWorkDropoff? workDropoff, 
        ICreateStream? streamCreate)
    {
        _fileSystem = fileSystem;
        _workDropoff = workDropoff;
        _streamCreate = streamCreate;
    }
}