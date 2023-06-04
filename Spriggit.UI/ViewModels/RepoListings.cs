using System.Collections.ObjectModel;
using Noggog;
using Spriggit.UI.Settings;

namespace Spriggit.UI.ViewModels;

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
        Links.SetTo(settings.Links.Select(x => _linkFactory(new LinkInputVm(x.GitPath, x.ModPath))));
    }

    public void SaveInto(MainSettings settings)
    {
        settings.Links = Links.Select(x => new LinkSettings()
        {
            GitPath = x.Input.GitFolderPicker.TargetPath,
            ModPath = x.Input.ModPathPicker.TargetPath,
        }).ToArray();
    }
}