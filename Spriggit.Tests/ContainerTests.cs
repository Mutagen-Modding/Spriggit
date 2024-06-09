using Autofac;
using Noggog.Autofac;
using Spriggit.UI.Modules;
using Spriggit.UI.Services;
using Xunit;

namespace Spriggit.Tests;

public class ContainerTests
{
    private static ContainerBuilder GetBuilder()
    {
        var builder = new ContainerBuilder();
        builder.RegisterModule<MainModule>();
        ContainerTestUtil.RegisterCommonMocks(builder);
        return builder;
    }
        
    [Fact]
    public void GuiModule()
    {
        var builder = GetBuilder();
        var cont = builder.Build();
        cont.Validate(
            typeof(Startup));
    }
}