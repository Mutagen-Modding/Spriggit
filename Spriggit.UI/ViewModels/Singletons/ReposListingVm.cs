using System.Collections.ObjectModel;
using System.Reactive;
using System.Windows.Input;
using Noggog.WPF;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Spriggit.UI.ViewModels.Transient;

namespace Spriggit.UI.ViewModels.Singletons;

public class ReposListingVm : ViewModel
{
    public ObservableCollection<LinkVm> Links { get; }
    
    public ICommand AddNewLinkCommand { get; }
    
    public ICommand SyncSelectedToModCommand { get; }
    
    public ICommand SyncSelectedToGitCommand { get; }

    [Reactive] public IReadOnlyList<LinkVm>? SelectedLinks { get; set; }

    private IReadOnlyList<LinkVm> GetLinksToConvert => Links;
        // SelectedLinks == null || SelectedLinks.Count == 0 ? Links : SelectedLinks;

    public ReposListingVm(
        RepoListings listings,
        ActivePanelVm activePanelVm,
        AddNewLinkVm addNewLinkVm)
    {
        Links = listings.Links;
        AddNewLinkCommand = ReactiveCommand.Create(() => activePanelVm.Focus(addNewLinkVm));
        SyncSelectedToGitCommand = ReactiveCommand.Create(() =>
        {
            foreach (var link in GetLinksToConvert)
            {
                ICommand cmd = link.SyncToGitCommand;
                if (cmd.CanExecute(Unit.Default))
                {
                    cmd.Execute(Unit.Default);
                }
            }
        });
        SyncSelectedToModCommand = ReactiveCommand.Create(() =>
        {
            foreach (var link in GetLinksToConvert)
            {
                ICommand cmd = link.SyncToModCommand;
                if (cmd.CanExecute(Unit.Default))
                {
                    cmd.Execute(Unit.Default);
                }
            }
        });
    }
}