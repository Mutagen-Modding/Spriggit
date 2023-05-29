using System.Collections.ObjectModel;
using System.Windows.Input;
using Noggog.WPF;
using ReactiveUI;

namespace Spriggit.UI.ViewModels;

public class ReposListingVm : ViewModel
{
    public ObservableCollection<LinkVm> Links { get; }
    
    public ICommand AddNewLinkCommand { get; }

    public ReposListingVm(
        RepoListings listings,
        ActivePanelVm activePanelVm,
        AddNewLinkVm addNewLinkVm)
    {
        Links = listings.Links;
        AddNewLinkCommand = ReactiveCommand.Create(() => activePanelVm.Focus(addNewLinkVm));
    }
}