using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI;
using Spriggit.UI.Settings;
using Spriggit.UI.ViewModels.Singletons;

namespace Spriggit.UI;

public partial class EditListingView
{
    public EditListingView()
    {
        InitializeComponent();
        this.WhenActivated(disp =>
        {
            this.Bind(ViewModel, x => x.LinkInput.Release, x => x.GameReleaseCombo.SelectedItem)
                .DisposeWith(disp);
            this.OneWayBind(ViewModel, x => x.LinkInput.Releases, x => x.GameReleaseCombo.ItemsSource)
                .DisposeWith(disp);
            this.WhenAnyValue(x => x.ViewModel!.LinkInput.ModPathPicker)
                .BindTo(this, x => x.ModPathPicker.PickerVM)
                .DisposeWith(disp);
            this.WhenAnyValue(x => x.ViewModel!.LinkInput.GitFolderPicker)
                .BindTo(this, x => x.GitFolderPicker.PickerVM)
                .DisposeWith(disp);
            this.Bind(ViewModel, x => x.LinkInput.PackageName, x => x.SpriggitPackageBox.Text)
                .DisposeWith(disp);
            this.Bind(ViewModel, x => x.LinkInput.Version, x => x.SpriggitVersionBox.Text)
                .DisposeWith(disp);
            this.Bind(ViewModel, x => x.FinishCommand, x => x.AddButton.Command)
                .DisposeWith(disp);
            this.Bind(ViewModel, x => x.DiscardCommand, x => x.CancelButton.Command)
                .DisposeWith(disp);
            this.WhenAnyValue(x => x.ViewModel)
                .Select(x => x is EditLinkVm ? "Apply" : "Add")
                .BindTo(this, x => x.AddButton.Content)
                .DisposeWith(disp);
            this.Bind(ViewModel, x => x.LinkInput.SourceCategory, x => x.TabControl.SelectedIndex,
                    x => (int)x,
                    x => (LinkSourceCategory)x)
                .DisposeWith(disp);
        });
    }
}