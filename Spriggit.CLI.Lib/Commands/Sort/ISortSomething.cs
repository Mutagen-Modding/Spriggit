using Mutagen.Bethesda.Plugins.Records;

namespace Spriggit.CLI.Lib.Commands.Sort;

public interface ISortSomething<TMod, TModGetter>
    where TMod : IMod, TModGetter
    where TModGetter : IModGetter
{
    bool HasWorkToDo(TModGetter mod);
    void DoWork(TMod mod);
}