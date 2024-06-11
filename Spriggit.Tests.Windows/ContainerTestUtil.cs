using Autofac;
using Noggog;
using Spriggit.UI;

namespace Spriggit.Tests;

public static class ContainerTestUtil
{
    public static void RegisterCommonMocks(ContainerBuilder builder)
    {
        builder.RegisterMock<IMainWindow>();
    }
}