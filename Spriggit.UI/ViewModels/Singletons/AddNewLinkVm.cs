using System.IO;
using System.Reactive.Linq;
using System.Windows.Input;
using Noggog;
using Noggog.WPF;
using ReactiveUI;
using Spriggit.UI.Settings;
using Spriggit.UI.ViewModels.Transient;

namespace Spriggit.UI.ViewModels.Singletons;

public class AddNewLinkVm : ViewModel, IActivatableVm, ISaveMainSettings, IEditLinkVm
{
    public LinkInputVm LinkInput { get; private set; }
    
    public ICommand FinishCommand { get; }
    public ICommand DiscardCommand { get; }

    private ViewModel? _previous;

    public AddNewLinkVm(
        LinkInputVm.Factory inputFactory,
        LinkVm.InputFactory linkFactory,
        RepoListings reposListingVm,
        ActivePanelVm activePanelVm)
    {
        LinkInput = inputFactory();
        FinishCommand = ReactiveCommand.Create(
            execute: () =>
            {
                reposListingVm.Links.Add(linkFactory(LinkInput));
                activePanelVm.Focus(_previous);

                var replacement = inputFactory();
                replacement.CopyFrom(LinkInput);
                replacement.ModPathPicker.TargetPath = GetModPathSaveString(LinkInput);
                LinkInput = replacement;
            },
            canExecute: this.WhenAnyValue(x => x.LinkInput.InError)
                .Select(x => !x));

        DiscardCommand = ReactiveCommand.Create(() =>
        {
            activePanelVm.Focus(_previous);
        });
    }

    public void Activate(ViewModel? previous)
    {
        _previous = previous;
    }

    public void ReadFrom(MainSettings settings)
    {
        LinkInput.Absorb(settings.AddNewLinkSettings);
    }

    public void SaveInto(MainSettings settings)
    {
        settings.AddNewLinkSettings = LinkInput.ToSettings();
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