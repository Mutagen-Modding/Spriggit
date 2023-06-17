using System.Collections.ObjectModel;
using Noggog;
using Spriggit.UI.Settings;
using Spriggit.UI.ViewModels.Transient;

namespace Spriggit.UI.ViewModels.Singletons;

public class RepoListings : ISaveMainSettings
{
    private readonly LinkVm.InputFactory _linkFactory;
    public ObservableCollection<LinkVm> Links { get; } = new();

    public RepoListings(LinkVm.InputFactory linkFactory)
    {
        _linkFactory = linkFactory;
    }
    
    public void ReadFrom(MainSettings settings)
    {
        Links.SetTo(settings.Links.Select(x => _linkFactory(new LinkInputVm(x))));
    }

    public void SaveInto(MainSettings settings)
    {
        settings.Links = Links
            .Select(x => x.Input.ToSettings())
            .ToArray();
    }
}