using System.Reactive.Linq;
using System.Windows.Input;
using Noggog;
using Noggog.WPF;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Spriggit.Engine;
using Spriggit.UI.ViewModels.Transient;

namespace Spriggit.UI.ViewModels.Singletons;

public class EditLinkVm : ViewModel, IActivatableVm, IEditLinkVm
{
    private readonly LinkInputVm _empty;
    
    public LinkInputVm LinkInput { get; private set; }
    
    private ViewModel? _previous;
    
    public ICommand FinishCommand { get; }
    public ICommand DiscardCommand { get; }

    [Reactive] public LinkInputVm? Target { get; set; }
    
    public EditLinkVm(
        LinkInputVm.Factory inputFactory,
        ActivePanelVm activePanelVm)
    {
        _empty = inputFactory();
        LinkInput = inputFactory();
        FinishCommand = ReactiveCommand.Create(
            execute: () =>
            {
                if (Target == null) return;
                Target.CopyFrom(LinkInput);
                activePanelVm.Focus(_previous);
                LinkInput.CopyFrom(_empty);
                Target = null;
            },
            canExecute: 
            this.WhenAnyValue(
                    x => x.LinkInput.InError,
                    x => x.Target)
                .Select(x => !x.Item1 || x.Item2 != null));

        DiscardCommand = ReactiveCommand.Create(() =>
        {
            activePanelVm.Focus(_previous);
        });

        this.WhenAnyValue(x => x.Target)
            .Subscribe(x =>
            {
                LinkInput.CopyFrom(x ?? _empty);
            })
            .DisposeWith(this);
    }
    
    public void Activate(ViewModel? previous)
    {
        _previous = previous;
    }
}