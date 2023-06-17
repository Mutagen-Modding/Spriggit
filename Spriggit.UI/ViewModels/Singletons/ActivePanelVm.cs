using Noggog.WPF;
using ReactiveUI.Fody.Helpers;

namespace Spriggit.UI.ViewModels.Singletons;

public interface IActivatableVm
{
    void Activate(ViewModel? previous);
}

public class ActivePanelVm : ViewModel
{
    private MainVm? _mvm;
    [Reactive] public ViewModel? ActivePanel { get; private set; }

    public void Focus(ViewModel? vm)
    {
        if (vm is IActivatableVm activatableVm)
        {
            activatableVm.Activate(ActivePanel);   
        }
        ActivePanel = vm ?? _mvm;
    }

    public void SetMainVm(MainVm mvm)
    {
        _mvm = mvm;
    }
}