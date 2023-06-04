using System.IO;
using System.Reactive.Linq;
using System.Windows.Input;
using Noggog;
using Noggog.WPF;
using ReactiveUI;
using Spriggit.UI.Settings;

namespace Spriggit.UI.ViewModels;

public class AddNewLinkVm : ViewModel, IActivatableVm, ISaveMainSettings
{
    public LinkInputVm LinkInput { get; private set; } = new();
    
    public ICommand AddCommand { get; }

    private ViewModel? _previous;

    public AddNewLinkVm(
        LinkVm.InputFactory linkFactory,
        RepoListings reposListingVm,
        ActivePanelVm activePanelVm)
    {
        AddCommand = ReactiveCommand.Create(
            execute: () =>
            {
                reposListingVm.Links.Add(linkFactory(LinkInput));
                activePanelVm.Focus(_previous);

                var replacement = new LinkInputVm();
                replacement.GitFolderPicker.TargetPath = LinkInput.GitFolderPicker.TargetPath;
                replacement.ModPathPicker.TargetPath = GetModPathSaveString(LinkInput);
                LinkInput = replacement;
            },
            canExecute: Observable.CombineLatest(
                this.WhenAnyValue(x => x.LinkInput.ModPathPicker.InError),
                this.WhenAnyValue(x => x.LinkInput.GitFolderPicker.InError),
                (m, g) => !m && !g));
    }

    public void Activate(ViewModel? previous)
    {
        _previous = previous;
    }

    public void ReadFrom(MainSettings settings)
    {
        LinkInput.GitFolderPicker.TargetPath = settings.AddNewLinkSettings.GitPath;
        LinkInput.ModPathPicker.TargetPath = settings.AddNewLinkSettings.ModPath;
    }

    public void SaveInto(MainSettings settings)
    {
        settings.AddNewLinkSettings.GitPath = LinkInput.GitFolderPicker.TargetPath;
        settings.AddNewLinkSettings.ModPath = GetModPathSaveString(LinkInput);
    }

    private string GetModPathSaveString(LinkInputVm inputVm)
    {
        if (inputVm.ModPathPicker.TargetPath.IsNullOrWhitespace())
        {
            return string.Empty;
        }

        return Path.GetDirectoryName(inputVm.ModPathPicker.TargetPath) ?? string.Empty;
    }
}