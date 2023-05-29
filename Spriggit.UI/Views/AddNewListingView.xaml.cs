using System.Reactive.Disposables;
using ReactiveUI;

namespace Spriggit.UI;

public partial class AddNewListingView
{
    public AddNewListingView()
    {
        InitializeComponent();
        this.WhenActivated(disp =>
        {
            this.WhenAnyValue(x => x.ViewModel!.LinkInput.ModPathPicker)
                .BindTo(this, x => x.ModPathPicker.PickerVM)
                .DisposeWith(disp);
            this.WhenAnyValue(x => x.ViewModel!.LinkInput.GitFolderPicker)
                .BindTo(this, x => x.GitFolderPicker.PickerVM)
                .DisposeWith(disp);
            this.WhenAnyValue(x => x.ViewModel!.AddCommand)
                .BindTo(this, x => x.AddButton.Command)
                .DisposeWith(disp);
        });
    }
}