using System.Collections.ObjectModel;
using Noggog;
using Spriggit.UI.Settings;

namespace Spriggit.UI.ViewModels;

public class RepoListings : ISaveMainSettings
{
    public ObservableCollection<LinkVm> Links { get; } = new();
    
    public void ReadFrom(MainSettings settings)
    {
        Links.SetTo(settings.Links.Select(x => new LinkVm(x)));
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