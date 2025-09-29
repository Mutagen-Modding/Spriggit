using Shouldly;
using Spriggit.CLI.Lib.Commands.Sort;
using Xunit;

namespace Spriggit.Tests.Sort.CliSorting;

public class SortCommandTests
{
    [Fact]
    public void InstantiationTest()
    {
        var service = SortCommand.GetService();
        service.ShouldNotBeNull();
    }
}