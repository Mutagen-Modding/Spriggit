using System.Reactive.Disposables;
using ReactiveUI;

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
            this.WhenAnyValue(x => x.ViewModel!.Links)
                .BindTo(this, x => x.LinkBox.ItemsSource)
                .DisposeWith(disp);
        });
    }
}