using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Controls;
using System.Windows.Forms;
using ReactiveUI;
using Spriggit.UI.ViewModels.Transient;

namespace Spriggit.UI;

public partial class ReposListingView
{
    public ReposListingView()
    {
        InitializeComponent();
        this.WhenActivated(disp =>
        {
            this.WhenAnyValue(x => x.ViewModel!.AddNewLinkCommand)
                .BindTo(this, x => x.AddNewLinkButton.Command)
                .DisposeWith(disp);
            this.WhenAnyValue(x => x.ViewModel!.SyncSelectedToModCommand)
                .BindTo(this, x => x.SyncSelectedToModButton.Command)
                .DisposeWith(disp);
            this.WhenAnyValue(x => x.ViewModel!.SyncSelectedToGitCommand)
                .BindTo(this, x => x.SyncSelectedToGitButton.Command)
                .DisposeWith(disp);
            this.WhenAnyValue(x => x.ViewModel!.Links)
                .BindTo(this, x => x.LinkBox.ItemsSource)
                .DisposeWith(disp);
            Observable.FromEventPattern<SelectionChangedEventHandler, SelectionChangedEventArgs>(
                    x => LinkBox.SelectionChanged += x,
                    x => LinkBox.SelectionChanged -= x)
                .Subscribe(_ =>
                {
                    if (ViewModel == null) return;
                    ViewModel.SelectedLinks = LinkBox.SelectedItems.OfType<LinkVm>().ToArray();
                })
                .DisposeWith(disp);
        });
    }
}