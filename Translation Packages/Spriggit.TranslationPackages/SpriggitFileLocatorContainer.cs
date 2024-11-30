using System.IO.Abstractions;
using Serilog;
using Spriggit.Core;
using Spriggit.Core.Services.Singletons;
using StrongInject;

namespace Spriggit.TranslationPackages;

[Register(typeof(SpriggitFileLocator))]
partial class SpriggitFileLocatorContainer : IContainer<SpriggitFileLocator>
{
    [Instance] private readonly IFileSystem _fileSystem;
    [Instance] private readonly ILogger _logger;

    public SpriggitFileLocatorContainer(
        IFileSystem fileSystem,
        ILogger logger)
    {
        _fileSystem = fileSystem;
        _logger = logger;
    }
}