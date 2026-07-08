using System.Reactive.Disposables;
using Noggog;
using ReactiveUI;

namespace Spriggit.UI;

public partial class WindowView
{
    public WindowView()
    {
        InitializeComponent();
        this.WhenActivated(disposable =>
        {
            this.WhenAnyValue(x => x.ViewModel!.ActivePanelVm.ActivePanel)
                .BindTo(this, x => x.ContentPane.Content)
                .DisposeWith(disposable);
        });
    }
}