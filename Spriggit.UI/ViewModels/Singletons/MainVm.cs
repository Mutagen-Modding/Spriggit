using Noggog.WPF;

namespace Spriggit.UI.ViewModels.Singletons;

public class MainVm : ViewModel
{
    public ActivePanelVm ActivePanelVm { get; }

    public MainVm(
        ReposListingVm reposListingVm,
        ActivePanelVm activePanelVm)
    {
        ActivePanelVm = activePanelVm;
        ActivePanelVm.SetMainVm(this);
        activePanelVm.Focus(reposListingVm);
    }
    
    public void Load()
    {
        
    }

    public void Init()
    {
        
    }
}