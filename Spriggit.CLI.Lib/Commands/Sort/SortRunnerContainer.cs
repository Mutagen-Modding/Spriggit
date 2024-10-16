using System.IO.Abstractions;
using Serilog;
using StrongInject;

namespace Spriggit.CLI.Lib.Commands.Sort;

[Register(typeof(SortSkyrim))]
[Register(typeof(SortFallout4))]
[Register(typeof(SortStarfield))]
[Register(typeof(SortCommandRunner))]
partial class SortRunnerContainer : IContainer<SortCommandRunner>
{
    [Instance] private readonly IFileSystem _fileSystem;
    [Instance] private readonly ILogger _logger;

    public SortRunnerContainer(
        IFileSystem fileSystem,
        ILogger logger)
    {
        _fileSystem = fileSystem;
        _logger = logger;
    }
}