using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;
using Noggog;
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
            this.Bind(ViewModel, x => x.LinkInput.OpenSpriggitConfigCommand, x => x.EditSpriggitConfigButton.Command)
                .DisposeWith(disp);
            this.Bind(ViewModel, x => x.LinkInput.OpenSpriggitConfigFolderCommand, x => x.GoToSpriggitConfigFolderButton.Command)
                .DisposeWith(disp);
            this.WhenAnyValue(x => x.ViewModel)
                .Select(x => x is EditLinkVm ? "Apply" : "Add")
                .BindTo(this, x => x.AddButton.Content)
                .DisposeWith(disp);
            this.Bind(ViewModel, x => x.LinkInput.SourceCategory, x => x.TabControl.SelectedIndex,
                    x => (int)x,
                    x => (LinkSourceCategory)x)
                .DisposeWith(disp);
            var isCustomVisibility = this.WhenAnyValue(x => x.ViewModel!.LinkInput.SourceCategory)
                .Select(x => x == LinkSourceCategory.Custom)
                .Select(x => x ? Visibility.Visible : Visibility.Collapsed);
            isCustomVisibility
                .BindTo(this, x => x.PackageTextBlock.Visibility)
                .DisposeWith(disp);
            isCustomVisibility
                .BindTo(this, x => x.SpriggitPackageBox.Visibility)
                .DisposeWith(disp);
            var hasConfig = this.WhenAnyValue(x => x.ViewModel!.LinkInput.SpriggitConfigPath)
                .Select(x => x != null);
            hasConfig
                .Select(has => !has)
                .BindTo(this, x => x.VersioningPane.IsEnabled)
                .DisposeWith(disp);
            hasConfig
                .Select(has => has ? Visibility.Visible : Visibility.Collapsed)
                .BindTo(this, x => x.SpriggitConfigActivePane.Visibility)
                .DisposeWith(disp);
            hasConfig
                .Select(has => has ? Visibility.Collapsed : Visibility.Visible)
                .BindTo(this, x => x.CreateSpriggitConfigPane.Visibility)
                .DisposeWith(disp);
            hasConfig
                .Select(has => has ? Color.FromArgb(0x11, 255, 255, 255) : Color.FromArgb(0, 0, 0, 0))
                .Select(x => new SolidColorBrush(x))
                .BindTo(this, x => x.VersioningPane.Background)
                .DisposeWith(disp);
            hasConfig
                .Select(has => has ? new Thickness(0,5,0,8) : new Thickness(0))
                .BindTo(this, x => x.VersioningPane.Margin)
                .DisposeWith(disp);
        });
    }
}