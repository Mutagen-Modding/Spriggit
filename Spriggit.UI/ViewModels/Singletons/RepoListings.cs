using System.Collections.ObjectModel;
using Noggog;
using Spriggit.UI.Settings;
using Spriggit.UI.ViewModels.Transient;

namespace Spriggit.UI.ViewModels.Singletons;

public class RepoListings : ISaveMainSettings
{
    private readonly LinkVm.InputFactory _linkFactory;
    private readonly LinkInputVm.Factory _inputFactory;
    public ObservableCollection<LinkVm> Links { get; } = new();

    public RepoListings(
        LinkVm.InputFactory linkFactory,
        LinkInputVm.Factory inputFactory)
    {
        _linkFactory = linkFactory;
        _inputFactory = inputFactory;
    }
    
    public void ReadFrom(MainSettings settings)
    {
        Links.SetTo(settings.Links.Select(x =>
        {
            var input = _inputFactory();
            input.Absorb(x);
            return _linkFactory(input);
        }));
    }

    public void SaveInto(MainSettings settings)
    {
        settings.Links = Links
            .Select(x => x.Input.ToSettings())
            .ToArray();
    }
}